using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

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
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }



        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // listen for solution events
            _solution = (IVsSolution)GetService(typeof(SVsSolution));
            ErrorHandler.ThrowOnFailure(_solution.AdviseSolutionEvents(this, out _solutionCookie));

            _dte = (DTE)GetService(typeof(SDTE));
            var events = _dte.Events;
            _buildEvents = events.BuildEvents;

            _buildEvents.OnBuildProjConfigBegin += BuildEventsOnOnBuildProjConfigBegin;
        }

        protected override void Dispose(bool disposing)
        {
            if (_solutionCookie != 0)
            {
                _solution.UnadviseSolutionEvents(_solutionCookie);
                _solutionCookie = 0;
            }

            base.Dispose(disposing);
        }

        private void BuildEventsOnOnBuildProjConfigBegin(string project, string projectConfig, string platform, string solutionConfig)
        {
            if (_errorListProvider == null)
            _errorListProvider = new ErrorListProvider(this);

            _errorListProvider.Tasks.Clear();

            foreach (Project proj in _dte.Solution.Projects)
            {
                foreach (ProjectItem item in proj.ProjectItems)
                {
                    if (item.Kind != "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}")
                        continue;

                    for (short i = 0; i < item.FileCount; i++)
                    {
                        var path = item.FileNames[i];
                        if (!File.Exists(path))
                        {
                            // WriteToBuildWindow(string.Format("Missing file: {0} in project {1}", path, proj.Name));
                            IVsHierarchy heirarchyItem;
                            _solution.GetProjectOfUniqueName(proj.FileName, out heirarchyItem);

                            var newError = new ErrorTask()
                            {
                                ErrorCategory = TaskErrorCategory.Error,
                                Category = TaskCategory.BuildCompile,
                                Text = "File referenced in project does not exist",
                                Document = path,
                                HierarchyItem = heirarchyItem
                            };

                            _errorListProvider.Tasks.Add(newError);
                        }
                    }
                }
            }

            if (_errorListProvider.Tasks.Count > 0)
                _errorListProvider.Show();
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
            return VSConstants.S_OK;
        }
    }
}
