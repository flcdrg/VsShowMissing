using System.Windows;
using System.Windows.Input;
using Microsoft.VisualStudio.Shell;

namespace Gardiner.VsShowMissing.Options
{
#pragma warning disable S110 // Inheritance tree of classes should not be too deep
    /// <summary>
    /// Interaction logic for OptionsDialogPageControl.xaml
    /// </summary>
    public partial class OptionsDialogPageControl
#pragma warning restore S110 // Inheritance tree of classes should not be too deep
    {
        public OptionsDialogPageControl()
        {
            InitializeComponent();

            IgnorePhysicalFiles.AddHandler(UIElementDialogPage.DialogKeyPendingEvent, new RoutedEventHandler(OnDialogKeyPendingEvent));
        }

        public OptionsDialogPageControl(OptionsDialogPage optionsDialogPage) : this()
        {
            DataContext = optionsDialogPage;
        }

        void OnDialogKeyPendingEvent(object sender, RoutedEventArgs e)
        {
            var args = e as DialogKeyEventArgs;
            if (args != null && args.Key == Key.Enter)
            {
                e.Handled = true;
            }
        }
    }
}
