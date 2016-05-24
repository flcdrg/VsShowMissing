using Microsoft.VisualStudio.Shell;

namespace DavidGardiner.Gardiner_VsShowMissing
{
    public class MissingErrorTask : ErrorTask
    {
        public string ProjectPath { get; set; }
    }
}