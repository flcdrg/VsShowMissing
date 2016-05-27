using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;

namespace DavidGardiner.Gardiner_VsShowMissing
{
    internal class IncludeFileCommand : BaseMissingCommand
    {
        private IncludeFileCommand(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public static IncludeFileCommand Instance { get; private set; }

        public static void Initialize(IServiceProvider provider)
        {
            Instance = new IncludeFileCommand(provider);
        }

        protected override void SetupCommands()
        {
            AddCommand(PackageGuids.guidGardiner_ErrorListCmdSet, PackageIds.cmdidIncludeFileInProject, InvokeHandler, AddCustomToolItemBeforeQueryStatus);
        }

        protected override void InvokeHandler(object sender, EventArgs eventArgs)
        {
            Window window = DTE.Windows.Item(WindowKinds.vsWindowKindErrorList);
            var myErrorList = (IVsTaskList2)window.Object;

            int count;
            myErrorList.GetSelectionCount(out count);

            var tasks = new List<MissingErrorTask>();

                ForEachTask(task =>
            {
                var item = task as MissingErrorTask;

                if (item != null)
                {
                    tasks.Add(item);
                }
            });

            var projects = ((DTE) DTE).AllProjects();
            foreach (var task in tasks)
            {
                var project =
                    projects.FirstOrDefault(
                        p => p.FullName.Equals(task.ProjectPath, StringComparison.InvariantCultureIgnoreCase));

                if (project != null)
                {
                    Debug.WriteLine($"Adding {task.Document} to {project.FullName}");
                    project.ProjectItems.AddFromFile(task.Document);
                }
            }
        }
  
        protected override bool VisibleExpression(MissingErrorTask task)
        {
            return (task == null || task.Code != "MI0002");
        }

        private Project FindProject(Projects projects, string projectFile)
        {
            foreach (Project project in projects)
            {
                if (string.Equals(project.FullName, projectFile, StringComparison.InvariantCultureIgnoreCase))
                    return project;
            }

            foreach (Project project in projects)
            {
                // find child projects
                foreach (ProjectItem projectItem in project.ProjectItems)
                {
                    Debug.WriteLine(projectItem);
                }
            }

            return null;
        }
    }
}