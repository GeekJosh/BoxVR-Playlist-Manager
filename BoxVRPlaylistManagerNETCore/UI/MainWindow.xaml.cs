using System.Windows;
using log4net;

namespace BoxVRPlaylistManagerNETCore.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ILog _log = LogManager.GetLogger(nameof(MainWindow));
        public MainWindow()
        {
            _log.Debug("MainWindow initializing");
            InitializeComponent();
            var viewModel = new MainWindowViewModel(Dispatcher);
            DataContext = viewModel;
        }
    }
}
