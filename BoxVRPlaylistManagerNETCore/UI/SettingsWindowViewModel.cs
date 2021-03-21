using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;

namespace BoxVRPlaylistManagerNETCore.UI
{
    public class SettingsWindowViewModel : NotifyingObject
    {
        public ICommand OkCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand DonateCommand { get; set; }
        public ICommand BrowseExeCommand { get; set; }
        public ICommand BrowseAppDataCommand { get; set; }

        public event EventHandler<bool> RequestClose;
        public SettingsWindowViewModel(Dispatcher dispatcher) : base(dispatcher)
        {
            OkCommand = new RelayCommand(OkCommandExecute);
            CancelCommand = new RelayCommand(CancelCommandExecute);
            DonateCommand = new RelayCommand(DonateCommandExecute);
            BrowseExeCommand = new RelayCommand(BrowseExeCommandExecute);
            BrowseAppDataCommand = new RelayCommand(BrowseAppDataCommandExecute);
        }

        private void OkCommandExecute(object arg)
        {
            RequestClose?.Invoke(this, true);
        }

        private void CancelCommandExecute(object arge)
        {
            RequestClose?.Invoke(this, false);
        }

        private void DonateCommandExecute(object arg)
        {
            var process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = true,
                FileName = @"https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=2MGZP7J29CP9W"
            };
            process.Start();
        }

        private void BrowseExeCommandExecute(object arg)
        {
            App.Configuration.BoxVRExePath = GetFolder(App.Configuration.BoxVRExePath);
        }

        private void BrowseAppDataCommandExecute(object arg)
        {
            App.Configuration.BoxVRAppDataPath = GetFolder(App.Configuration.BoxVRAppDataPath);
        }

        public string BoxVRExePath
        {
            get => App.Configuration.BoxVRExePath;
            set
            {
                if(App.Configuration.BoxVRExePath != value)
                {
                    App.Configuration.BoxVRExePath = value;
                    OnPropertyChanged(nameof(BoxVRExePath));
                }
            }
        }
        public string BoxVRAppDataPath
        {
            get => App.Configuration.BoxVRAppDataPath;
            set
            {
                if(App.Configuration.BoxVRAppDataPath != value)
                {
                    App.Configuration.BoxVRAppDataPath = value;
                    OnPropertyChanged(nameof(BoxVRAppDataPath));
                }
            }
        }

        private string GetFolder(string startFolder)
        {
            using(var dlg = new FolderBrowserDialog())
            {
                dlg.ShowNewFolderButton = false;

                if(!string.IsNullOrWhiteSpace(startFolder))
                    dlg.SelectedPath = Environment.ExpandEnvironmentVariables(startFolder);

                var result = dlg.ShowDialog();
                return result == System.Windows.Forms.DialogResult.OK ? dlg.SelectedPath : startFolder;
            }
        }
    }
}
