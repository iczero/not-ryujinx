using Ryujinx.Ava.UI.ViewModels;
using Ryujinx.Ui.Common.Configuration;
using System.ComponentModel;

namespace Ryujinx.Ava.UI.Windows
{
    public partial class ConsoleWindow : StyleableWindow
    {
        private static ConsoleWindowViewModel ConsoleWindowViewModel { get; set; }

        public ConsoleWindow()
        {
            ConsoleWindowViewModel = new ConsoleWindowViewModel(this);

            DataContext = ConsoleWindowViewModel;

            InitializeComponent();

            ConsoleScrollViewer.ScrollChanged += OnScrollChanged;
            AutoScrollCheckBox.Checked += OnScrollChanged;
        }

        private void OnScrollChanged(object sender, object e)
        {
            if (AutoScrollCheckBox.IsChecked == true)
            {
                ConsoleScrollViewer.ScrollToEnd();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
            ConfigurationState.Instance.Ui.ShowConsole.Value = false;

            base.OnClosing(e);
        }
    }
}