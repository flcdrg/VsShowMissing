using System;
using System.Collections.Generic;
using EnvDTE;
using EnvDTE80;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Shell;

namespace Gardiner.VsShowMissing
{
    public static class ProjectExtensions
    {
        public static IList<Project> AllProjects([NotNull] this DTE dte, bool excludeUnloaded = true)
        {
            if (dte == null)
            {
                throw new ArgumentNullException(nameof(dte));
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            Projects projects = dte.Solution.Projects;
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
                else if (excludeUnloaded && project.ConfigurationManager == null)
                {
                    // skip unloaded
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
            ThreadHelper.ThrowIfNotOnUIThread();

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
    }
}