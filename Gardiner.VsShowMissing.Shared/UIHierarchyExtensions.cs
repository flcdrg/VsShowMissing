using System;
using System.Collections;
using EnvDTE;

namespace DavidGardiner.Gardiner_VsShowMissing
{
#pragma warning disable S101 // Types should be named in camel case
    /// <summary>
    ///     Extends functionality for UIHierarchy classes, e.g:
    ///     VisualStudioServices.Dte.ToolWindows.SolutionExplorer
    /// </summary>
    /// <remarks>
    /// From https://gist.github.com/anonymous/1862526
    /// </remarks>
    internal static class UIHierarchyExtensions
#pragma warning restore S101 // Types should be named in camel case
    {
        /// <summary>
        ///     Finds the hierarchy item for the given item.
        /// </summary>
        /// <param name="hierarchy">The hierarchy object</param>
        /// <param name="item">The item.</param>
        /// <returns>
        ///     The found UIHierarchyItem, null if none found. For example, in case of the Solution explorer, it would be the item
        ///     in the solution
        ///     explorer that represents the given project
        /// </returns>
        public static UIHierarchyItem FindHierarchyItem(this UIHierarchy hierarchy, Project item)
        {
            return FindHierarchyItem(hierarchy, (object) item);
        }

        /// <summary>
        ///     <see cref="FindHierarchyItem(EnvDTE.UIHierarchy,EnvDTE.Project)" />
        /// </summary>
        public static UIHierarchyItem FindHierarchyItem(this UIHierarchy hierarchy, Solution item)
        {
            return FindHierarchyItem(hierarchy, (object) item);
        }

        /// <summary>
        ///     <see cref="FindHierarchyItem(EnvDTE.UIHierarchy,EnvDTE.Project)" />
        /// </summary>
        public static UIHierarchyItem FindHierarchyItem(this UIHierarchy hierarchy, ProjectItem item)

        {
            return FindHierarchyItem(hierarchy, (object) item);
        }

        private static UIHierarchyItem FindHierarchyItem(UIHierarchy hierarchy, object item)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            // This gets children of the root note in the hierarchy
            var items = hierarchy.UIHierarchyItems.Item(1).UIHierarchyItems;

            // Finds the given item in the hierarchy
            var uiItem = FindHierarchyItem(items, item);

            // uiItem would be null in most cases, however, for projects inside Solution Folders, there is a strange behavior in which the project byitself can't
            // be found in the hierarchy. Instead, in case of failure we'll search for the UIHierarchyItem
            if (uiItem == null && item is Project && ((Project) item).ParentProjectItem != null)
                uiItem = FindHierarchyItem(items, ((Project) item).ParentProjectItem);

            return uiItem;
        }

        /// <summary>
        ///     Enumerating children recursive would work, but it may be slow on large solution.
        ///     This tries to be smarter and faster
        /// </summary>
        private static UIHierarchyItem FindHierarchyItem(UIHierarchyItems items, object item)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            // This creates the full hierarchy for the given item
            var itemHierarchy = new Stack();
            CreateItemHierarchy(itemHierarchy, item);

            // Now that we have the items complete hierarchy, we assume that the item's hierarchy is a subset of the full heirarchy of the given
            // items variable. So we are going to go through every level of the given items and compare it to the matching level of itemHierarchy
            UIHierarchyItem last = null;

            while (itemHierarchy.Count != 0)
            {
                // Visual Studio would sometimes not recognize the children of a node in the hierarchy since its not expanded and thus not loaded.
                if (!items.Expanded)
                {
                    items.Expanded = true;
                }

                if (!items.Expanded)
                {
                    //Expand dont always work without this fix
                    var parent = ((UIHierarchyItem) items.Parent);
                    parent.Select(vsUISelectionType.vsUISelectionTypeSelect);
                }


                // We're popping the top ancestors first and each time going deeper until we reach the original item
                var itemOrParent = itemHierarchy.Pop();

                last = null;

                foreach (UIHierarchyItem child in items)
                {
                    if (child.Object == itemOrParent)
                    {
                        last = child;
                        items = child.UIHierarchyItems;
                        break;
                    }
                }
            }

            return last;
        }

        /// <summary>
        ///     Creates recursively the hierarchy for the given item.
        ///     Returns the complete hierarchy.
        /// </summary>
        private static void CreateItemHierarchy(Stack itemHierarchy, object item)
        {
            var projectItem = item as ProjectItem;
            if (projectItem != null)
            {
                var pi = projectItem;
                itemHierarchy.Push(pi);
                CreateItemHierarchy(itemHierarchy, pi.Collection.Parent);
            }

            else
            {
                var project = item as Project;
                if (project != null)
                {
                    var p = project;

                    itemHierarchy.Push(p);

                    if (p.ParentProjectItem != null)
                    {
                        //top nodes dont have solution as parent, but is null 
                        CreateItemHierarchy(itemHierarchy, p.ParentProjectItem);
                    }
                }

                else if (item is Solution)
                {
                    // doesn't seem to ever happen... 
                }

                else
                {
                    throw new InvalidOperationException("Unknown item");
                }
            }
        }
    }
}