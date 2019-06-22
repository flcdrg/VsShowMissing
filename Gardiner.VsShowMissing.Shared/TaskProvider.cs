using System;

using Microsoft.VisualStudio.Shell;

namespace DavidGardiner.Gardiner_VsShowMissing
{
    public class TaskProvider : ErrorListProvider
    {
        public TaskProvider(IServiceProvider provider) : base(provider)
        {
            ProviderName = "MissingFiles";
            ProviderGuid = new Guid("BEC0A177-F8F9-4A2D-80D2-7FA05DDB8D2B");
        }
    }
}