using System;
using System.ComponentModel.Design;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace DavidGardiner.Gardiner_VsShowMissing
{
    /// <summary>
    /// Basic class to wrap code about executed menu command.
    /// </summary>
    abstract class BaseCommand
    {
        protected BaseCommand(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            ServiceProvider = serviceProvider;
            DTE = serviceProvider.GetService(typeof(SDTE)) as DTE2;

#pragma warning disable S1699 // Constructors should only call non-overridable methods
#pragma warning disable CA2214 // Do not call overridable methods in constructors
            SetupCommands();
#pragma warning restore CA2214 // Do not call overridable methods in constructors
#pragma warning restore S1699 // Constructors should only call non-overridable methods
        }

        /// <summary>
        /// Gets the IDE object wrapper.
        /// </summary>
        protected DTE2 DTE { get; }

        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Setups new menu command with handlers.
        /// </summary>
        protected OleMenuCommand AddCommand(Guid menuGroup, int commandID, EventHandler invokeHandler, EventHandler beforeQueryHandler)
        {
            if (invokeHandler == null)
                throw new ArgumentNullException(nameof(invokeHandler), "Missing action to perform");

            OleMenuCommandService commandService = GetService<OleMenuCommandService, IMenuCommandService>();
            if (commandService != null)
            {
                OleMenuCommand addCustomToolItem = new OleMenuCommand(invokeHandler, new CommandID(menuGroup, commandID));

                if (beforeQueryHandler != null)
                {
                    addCustomToolItem.BeforeQueryStatus += beforeQueryHandler;
                }

                commandService.AddCommand(addCustomToolItem);
            }

            return null;
        }

        /// <summary>
        /// Gets the specific service.
        /// </summary>
        protected T GetService<T, S>() where T : class
        {
            return ServiceProvider.GetService(typeof(S)) as T;
        }

        /// <summary>
        /// Gets the specific service.
        /// </summary>
        protected T GetService<T>() where T : class
        {
            return ServiceProvider.GetService(typeof(T)) as T;
        }

        /// <summary>
        /// Overriden by child class to setup own menu commands and bind with invocation handlers.
        /// </summary>
        protected abstract void SetupCommands();
    }
}