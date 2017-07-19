using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace DavidGardiner.Gardiner_VsShowMissing
{
    class ExcludeFileCommand : BaseMissingCommand
    {
        public ExcludeFileCommand(IServiceProvider serviceProvider, ErrorListProvider errorListProvider) : base(serviceProvider, errorListProvider)
        {
        }

        protected override void SetupCommands()
        {
            AddCommand(PackageGuids.guidGardiner_ErrorListCmdSet, PackageIds.cmdidExcludeFileFromProject, InvokeHandler,
                AddCustomToolItemBeforeQueryStatus);
        }

        protected override void InvokeHandler(object sender, EventArgs eventArgs)
        {
            var tasks = MissingErrorTasks(Constants.FileInProjectNotOnDisk);
            var physicalFile = VSConstants.GUID_ItemType_PhysicalFile.ToString("B", CultureInfo.InvariantCulture).ToUpperInvariant();

            var projects = ((DTE) DTE).AllProjects();
            foreach (var task in tasks)
            {
                var project =
                    projects.FirstOrDefault(
                        p => p.FullName.Equals(task.ProjectPath, StringComparison.InvariantCultureIgnoreCase));

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
            Instance = new ExcludeFileCommand(serviceProvider, errorListProvider);
        }
    }
}