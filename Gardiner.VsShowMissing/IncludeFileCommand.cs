using System;
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
        }
  
        protected override bool VisibleExpression(MissingErrorTask task)
        {
            return (task == null || task.Code != "MI0002");
        }
    }
}