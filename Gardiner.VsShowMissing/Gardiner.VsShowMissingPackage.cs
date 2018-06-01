﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using DavidGardiner.Gardiner_VsShowMissing.Options;
using EnvDTE;
using MAB.DotIgnore;
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
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.guidGardiner_VsShowMissingPkgString)]
    [ProvideBindingPath] // Allow assembly references to be located
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    [ProvideOptionPage(typeof(OptionsDialogPage), "Show Missing", "General", 101, 100, true, new[] { "Show missing files" })]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
#pragma warning disable S101 // Types should be named in camel case
#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
    public sealed class Gardiner_VsShowMissingPackage : Package, IVsSolutionEvents
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
#pragma warning restore CA1707 // Identifiers should not contain underscores
#pragma warning restore S101 // Types should be named in camel case
    {
        private DTE _dte;
        private uint _solutionCookie;
        private IVsSolution _solution;
        private ErrorListProvider _errorListProvider;
        private BuildEvents _buildEvents;
        private IList<Project> _projects;
        private string _solutionDirectory;
        private List<Regex> _filters;
        private readonly Dictionary<string, IgnoreList> _gitignores;

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
            _gitignores = new Dictionary<string, IgnoreList>();
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

#if MEMPROFILER
            RedGate.MemoryProfiler.Snapshot.TakeSnapshot("Initialize");
#endif


            // listen for solution events
            _solution = (IVsSolution)GetService(typeof(SVsSolution));
            ErrorHandler.ThrowOnFailure(_solution.AdviseSolutionEvents(this, out _solutionCookie));

            if (_errorListProvider == null)
                _errorListProvider = new TaskProvider(this);

            // Commands
            IncludeFileCommand.Initialize(this, _errorListProvider);
            DeleteFileCommand.Initialize(this, _errorListProvider);
            ExcludeFileCommand.Initialize(this, _errorListProvider);

            _dte = (DTE)GetService(typeof(SDTE));
            var events = _dte.Events;
            _buildEvents = events.BuildEvents;

            Options = (OptionsDialogPage)GetDialogPage(typeof(OptionsDialogPage));

            _buildEvents.OnBuildProjConfigBegin += BuildEventsOnOnBuildProjConfigBegin;
            _buildEvents.OnBuildProjConfigDone += BuildEventsOnBuildProjConfigDone;
            _buildEvents.OnBuildBegin += BuildEventsOnOnBuildBegin;
            _buildEvents.OnBuildDone += BuildEventsOnOnBuildDone;
        }

#pragma warning disable CA1801 // Review unused parameters
        private void BuildEventsOnBuildProjConfigDone(string project, string projectConfig, string platform, string solutionConfig, bool success)
#pragma warning restore CA1801 // Review unused parameters
        {
            Debug.WriteLine($"BuildEventsOnBuildProjConfigDone {project} {projectConfig} {platform} {solutionConfig} {success}");

            if (Options.Timing == RunWhen.AfterBuild)
            {
                var proj = GetProject(project);
                FindMissingFiles(proj);
            }

            if (_errorListProvider.Tasks.Count > 0 && Options.FailBuildOnError && Options.Timing == RunWhen.BeforeBuild)
            {
                _dte.ExecuteCommand("Build.Cancel");
            }
        }

        public static OptionsDialogPage Options { get; private set; }

#pragma warning disable CA1801 // Review unused parameters
        private void BuildEventsOnOnBuildProjConfigBegin(string project, string projectConfig, string platform, string solutionConfig)
#pragma warning restore CA1801 // Review unused parameters
        {
            Debug.WriteLine($"BuildEventsOnOnBuildProjConfigBegin {project} {projectConfig} {platform} {solutionConfig}");

            if (Options.Timing == RunWhen.BeforeBuild)
            {
                var proj = GetProject(project);
                FindMissingFiles(proj);
            }
        }

        private Project GetProject(string project)
        {
            var projectPath = Path.Combine(_solutionDirectory, project);
            var proj = _projects.FirstOrDefault(p => p.FullName == projectPath);
            return proj;
        }

        private void BuildEventsOnOnBuildDone(vsBuildScope scope, vsBuildAction action)
        {
            Debug.WriteLine("BuildEventsOnOnBuildDone {0} {1}", scope, action);

            _projects = null;
            _solutionDirectory = null;

            if (_errorListProvider.Tasks.Count > 0)
            {
                _errorListProvider.Show();
            }
        }

        private void BuildEventsOnOnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            Debug.WriteLine("BuildEventsOnOnBuildBegin {0} {1}", scope, action);

            _projects = _dte.AllProjects();
            _solutionDirectory = Path.GetDirectoryName(_dte.Solution.FullName);

            // unhook event handlers to reduce risk of memory leaks
            foreach (MissingErrorTask task in _errorListProvider.Tasks)
            {
                task.Navigate -= SelectParentProjectInSolution;
                task.Navigate -= NewErrorOnNavigate;
            }

            _errorListProvider.Tasks.Clear();

            _gitignores.Clear();
            AddGitIgnoreFromDirectory(_solutionDirectory);

            _filters = new List<Regex>();

            if (!string.IsNullOrEmpty(Options.IgnorePhysicalFiles))
            {
                _filters.AddRange(Options.IgnorePhysicalFiles.Split(new[] { "\r\n" },
                    StringSplitOptions.RemoveEmptyEntries).Select(p => FindFilesPatternToRegex.Convert(p.Trim())));
            }
        }

        private void FindMissingFiles(Project proj)
        {
            Debug.WriteLine($"Project {proj.Name}");

            IDictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("Configuration", proj.ConfigurationManager.ActiveConfiguration.ConfigurationName);
            using (var projectCollection = new ProjectCollection(dict))
            {
                var buildProject = projectCollection.LoadProject(proj.FullName);

                var physicalFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var logicalFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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

                        if (_filters.Any(f => f.IsMatch(file)) || (CheckIgnored(file)))
                        {
                            Debug.WriteLine("\tIgnored by filter");
                            continue;
                        }

                        IVsHierarchy hierarchyItem;
                        string physicalFileProject = physicalFileProjectMap[file];
                        _solution.GetProjectOfUniqueName(physicalFileProject, out hierarchyItem);

                        var newError = new MissingErrorTask()
                        {
                            ErrorCategory = errorCategory,
                            Category = TaskCategory.BuildCompile,
                            Text = "File on disk is not included in project",
                            Code = Constants.FileOnDiskNotInProject,
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

        private bool CheckIgnored(string file)
        {
            if (!Options.UseGitIgnore)
            {
                return false;
            }

            var directory = Path.GetDirectoryName(file);

            // Chance we're linking to a file outside our hierarchy
            while (directory != null && directory.StartsWith(_solutionDirectory, StringComparison.CurrentCultureIgnoreCase))
            {
                var truncatedFile = file.Substring(directory.Length + 1); // allow for removing separator
                if (_gitignores.ContainsKey(directory) && _gitignores[directory].IsIgnored(truncatedFile, false))
                {
                    return true;
                }

                // move up to parent
                directory = Path.GetDirectoryName(directory);
            }

            if (directory != null && _gitignores.ContainsKey(directory))
            {
                return _gitignores[directory].IsIgnored(file, false);
            }

            return false;
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

        private void NavigateProjectItems(ProjectItems projectItems, Microsoft.Build.Evaluation.Project buildProject, ISet<string> projectPhysicalFiles, ISet<string> projectLogicalFiles, ISet<string> processedPhysicalDirectories, IDictionary<string, string> physicalFileProjectMap  )
        {
            if (projectItems == null)
                return;

            var projectDirectory = buildProject.DirectoryPath + Path.DirectorySeparatorChar;
            var projectFilename = buildProject.FullPath;

            var errorCategory = Options.MessageLevel;

            foreach (ProjectItem item in projectItems)
            {
                NavigateProjectItems(item.ProjectItems, buildProject, projectPhysicalFiles, projectLogicalFiles, processedPhysicalDirectories, physicalFileProjectMap);

                if (item.Kind != "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}") // VSConstants.GUID_ItemType_PhysicalFile
                    continue;

                string itemName = item.Name;
                Debug.WriteLine("\t" + itemName + item.Kind);

                for (short i = 0; i < item.FileCount; i++)
                {           
                    var filePath = item.FileNames[i];

                    // Skip if this is a linked file
                    if (!filePath.StartsWith(projectDirectory, StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    projectLogicalFiles.Add(filePath);

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
                            Text = "File referenced in project does not exist",
                            Code = "MI0001",
                            Document = filePath,
                            HierarchyItem = hierarchyItem,
                            ProjectPath = projectFilename
                        };

                        newError.Navigate += NewErrorOnNavigate;
                        Debug.WriteLine("\t\t** Missing");

                        _errorListProvider.Tasks.Add(newError);
                    }

                    string directoryName = Path.GetDirectoryName(filePath);

                    // If we haven't seen this directory before, find the files inside it
                    if (processedPhysicalDirectories.Add(directoryName))
                    {
                        AddGitIgnoreFromDirectory(directoryName);

                        var physicalFiles =
                            new DirectoryInfo(directoryName).GetFiles()
                                .Where(
                                    f => f.Attributes != FileAttributes.Hidden && f.Attributes != FileAttributes.System)
                                .Where(f => !f.Name.EndsWith(".user", StringComparison.InvariantCultureIgnoreCase) 
                                    && !f.Name.EndsWith("proj", StringComparison.InvariantCultureIgnoreCase))
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

        private void AddGitIgnoreFromDirectory(string directoryName)
        {
            var gitIgnoreFile = Path.Combine(directoryName, ".gitignore");
            if (Options.UseGitIgnore && File.Exists(gitIgnoreFile))
            {
                _gitignores.Add(directoryName, new IgnoreList(gitIgnoreFile));
            }
        }

        private void SelectParentProjectInSolution(object sender, EventArgs e)
        {
            var error = (MissingErrorTask)sender;

            var project = _dte.AllProjects()
                .FirstOrDefault(p => p.FullName.Equals(error.ProjectPath, StringComparison.InvariantCultureIgnoreCase));

            if (project != null)
                SelectItemInSolutionExplorer(project.ParentProjectItem);
        }

        private void NewErrorOnNavigate(object sender, EventArgs eventArgs)
        {
            Debug.WriteLine($"NewErrorOnNavigate {sender}");
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
