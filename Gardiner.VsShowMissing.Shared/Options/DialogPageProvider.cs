using System.Runtime.InteropServices;

namespace Gardiner.VsShowMissing.Options
{
    /// <summary>
    /// A provider for custom <see cref="Microsoft.VisualStudio.Shell.DialogPage" /> implementations.
    /// </summary>
    internal class DialogPageProvider
    {
        [ComVisible(true)]
        public class General : BaseOptionPage<GeneralOptions> { }
    }
}