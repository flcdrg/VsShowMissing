using System;

namespace DavidGardiner.Gardiner_VsShowMissing
{
    internal class DeleteFileCommand : BaseMissingCommand
    {
        public DeleteFileCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        
        public static DeleteFileCommand Instance { get; private set; }

        public static void Initialize(IServiceProvider provider)
        {
            Instance = new DeleteFileCommand(provider);
        }
        
        protected override void SetupCommands()
        {
            AddCommand(PackageGuids.guidGardiner_ErrorListCmdSet, PackageIds.cmdidDeleteFileOnDisk, InvokeHandler, AddCustomToolItemBeforeQueryStatus);
        }

        protected override bool VisibleExpression(MissingErrorTask task)
        {
            return (task == null || task.Code != "MI0001");
        }

        protected override void InvokeHandler(object sender, EventArgs eventArgs)
        {
        }

    }
}