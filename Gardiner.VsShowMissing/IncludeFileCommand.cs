using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
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
        }

        private void AddCustomToolItemBeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand button = (OleMenuCommand) sender;
            button.Visible = false;
 

            //button.Checked = _item.Properties.Item("CustomTool").Value.ToString() == CUSTOM_TOOL_NAME;
            button.Visible = true;

            OleMenuCommand menuItem = sender as OleMenuCommand;
            {
                Window window = DTE.Windows.Item(EnvDTE80.WindowKinds.vsWindowKindErrorList);
                var myErrorList = (EnvDTE80.ErrorList)window.Object;
                var errorItems = myErrorList.ErrorItems;
                if (errorItems != null)
                {

                    if (errorItems.Count > 0)
                    {
                        for (int i = 1; i <= errorItems.Count; i++)
                        {
                            ErrorItem item = errorItems.Item(i);

                            var targetProp = item.GetType().GetProperty("Target");

                            Debug.WriteLine(item);
                            //var missingErrorTask = item as MissingErrorTask;
                            //Debug.WriteLine(missingErrorTask);
                        }

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
       
    }
}