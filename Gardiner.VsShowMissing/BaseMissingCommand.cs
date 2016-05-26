using System;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace DavidGardiner.Gardiner_VsShowMissing
{
    internal abstract class BaseMissingCommand : BaseCommand
    {
        protected BaseMissingCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected abstract void InvokeHandler(object sender, EventArgs eventArgs);

        protected void AddCustomToolItemBeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand menuItem = (OleMenuCommand) sender;

            menuItem.Visible = CalculateVisible();
        }

        protected void ForEachTask(Action<object> action)
        {
            Window window = DTE.Windows.Item(WindowKinds.vsWindowKindErrorList);
            var myErrorList = (IVsTaskList2) window.Object;

            IVsEnumTaskItems itemEnumerator;
            myErrorList.EnumSelectedItems(out itemEnumerator);

            uint[] fetched = {0};
            IVsTaskItem[] items = {null};
            for (itemEnumerator.Reset(); itemEnumerator.Next(1, items, fetched) == VSConstants.S_OK && fetched[0] == 1;)
            {
                action(items[0]);
            }
        }

        protected abstract bool VisibleExpression(MissingErrorTask task);

              private bool CalculateVisible()
        {
            int misMatched = 0;

            ForEachTask(item =>
            {
                var task = item as MissingErrorTask;

                // Visible if we've only selected this kind of item.
                if (VisibleExpression(task))
                {
                    misMatched++;
                }
            });

            var visible = misMatched == 0;
            return visible;
        }

    }
}