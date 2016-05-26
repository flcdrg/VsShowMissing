using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;

namespace DavidGardiner.Gardiner_VsShowMissing
{
    internal class IncludeFileCommand : BaseCommand
    {
        private readonly ErrorListProvider _errorListProvider;

        private IncludeFileCommand(IServiceProvider serviceProvider, ErrorListProvider errorListProvider)
            : base(serviceProvider)
        {
            _errorListProvider = errorListProvider;
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

        private void InvokeHandler(object sender, EventArgs eventArgs)
        {
            Window window = DTE.Windows.Item(EnvDTE80.WindowKinds.vsWindowKindErrorList);
            var myErrorList = (IVsTaskList2)window.Object;

            int count;
            myErrorList.GetSelectionCount(out count);
        }

        private void AddCustomToolItemBeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand menuItem = (OleMenuCommand) sender;

            // Visible if we've only selected this kind of item.
            Window window = DTE.Windows.Item(EnvDTE80.WindowKinds.vsWindowKindErrorList);
            var myErrorList = (IVsTaskList2) window.Object;

            IVsEnumTaskItems itemEnumerator;
            myErrorList.EnumSelectedItems(out itemEnumerator);

            uint[] fetched = {0};
            IVsTaskItem[] items = {null};
            int misMatched = 0;
            for (itemEnumerator.Reset(); itemEnumerator.Next(1, items, fetched) == VSConstants.S_OK && fetched[0] == 1;)
            {
                Task task = items[0] as MissingErrorTask;

                if (task == null)
                {
                    misMatched++;
                }
            }

            if (misMatched == 0)
            {
                menuItem.Enabled = true;
                menuItem.Visible = true;
            }
            else
            {
                menuItem.Enabled = false;
                menuItem.Visible = false;
            }
        }
    }
}