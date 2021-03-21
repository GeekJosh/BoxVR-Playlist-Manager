using System.Windows;

namespace BoxVRPlaylistManagerNETCore.UI
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            var viewModel = new SettingsWindowViewModel(Dispatcher);
            viewModel.RequestClose += ViewModel_RequestClose;
            DataContext = viewModel;
        }

        private void ViewModel_RequestClose(object sender, bool e)
        {
            DialogResult = e;
            Close();
        }
    }
}
