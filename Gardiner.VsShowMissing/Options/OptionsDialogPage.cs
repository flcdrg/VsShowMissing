using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DavidGardiner.Gardiner_VsShowMissing.Annotations;
using Microsoft.VisualStudio.Shell;
using UIElement = System.Windows.UIElement;

namespace DavidGardiner.Gardiner_VsShowMissing.Options
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Based on http://blog.danskingdom.com/adding-a-wpf-settings-page-to-the-tools-options-dialog-window-for-your-visual-studio-extension/ and https://github.com/Haacked/Encourage/tree/master
    /// </remarks>
    [Description("Extension that checks for any files referenced in projects that do not exist")]
    [LocDisplayName("Extension that checks for any files referenced in projects that do not exist")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [CLSCompliant(false)]
    [ComVisible(true)]
    [Guid("1D9ECCF3-5D2F-4112-9B25-264596873DC9")]
    public class OptionsDialogPage : UIElementDialogPage, INotifyPropertyChanged
    {
        private OptionsDialogPageControl _optionsDialogControl;
        private TaskErrorCategory _messageLevel;
        private bool _failBuildOnError;
        private RunWhen _timing;
        private bool _notIncludedFiles;
        private string _ignorePhysicalFiles;
        private bool _useGitIgnore;

        [LocDisplayName("Use .gitignore")]
        [Description("If checked then .gitignore file is also used for ignoring files")]
        [Category("Show Missing")]
        [DefaultValue(true)]
        public bool UseGitIgnore
        {
            get { return _useGitIgnore; }
            set
            {
                if (value == _useGitIgnore) return;
                _useGitIgnore = value;
                OnPropertyChanged();
            }
        }

        [LocDisplayName("Message importance")]
        [Description("What kind of message to create in the Error List window for each missing file")]
        [Category("Show Missing")]
        [DefaultValue(TaskErrorCategory.Error)]
        public TaskErrorCategory MessageLevel
        {
            get { return _messageLevel; }
            set
            {
                if (value == _messageLevel) return;
                _messageLevel = value;
                OnPropertyChanged();
            }
        }

        [LocDisplayName("Cancel build on error")]
        [Description(
            "If true, cancel the build if any missing files are found. This only has effect if When is set to BeforeBuild"
            )]
        [Category("Show Missing")]
        [DefaultValue(false)]
        public bool FailBuildOnError
        {
            get { return _failBuildOnError; }
            set
            {
                if (value == _failBuildOnError) return;
                _failBuildOnError = value;
                OnPropertyChanged();
            }
        }

        [LocDisplayName("When")]
        [Description("When to check for missing files")]
        [Category("Show Missing")]
        [DefaultValue(RunWhen.BeforeBuild)]
        public RunWhen Timing
        {
            get { return _timing; }
            set
            {
                if (value == _timing) return;
                _timing = value;
                OnPropertyChanged();
            }
        }

        [LocDisplayName("Non-included files")]
        [Description("Generate warnings/errors for files on disk that are not included in the project")]
        [Category("Show Missing")]
        [DefaultValue(true)]
        public bool NotIncludedFiles
        {
            get { return _notIncludedFiles; }
            set
            {
                if (value == _notIncludedFiles) return;
                _notIncludedFiles = value;
                OnPropertyChanged();
            }
        }

        [LocDisplayName("Ignore Pattern")]
        [Description("Semicolon-separated list of filename patterns to ignore when checking physical files")]
        [Category("Show Missing")]
        [DefaultValue("*.*proj;*.user;.gitignore;*.ruleset;*.suo;*.licx;*.dotSettings")]
        public string IgnorePhysicalFiles
        {
            get { return _ignorePhysicalFiles; }
            set
            {
                if (string.Equals(value, _ignorePhysicalFiles, StringComparison.Ordinal)) return;

                Debug.WriteLine($"IgnoreFiles\nOld: {_ignorePhysicalFiles}\nNew: {value}");
                _ignorePhysicalFiles = value;
                OnPropertyChanged();
            }
        }

        protected override UIElement Child => _optionsDialogControl ?? (_optionsDialogControl = new OptionsDialogPageControl(this));

        protected override void OnApply(PageApplyEventArgs args)
        {
            if (args.ApplyBehavior == ApplyKind.Apply)
            {
                //string[] userEncouragments = _optionsDialogControl.Encouragements.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                //// GetEncouragements().AllEncouragements = userEncouragments;
            }

            base.OnApply(args);
        }

        public OptionsDialogPage()
        {
            DefaultSettings();
        }

        public override void ResetSettings()
        {
            DefaultSettings();
            base.ResetSettings();
        }

        private void DefaultSettings()
        {
            IgnorePhysicalFiles = "*.*proj\r\n*.user\r\n.gitignore\r\n*.ruleset\r\n*.suo\r\n*.licx\r\n*.dotSettings\r\n*.vspscc";
            NotIncludedFiles = true;
            UseGitIgnore = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Debug.WriteLine($"{propertyName}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
