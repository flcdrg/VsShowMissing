using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using EnvDTE;
using EnvDTE80;
using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Project = EnvDTE.Project;
using ProjectItem = EnvDTE.ProjectItem;

namespace DavidGardiner.Gardiner_VsShowMissing
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    //[ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidGardiner_VsShowMissingPkgString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    [ProvideOptionPage(typeof(ShowMissingOptions), "Show Missing", "General", 101, 100, true, new[] { "File Nesting in Solution Explorer" })]
    public sealed class Gardiner_VsShowMissingPackage : Package, IVsSolutionEvents
    {
        private DTE _dte;
        private uint _solutionCookie;
        private IVsSolution _solution;
        private ErrorListProvider _errorListProvider;
        private BuildEvents _buildEvents;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require
        /// any Visual Studio service because at this point the package object is created but
        /// not sited yet inside Visual Studio environment. The place to do all the other
        /// initialization is the Initialize method.
        /// </summary>
        public Gardiner_VsShowMissingPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", ToString()));
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", ToString()));
            base.Initialize();

            // listen for solution events
            _solution = (IVsSolution)GetService(typeof(SVsSolution));
            ErrorHandler.ThrowOnFailure(_solution.AdviseSolutionEvents(this, out _solutionCookie));

            _dte = (DTE)GetService(typeof(SDTE));
            var events = _dte.Events;
            _buildEvents = events.BuildEvents;

            if (_errorListProvider == null)
                _errorListProvider = new TaskProvider(this);

            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => 
             { 
                 Options = (ShowMissingOptions)GetDialogPage(typeof(ShowMissingOptions)); 
             }), DispatcherPriority.ApplicationIdle, null); 

            _buildEvents.OnBuildProjConfigBegin += BuildEventsOnOnBuildProjConfigBegin;
            _buildEvents.OnBuildBegin += BuildEventsOnOnBuildBegin;
            _buildEvents.OnBuildDone += BuildEventsOnOnBuildDone;
        }

        public static ShowMissingOptions Options { get; private set; }

        private void BuildEventsOnOnBuildDone(vsBuildScope scope, vsBuildAction action)
        {
            Debug.WriteLine("BuildEventsOnOnBuildDone {0} {1}", scope, action);

            if (Options.Timing == RunWhen.AfterBuild)
                FindMissingFiles();
        }

        private void BuildEventsOnOnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            Debug.WriteLine("BuildEventsOnOnBuildBegin {0} {1}", scope, action);

            if (Options != null && Options.Timing == RunWhen.BeforeBuild)
                FindMissingFiles();
        }

        private void FindMissingFiles()
        {
            _errorListProvider.Tasks.Clear();

            var projects = Projects();
            foreach (Project proj in projects)
            {
                Debug.WriteLine(proj.Name);

                // Skip unloaded projects 
                if (proj.ConfigurationManager == null)
                    continue;

                IDictionary<string, string> dict = new Dictionary<string, string>();
                dict.Add("Configuration", proj.ConfigurationManager.ActiveConfiguration.ConfigurationName);
                using (var projectCollection = new ProjectCollection(dict))
                {
                    var buildProject = projectCollection.LoadProject(proj.FullName);

                    var physicalFiles = new HashSet<string>(new CaseInsensitiveEqualityComparer());
                    var logicalFiles = new HashSet<string>(new CaseInsensitiveEqualityComparer());
                    var physicalFileProjectMap = new Dictionary<string, string>();

                    NavigateProjectItems(proj.ProjectItems, buildProject, physicalFiles, logicalFiles,
                        new HashSet<string>(), physicalFileProjectMap);

                    if (Options.NotIncludedFiles)
                    {
                        
                        var errorCategory = Options.MessageLevel;

                        physicalFiles.ExceptWith(logicalFiles);

                        foreach (var file in physicalFiles)
                        {
                            Debug.WriteLine($"Physical file: {file}");

                            IVsHierarchy hierarchyItem;
                            string physicalFileProject = physicalFileProjectMap[file];
                            _solution.GetProjectOfUniqueName(physicalFileProject, out hierarchyItem);

                            var newError = new MissingErrorTask()
                            {
                                ErrorCategory = errorCategory,
                                Category = TaskCategory.BuildCompile,
                                Text = "MI0002 : File on disk is not included in project",
                                Document = file,
                                HierarchyItem = hierarchyItem,
                                ProjectPath = physicalFileProject,
                            };

                            newError.Navigate += SelectParentProjectInSolution;
                            Debug.WriteLine("\t\t** Missing");

                            _errorListProvider.Tasks.Add(newError);
                        }
                    }
                }
            }

            if (_errorListProvider.Tasks.Count > 0)
            {
                _errorListProvider.Show();

                if (Options.FailBuildOnError && Options.Timing == RunWhen.BeforeBuild)
                {
                    _dte.ExecuteCommand("Build.Cancel");
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_solutionCookie != 0)
            {
                _solution.UnadviseSolutionEvents(_solutionCookie);
                _solutionCookie = 0;
            }

            if (disposing)
            {
                _errorListProvider.Dispose();
            }

            base.Dispose(disposing);
        }

        private void BuildEventsOnOnBuildProjConfigBegin(string project, string projectConfig, string platform, string solutionConfig)
        {
            Debug.WriteLine($"BuildEventsOnOnBuildProjConfigBegin {project}");
        }

        private void NavigateProjectItems(ProjectItems projectItems, Microsoft.Build.Evaluation.Project buildProject, ISet<string> projectPhysicalFiles, ISet<string> projectLogicalFiles, ISet<string> processedPhysicalDirectories, IDictionary<string, string> physicalFileProjectMap  )
        {
            if (projectItems == null)
                return;

            var errorCategory = Options.MessageLevel;

            foreach (ProjectItem item in projectItems)
            {
                NavigateProjectItems(item.ProjectItems, buildProject, projectPhysicalFiles, projectLogicalFiles, processedPhysicalDirectories, physicalFileProjectMap);

                if (item.Kind != "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}") // VSConstants.GUID_ItemType_PhysicalFile
                    continue;

                Debug.WriteLine("\t" + item.Name);

                string projectFilename = item.ContainingProject.FileName;

                for (short i = 0; i < item.FileCount; i++)
                {
                    var filePath = item.FileNames[i];

                    projectLogicalFiles.Add(filePath);

                    // Skip if this is a linked file

                    if (!File.Exists(filePath))
                    {
                        var relativePath = filePath.Replace(buildProject.DirectoryPath + @"\", "");

                        var found = buildProject.Items.Any(x => x.EvaluatedInclude == relativePath);

                        if (!found)
                        {
                            Debug.WriteLine("\t\tFile excluded due to Condition evaluation");
                            return;
                        }

                        IVsHierarchy hierarchyItem;
                        _solution.GetProjectOfUniqueName(projectFilename, out hierarchyItem);

                        var newError = new MissingErrorTask()
                        {
                            ErrorCategory = errorCategory,
                            Category = TaskCategory.BuildCompile,
                            Text = "MI0001 : File referenced in project does not exist",
                            Document = filePath,
                            HierarchyItem = hierarchyItem,
                        };

                        newError.Navigate += NewErrorOnNavigate;
                        Debug.WriteLine("\t\t** Missing");

                        _errorListProvider.Tasks.Add(newError);
                    }

                    string directoryName = Path.GetDirectoryName(filePath);

                    // If we haven't seen this directory before, find the files inside it
                    if (processedPhysicalDirectories.Add(directoryName))
                    {
                        var physicalFiles =
                            new DirectoryInfo(directoryName).GetFiles()
                                .Where(
                                    f => f.Attributes != FileAttributes.Hidden && f.Attributes != FileAttributes.System)
                                .Where(f => !f.Name.EndsWith(".user") && !f.Name.EndsWith("proj"))
                                .Select(f => f.FullName)
                                .ToList();

                        foreach (var physicalFile in physicalFiles)
                        {
                            projectPhysicalFiles.Add(physicalFile);
                            physicalFileProjectMap.Add(physicalFile, projectFilename);
                        }
                    }
                }
            }
        }

        private void SelectParentProjectInSolution(object sender, EventArgs e)
        {
            var error = (MissingErrorTask)sender;

            var projectItem = _dte.Solution.FindProjectItem(error.ProjectPath);
            SelectItemInSolutionExplorer(projectItem);
        }

        private void NewErrorOnNavigate(object sender, EventArgs eventArgs)
        {
            Debug.WriteLine(sender);
            var error = (ErrorTask)sender;

            var projectItem = _dte.Solution.FindProjectItem(error.Document);
            SelectItemInSolutionExplorer(projectItem);
        }

        private void SelectItemInSolutionExplorer(ProjectItem projectItem)
        {
            if (projectItem != null)
            {
                var uih = (UIHierarchy) _dte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Object;
                UIHierarchyItem uiHierarchyItem = uih.FindHierarchyItem(projectItem);

                uiHierarchyItem?.Select(vsUISelectionType.vsUISelectionTypeSelect);
            }
        }

        private IList<Project> Projects()
        {
            Projects projects = _dte.Solution.Projects;
            List<Project> list = new List<Project>();
            var item = projects.GetEnumerator();
            while (item.MoveNext())
            {
                var project = item.Current as Project;
                if (project == null)
                {
                    continue;
                }

                if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    list.AddRange(GetSolutionFolderProjects(project));
                }
                else
                {
                    list.Add(project);
                }
            }

            return list;
        }

        private static IEnumerable<Project> GetSolutionFolderProjects(Project solutionFolder)
        {
            List<Project> list = new List<Project>();
            for (var i = 1; i <= solutionFolder.ProjectItems.Count; i++)
            {
                var subProject = solutionFolder.ProjectItems.Item(i).SubProject;
                if (subProject == null)
                {
                    continue;
                }

                // If this is another solution folder, do a recursive call, otherwise add
                if (subProject.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    list.AddRange(GetSolutionFolderProjects(subProject));
                }
                else
                {
                    list.Add(subProject);
                }
            }
            return list;
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            _errorListProvider.Tasks.Clear();

            return VSConstants.S_OK;
        }
    }
}
