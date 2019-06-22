using Microsoft.VisualStudio.Shell;

namespace Gardiner.VsShowMissing
{
    public class MissingErrorTask : ErrorTask
    {
        public string ProjectPath { get; set; }

        public string Code { get; set; }
    }
}