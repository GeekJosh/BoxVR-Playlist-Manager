using System.Collections.ObjectModel;
using System.Windows;
using NLog;

namespace BoxVR_Playlist_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Logger _log = LogManager.GetLogger(nameof(MainWindow));

        public MainWindow()
        {
            _log.Debug("MainWindow initializing");
            InitializeComponent();
            var viewModel = new MainWindowViewModel(Dispatcher);
            DataContext = viewModel;
        }
    }
}
