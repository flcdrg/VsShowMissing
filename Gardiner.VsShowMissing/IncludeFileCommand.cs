using System;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace DavidGardiner.Gardiner_VsShowMissing
{
    internal class IncludeFileCommand : BaseCommand
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

        private void InvokeHandler(object sender, EventArgs eventArgs)
        {
            Window window = DTE.Windows.Item(WindowKinds.vsWindowKindErrorList);
            var myErrorList = (IVsTaskList2)window.Object;

            int count;
            myErrorList.GetSelectionCount(out count);
        }

        private void AddCustomToolItemBeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand menuItem = (OleMenuCommand) sender;

            Window window = DTE.Windows.Item(WindowKinds.vsWindowKindErrorList);
            var myErrorList = (IVsTaskList2) window.Object;

            IVsEnumTaskItems itemEnumerator;
            myErrorList.EnumSelectedItems(out itemEnumerator);

            uint[] fetched = {0};
            IVsTaskItem[] items = {null};
            int misMatched = 0;
            for (itemEnumerator.Reset(); itemEnumerator.Next(1, items, fetched) == VSConstants.S_OK && fetched[0] == 1;)
            {
                var task = items[0] as MissingErrorTask;

                // Visible if we've only selected this kind of item.
                if (task == null || task.Code != "MI002")
                {
                    misMatched++;
                }
            }

            menuItem.Visible = misMatched == 0;
        }
    }
}