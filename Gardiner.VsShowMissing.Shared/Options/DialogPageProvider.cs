namespace Gardiner.VsShowMissing.Options
{
#pragma warning disable CA1812

    /// <summary>
    /// A provider for custom <see cref="DialogPage" /> implementations.
    /// </summary>
    internal class DialogPageProvider
    {
        public class General : BaseOptionPage<GeneralOptions> { }
    }
}