﻿using System;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.AsyncPackageHelpers
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Helper method to use async/await with IAsyncServiceProvider implementation
        /// </summary>
        /// <param name="asyncServiceProvider">IAsyncServciceProvider instance</param>
        /// <param name="serviceType">Type of the Visual Studio service requested</param>
        /// <returns>Service object as type of T</returns>
        public static async Task<T> GetServiceAsync<T>(this IAsyncServiceProvider asyncServiceProvider, Type serviceType) where T : class
        {
            T returnValue = null;

            await ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                object serviceInstance = null;
                Guid serviceTypeGuid = serviceType.GUID;
                serviceInstance = await asyncServiceProvider.QueryServiceAsync(ref serviceTypeGuid);
              
                // We have to make sure we are on main UI thread before trying to cast as underlying implementation
                // can be an STA COM object and doing a cast would require calling QueryInterface/AddRef marshaling 
                // to main thread via COM.
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                returnValue = serviceInstance as T;
            });

            return returnValue;
        }

        /// <summary>
        /// Gets if async package is supported in the current instance of Visual Studio
        /// </summary>
        /// <param name="serviceProvider">an IServiceProvider instance, usually a Package instance</param>
        /// <returns>true if async packages are supported</returns>
        public static bool IsAsyncPackageSupported([NotNull] this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            IAsyncServiceProvider asyncServiceProvider = serviceProvider.GetService(typeof(SAsyncServiceProvider)) as IAsyncServiceProvider;
            return asyncServiceProvider != null;
        }
    }
}