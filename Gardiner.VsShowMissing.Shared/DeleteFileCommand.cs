using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace Gardiner.VsShowMissing
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class DeleteFileCommand : ErrorListCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteFileCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        private DeleteFileCommand(
            OleMenuCommandService commandService, 
            DTE dte,
            ErrorListProvider errorListProvider
            ) : base(dte, errorListProvider)
        {
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(PackageGuids.VS2022, PackageIds.cmdidDeleteFileOnDisk);
            var menuItem = new OleMenuCommand(Execute, menuCommandID);
            menuItem.BeforeQueryStatus += MenuItemOnBeforeQueryStatus;
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static DeleteFileCommand Instance { get; private set; }

        public static void Initialize(IServiceProvider provider, ErrorListProvider errorListProvider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var commandService = (OleMenuCommandService) provider.GetService(typeof(IMenuCommandService));
            var dte = (DTE) provider.GetService(typeof(DTE));

            Instance = new DeleteFileCommand(commandService, dte, errorListProvider);
        }

#if VS2019 || VS2022
        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="errorListProvider"></param>
        public static async System.Threading.Tasks.Task InitializeAsync(AsyncPackage package, ErrorListProvider errorListProvider)
        {
            // Switch to the main thread - the call to AddCommand in DeleteFileCommand's constructor requires
            // the UI thread.
            await package.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            var dte = (DTE)await package.GetServiceAsync(typeof(DTE));

            Instance = new DeleteFileCommand(commandService, dte, errorListProvider);
        }
#endif

        protected override bool VisibleExpression(MissingErrorTask task)
        {
            return (task != null && task.Code == Constants.FileOnDiskNotInProject);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
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
#pragma warning disable CA1031
                catch
                {
                    failedFiles.Add(task.Document);
                }
            }

            if (failedFiles.Count > 0)
            {
                System.Windows.Forms.MessageBox.Show("Unable to delete these files:\n\n" + string.Join("\n", failedFiles), "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
