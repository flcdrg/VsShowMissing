using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace Gardiner.VsShowMissing
{
    sealed class ExcludeFileCommand : ErrorListCommand
    {
        private ExcludeFileCommand(
            OleMenuCommandService commandService,
            DTE dte, 
            ErrorListProvider errorListProvider) 
            : base(dte, errorListProvider)
        {
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(PackageGuids.guidGardiner_ErrorListCmdSet, PackageIds.cmdidExcludeFileFromProject);
            var menuItem = new OleMenuCommand(Execute, menuCommandID);
            menuItem.BeforeQueryStatus += MenuItemOnBeforeQueryStatus;
            commandService.AddCommand(menuItem);
        }

        private void Execute(object sender, EventArgs eventArgs)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var tasks = MissingErrorTasks(Constants.FileInProjectNotOnDisk);
            var physicalFile = VSConstants.GUID_ItemType_PhysicalFile.ToString("B", CultureInfo.InvariantCulture).ToUpperInvariant();

            ThreadHelper.ThrowIfNotOnUIThread();
            var projects = Dte.AllProjects();
            foreach (var task in tasks)
            {
#pragma warning disable VSTHRD010
                var project =
                    projects.FirstOrDefault(
                        p => p.FullName.Equals(task.ProjectPath, StringComparison.InvariantCultureIgnoreCase));
#pragma warning restore VSTHRD010

                if (project != null)
                {
                    Debug.WriteLine($"Removing {task.Document} from {project.FullName}");

                    foreach (ProjectItem projectItem in project.ProjectItems)
                    {                        
                        if (projectItem.Kind.Equals(physicalFile, StringComparison.InvariantCultureIgnoreCase) && projectItem.FileNames[0].Equals(task.Document, StringComparison.InvariantCultureIgnoreCase))
                        {
                            projectItem.Remove();
                        }
                    }

                    RemoveTask(task);
                }
            }
        }

        protected override bool VisibleExpression(MissingErrorTask task)
        {
            return (task != null && task.Code == Constants.FileInProjectNotOnDisk);
        }

        public static ExcludeFileCommand Instance { get; private set; }

        public static void Initialize(IServiceProvider serviceProvider, ErrorListProvider errorListProvider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var commandService = (OleMenuCommandService)serviceProvider.GetService(typeof(IMenuCommandService));
            var dte = (DTE)serviceProvider.GetService(typeof(DTE));

            Instance = new ExcludeFileCommand(commandService, dte, errorListProvider);
        }

#if VS2019
        internal static async System.Threading.Tasks.Task InitializeAsync(AsyncPackage package, ErrorListProvider errorListProvider)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = (OleMenuCommandService) await package.GetServiceAsync(typeof(IMenuCommandService));
            var dte = (DTE) await package.GetServiceAsync(typeof(DTE));

            Instance = new ExcludeFileCommand(commandService, dte, errorListProvider);
        }
#endif
    }
}