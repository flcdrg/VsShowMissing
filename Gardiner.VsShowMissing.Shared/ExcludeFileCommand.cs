using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

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

            var menuCommandID = new CommandID(PackageGuids.VS2022, PackageIds.cmdidExcludeFileFromProject);
            var menuItem = new OleMenuCommand(Execute, menuCommandID);
            menuItem.BeforeQueryStatus += MenuItemOnBeforeQueryStatus;
            commandService.AddCommand(menuItem);
        }

        private void RemoveDocument(ProjectItems projectItems, string fileName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if ( projectItems != null)
            {
                foreach (ProjectItem projectItem in projectItems)
                {
                    var physicalFile = VSConstants.GUID_ItemType_PhysicalFile.ToString("B", CultureInfo.InvariantCulture).ToUpperInvariant();
                    if (projectItem.Kind.Equals(physicalFile, StringComparison.InvariantCultureIgnoreCase) && projectItem.FileNames[0].Equals(fileName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        projectItem.Remove();
                        return;
                    }
                    RemoveDocument(projectItem.ProjectItems, fileName);
                }
            }
        }

        private void Execute(object sender, EventArgs eventArgs)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var tasks = MissingErrorTasks(Constants.FileInProjectNotOnDisk);

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
                    RemoveDocument(project.ProjectItems, task.Document);
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

#if VS2019 || VS2022
        internal static async System.Threading.Tasks.Task InitializeAsync(AsyncPackage package, ErrorListProvider errorListProvider)
        {
            await package.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = (OleMenuCommandService) await package.GetServiceAsync(typeof(IMenuCommandService));
            var dte = (DTE) await package.GetServiceAsync(typeof(DTE));

            Instance = new ExcludeFileCommand(commandService, dte, errorListProvider);
        }
#endif
    }
}