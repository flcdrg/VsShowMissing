using System;
using System.Collections.Generic;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace DavidGardiner.Gardiner_VsShowMissing
{
    internal abstract class BaseMissingCommand : BaseCommand
    {
        private readonly ErrorListProvider _errorListProvider;

        protected BaseMissingCommand(IServiceProvider serviceProvider, ErrorListProvider errorListProvider) : base(serviceProvider)
        {
            _errorListProvider = errorListProvider;
        }

        protected abstract void InvokeHandler(object sender, EventArgs eventArgs);

        protected void AddCustomToolItemBeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand menuItem = (OleMenuCommand) sender;

            ThreadHelper.ThrowIfNotOnUIThread();
            menuItem.Visible = CalculateVisible();
        }

        protected void ForEachTask(Action<object> action)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Window window = DTE.Windows.Item(WindowKinds.vsWindowKindErrorList);
            var myErrorList = (IVsTaskList2) window.Object;

            IVsEnumTaskItems itemEnumerator;
            myErrorList.EnumSelectedItems(out itemEnumerator);

            uint[] fetched = {0};
            IVsTaskItem[] items = {null};
#pragma warning disable S1264
            for (itemEnumerator.Reset(); itemEnumerator.Next(1, items, fetched) == VSConstants.S_OK && fetched[0] == 1;)
            {
                action(items[0]);
            }
#pragma warning restore S1264
        }

        protected abstract bool VisibleExpression(MissingErrorTask task);

        protected void RemoveTask(Task task)
        {
            _errorListProvider.Tasks.Remove(task);
        }

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
            return visible;
        }

        protected List<MissingErrorTask> MissingErrorTasks(string code)
        {
            var tasks = new List<MissingErrorTask>();
            ThreadHelper.ThrowIfNotOnUIThread();
            ForEachTask(task =>
            {
                var item = task as MissingErrorTask;

                if (item != null && item.Code == code)
                {
                    tasks.Add(item);
                }
            });
            return tasks;
        }
    }
}