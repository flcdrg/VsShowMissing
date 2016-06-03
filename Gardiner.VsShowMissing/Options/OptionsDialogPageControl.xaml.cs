using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualStudio.Shell;

namespace DavidGardiner.Gardiner_VsShowMissing.Options
{
    /// <summary>
    /// Interaction logic for OptionsDialogPageControl.xaml
    /// </summary>
    public partial class OptionsDialogPageControl
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
