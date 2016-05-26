using System;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
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

        protected override void InvokeHandler(object sender, EventArgs eventArgs)
        {
            Window window = DTE.Windows.Item(WindowKinds.vsWindowKindErrorList);
            var myErrorList = (IVsTaskList2)window.Object;

            int count;
            myErrorList.GetSelectionCount(out count);
        }

        protected override void AddCustomToolItemBeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand menuItem = (OleMenuCommand) sender;

            menuItem.Visible = CalculateVisible();
        }

        private bool CalculateVisible()
        {
            int misMatched = 0;

            ForEachTask(item =>
            {
                var task = item as MissingErrorTask;

                // Visible if we've only selected this kind of item.
                if (task == null || task.Code != "MI002")
                {
                    misMatched++;
                }
            });

            var visible = misMatched == 0;
            return visible;
        }
    }
}