using System;
using System.Collections.Generic;
using System.Diagnostics;

using Community.VisualStudio.Toolkit;

using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

#if VS2019
using TaskListItem = Microsoft.VisualStudio.Shell.Task;
#else
using TaskListItem = Microsoft.VisualStudio.Shell.TaskListItem;
#endif
namespace Gardiner.VsShowMissing
{
    abstract class ErrorListCommand
    {
        protected DTE Dte { get; }
        private readonly ErrorListProvider _errorListProvider;

        protected ErrorListCommand(DTE dte, ErrorListProvider errorListProvider)
        {
            Dte = dte ?? throw new ArgumentNullException(nameof(dte));
            _errorListProvider = errorListProvider ?? throw new ArgumentNullException(nameof(errorListProvider));
        }

        protected void ForEachTask(Action<object> action)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Window window = Dte.Windows.Item(WindowKinds.vsWindowKindErrorList);
            var myErrorList = (IVsTaskList2)window.Object;

            myErrorList.EnumSelectedItems(out var itemEnumerator);

            if (itemEnumerator == null)
            {
                // Probably too early or list is empty?
                Debug.WriteLine("too early");
                return;
            }

            uint[] fetched = { 0 };
            IVsTaskItem[] items = { null };
#pragma warning disable S1264
            for (itemEnumerator.Reset(); itemEnumerator.Next(1, items, fetched) == VSConstants.S_OK && fetched[0] == 1;)
            {
                action(items[0]);
            }
#pragma warning restore S1264
        }

        protected List<MissingErrorTask> MissingErrorTasks(string code)
        {
            var tasks = new List<MissingErrorTask>();
            ThreadHelper.ThrowIfNotOnUIThread();
            ForEachTask(task =>
            {
                if (task is MissingErrorTask item && item.Code == code)
                {
                    tasks.Add(item);
                }
            });
            return tasks;
        }

        protected void RemoveTask(TaskListItem task)
        {
            _errorListProvider.Tasks.Remove(task);
        }

        protected void MenuItemOnBeforeQueryStatus(object sender, EventArgs e)
        {
            var menuItem = (OleMenuCommand)sender;

            Debug.WriteLine($"MenuItemOnBeforeQueryStatus {menuItem.Text}");

            ThreadHelper.ThrowIfNotOnUIThread();
            menuItem.Visible = CalculateVisible();
        }

        protected abstract bool VisibleExpression(MissingErrorTask task);

        private bool CalculateVisible()
        {
            int misMatched = 0;
            ThreadHelper.ThrowIfNotOnUIThread();

            ForEachTask(item =>
            {
                var task = item as MissingErrorTask;

                // Visible if we've only selected this kind of item.
                if (!VisibleExpression(task))
                {
                    misMatched++;
                }
            });

            var visible = misMatched == 0;

            Debug.WriteLine($"Visible {visible}");
            return visible;
        }
    }
}
