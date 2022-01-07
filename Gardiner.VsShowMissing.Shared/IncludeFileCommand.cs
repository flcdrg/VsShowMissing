using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace Gardiner.VsShowMissing
{
    internal class IncludeFileCommand : ErrorListCommand
    {
        private IncludeFileCommand(
            OleMenuCommandService commandService,
            DTE dte,
            ErrorListProvider errorListProvider)
            : base(dte, errorListProvider)
        {
            if (commandService == null)
            {
                throw new ArgumentNullException(nameof(commandService));
            }

            var menuCommandID = new CommandID(PackageGuids.VS2022, PackageIds.cmdidIncludeFileInProject);
            var menuItem = new OleMenuCommand(Execute, menuCommandID);
            menuItem.BeforeQueryStatus += MenuItemOnBeforeQueryStatus;
            commandService.AddCommand(menuItem);
        }

        public static IncludeFileCommand Instance { get; private set; }

        private void Execute(object sender, EventArgs eventArgs)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var tasks = MissingErrorTasks(Constants.FileOnDiskNotInProject);

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
                    Debug.WriteLine($"Adding {task.Document} to {project.FullName}");
                    project.ProjectItems.AddFromFile(task.Document);
                    RemoveTask(task);
                }
            }
        }

        protected override bool VisibleExpression(MissingErrorTask task)
        {
            return (task != null && task.Code == Constants.FileOnDiskNotInProject);
        }

        public static void Initialize(IServiceProvider serviceProvider, ErrorListProvider errorListProvider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var commandService = (OleMenuCommandService)serviceProvider.GetService(typeof(IMenuCommandService));
            var dte = (DTE)serviceProvider.GetService(typeof(DTE));

            Instance = new IncludeFileCommand(commandService, dte, errorListProvider);
        }

#if VS2019 || VS2022
        internal static async System.Threading.Tasks.Task InitializeAsync(AsyncPackage package, ErrorListProvider errorListProvider)
        {
            await package.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = (OleMenuCommandService)await package.GetServiceAsync(typeof(IMenuCommandService));
            var dte = (DTE)await package.GetServiceAsync(typeof(DTE));

            Instance = new IncludeFileCommand(commandService, dte, errorListProvider);
        }
#endif
    }
}