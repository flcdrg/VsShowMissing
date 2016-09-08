using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;

namespace DavidGardiner.Gardiner_VsShowMissing
{
    internal class DeleteFileCommand : BaseMissingCommand
    {
        public DeleteFileCommand(IServiceProvider serviceProvider, ErrorListProvider errorListProvider) 
            : base(serviceProvider, errorListProvider)
        {
        }
        
        public static DeleteFileCommand Instance { get; private set; }

        public static void Initialize(IServiceProvider provider, ErrorListProvider errorListProvider)
        {
            Instance = new DeleteFileCommand(provider, errorListProvider);
        }
        
        protected override void SetupCommands()
        {
            AddCommand(PackageGuids.guidGardiner_ErrorListCmdSet, PackageIds.cmdidDeleteFileOnDisk, InvokeHandler, AddCustomToolItemBeforeQueryStatus);
        }

        protected override bool VisibleExpression(MissingErrorTask task)
        {
            return (task != null && task.Code == Constants.FileOnDiskNotInProject);
        }

        protected override void InvokeHandler(object sender, EventArgs eventArgs)
        {
            var tasks = MissingErrorTasks(Constants.FileOnDiskNotInProject);

            var failedFiles = new List<string>();

            foreach (var task in tasks)
            {
                Debug.WriteLine($"Deleting file {task.Document}");

                try
                {
                    // Remove possible read-only attribute before deleting
                    File.SetAttributes(task.Document, FileAttributes.Normal);
                    File.Delete(task.Document);
                    RemoveTask(task);
                }
                catch (Exception)
                {
                    failedFiles.Add(task.Document);
                }
            }

            if (failedFiles.Count > 0)
            {
                MessageBox.Show("Unable to delete these files:\n\n" + string.Join("\n", failedFiles), "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}