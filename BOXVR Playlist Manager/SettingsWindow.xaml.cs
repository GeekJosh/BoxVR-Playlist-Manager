using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BoxVR_Playlist_Manager
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            DialogResult = true;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reload();
            DialogResult = false;
        }

        private void paypalDonate_Click(object sender, RoutedEventArgs e)
        {
            var process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = true,
                FileName = @"https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=2MGZP7J29CP9W"
            };
            process.Start();
        }

        private void btnBrowseExe_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.BoxVRExePath = GetFolder(Properties.Settings.Default.BoxVRExePath);
        }

        private void btnBrowseAppData_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.BoxVRAppDataPath = GetFolder(Properties.Settings.Default.BoxVRAppDataPath);
        }

        private string GetFolder(string startFolder)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.ShowNewFolderButton = false;

                if (!string.IsNullOrWhiteSpace(startFolder))
                    dlg.SelectedPath = Environment.ExpandEnvironmentVariables(startFolder);

                var result = dlg.ShowDialog(this.GetIWin32Window());
                return result == System.Windows.Forms.DialogResult.OK ? dlg.SelectedPath : startFolder;
            }
        }
    }
}
