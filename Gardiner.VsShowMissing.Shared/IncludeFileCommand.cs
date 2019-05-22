using System;
using System.Diagnostics;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace DavidGardiner.Gardiner_VsShowMissing
{
    internal class IncludeFileCommand : BaseMissingCommand
    {
        private IncludeFileCommand(IServiceProvider serviceProvider, ErrorListProvider errorListProvider)
            : base(serviceProvider, errorListProvider)
        {
        }

        public static IncludeFileCommand Instance { get; private set; }

        public static void Initialize(IServiceProvider provider, ErrorListProvider errorListProvider)
        {
            Instance = new IncludeFileCommand(provider, errorListProvider);
        }

        protected override void SetupCommands()
        {
            AddCommand(PackageGuids.guidGardiner_ErrorListCmdSet, PackageIds.cmdidIncludeFileInProject, InvokeHandler, AddCustomToolItemBeforeQueryStatus);
        }

        protected override void InvokeHandler(object sender, EventArgs eventArgs)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var tasks = MissingErrorTasks(Constants.FileOnDiskNotInProject);

            var projects = ((DTE) DTE).AllProjects();
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
    }
}