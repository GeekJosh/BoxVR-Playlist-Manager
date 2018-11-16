using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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

namespace BoxVR_Playlist_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<Playlist> Playlists;

        private Playlist _selectedPlaylist;
        private Playlist SelectedPlaylist
        {
            get => _selectedPlaylist;
            set
            {
                _selectedPlaylist = value;
                playlistView.DataContext = SelectedPlaylist;
                playlistView.UpdateLayout();
                gridGeneratingBeatmaps.DataContext = SelectedPlaylist;
            }
        }

        private delegate Point GetPosition(IInputElement element);
        private int _rowIndex;

        public MainWindow()
        {
            App.logger.Trace("MainWindow initializing");
            InitializeComponent();

            _rowIndex = -1;
            playlistTracks.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(playlistTracks_PreviewMouseLeftButtonDown);
            playlistTracks.Drop += new DragEventHandler(playlistTracks_Drop);
            App.logger.Trace("MainWindow initialized");

            LoadPlaylists();
            icPlaylists.ItemsSource = Playlists;
        }

        private void playlistTracks_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _rowIndex = GetCurrentRowIndex(e.GetPosition);
            if (_rowIndex < 0)
                return;

            playlistTracks.SelectedIndex = _rowIndex;
            if (!(playlistTracks.Items[_rowIndex] is Track track))
                return;

            if (DragDrop.DoDragDrop(playlistTracks, track, DragDropEffects.Move) != DragDropEffects.None)
                playlistTracks.SelectedItem = track;
        }

        private void playlistTracks_Drop(object sender, DragEventArgs e)
        {
            if (_rowIndex < 0)
                return;

            var i = GetCurrentRowIndex(e.GetPosition);

            if (i < 0 || i == _rowIndex)
                return;

            var movedTrack = SelectedPlaylist.Tracks[_rowIndex];
            // if this is the last row, add to end, else in position
            if (i == playlistTracks.Items.Count - 1)
            {
                SelectedPlaylist.Tracks.RemoveAt(_rowIndex);
                SelectedPlaylist.Tracks.Add(movedTrack);
            }
            else
            {
                SelectedPlaylist.Tracks.RemoveAt(_rowIndex);
                SelectedPlaylist.Tracks.Insert(i, movedTrack);
            }
        }

        private int GetCurrentRowIndex(GetPosition pos)
        {
            for(int i = 0; i < playlistTracks.Items.Count; i++)
            {
                var item = GetRowItem(i);
                if (GetMouseTargetRow(item, pos))
                    return i;
            }

            return -1;
        }

        private DataGridRow GetRowItem(int index)
        {
            if (playlistTracks.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                return null;

            return playlistTracks.ItemContainerGenerator.ContainerFromIndex(index) as DataGridRow;
        }

        private bool GetMouseTargetRow(Visual target, GetPosition position)
        {
            if (target == null) return false;
            var rect = VisualTreeHelper.GetDescendantBounds(target);
            var point = position((IInputElement)target);
            return rect.Contains(point);
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            App.logger.Trace("Settings window opened");
            var settingsWindow = new SettingsWindow();
            var settingsChanged = settingsWindow.ShowDialog();
            if(settingsChanged.HasValue && settingsChanged.Value)
            {
                App.logger.Trace("Settings were changed");
                LoadPlaylists();
            }
            else
            {
                App.logger.Trace("No settings changed");
            }
        }

        private void playlistItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPlaylist != null && SelectedPlaylist.IsModified)
            {
                var result = MessageBox.Show("Do you want to save your changes?", "Save Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);
                if(result == MessageBoxResult.Yes)
                {
                    SelectedPlaylist.Save().ContinueWith(t => Dispatcher.Invoke(() => SelectedPlaylist = ((Button)sender).Tag as Playlist));
                }
                else if(result == MessageBoxResult.No)
                {
                    SelectedPlaylist.Reset();
                    SelectedPlaylist = ((Button)sender).Tag as Playlist;
                }
            }
            else
            {
                SelectedPlaylist = ((Button)sender).Tag as Playlist;
            }
            
        }

        private void ctx_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPlaylist.SelectedTrack != null)
                SelectedPlaylist.Tracks.Remove(SelectedPlaylist.SelectedTrack);
        }

        private void ctx_Explorer_Click(object sender, RoutedEventArgs e)
        {
            if(SelectedPlaylist.SelectedTrack != null)
            {
                using (var p = new Process())
                {
                    p.StartInfo = new ProcessStartInfo()
                    {
                        FileName = SelectedPlaylist.SelectedTrack.Path,
                        UseShellExecute = true
                    };
                    p.Start();
                }
            }
            
        }

        private void trackMoveUp_Click(object sender, RoutedEventArgs e)
        {
            var selectedTrack = ((Button)sender).Tag as Track;
            var index = SelectedPlaylist.Tracks.IndexOf(selectedTrack);
            if (index > 0)
            {
                var prev = SelectedPlaylist.Tracks[index - 1];
                SelectedPlaylist.Tracks[index - 1] = selectedTrack;
                SelectedPlaylist.Tracks[index] = prev;
            }
        }

        private void trackMoveDown_Click(object sender, RoutedEventArgs e)
        {
            var selectedTrack = ((Button)sender).Tag as Track;
            var index = SelectedPlaylist.Tracks.IndexOf(selectedTrack);
            if(index < SelectedPlaylist.Tracks.Count - 1)
            {
                var next = SelectedPlaylist.Tracks[index + 1];
                SelectedPlaylist.Tracks[index + 1] = selectedTrack;
                SelectedPlaylist.Tracks[index] = next;
            }
        }

        private void LoadPlaylists()
        {
            App.logger.Trace("Playlists loading");
            Playlists = new ObservableCollection<Playlist>();
            var playlistPath = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables(Properties.Settings.Default.BoxVRAppDataPath), "Playlists");

            if (Directory.Exists(playlistPath))
            {
                foreach(var playlist in Directory.EnumerateFiles(playlistPath, "*.playlist.txt"))
                {
                    Playlists.Add(new Playlist(playlist));
                }
            }
            else
            {
                App.logger.Debug($"Playlist path does not exist: {playlistPath}");
                return;
            }

            App.logger.Trace($"{Playlists.Count} playlists loaded from {playlistPath}");
        }

        private void btnAddPlaylist_Click(object sender, RoutedEventArgs e)
        {
            var p = new Playlist();
            Playlists.Add(p);
            SelectedPlaylist = p;
        }

        private void btnSavePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPlaylist != null)
                Task.Run(async () => await SelectedPlaylist.Save());
        }

        private void btnImportPlaylist_Click(object sender, RoutedEventArgs e)
        {
            using(var dlg = new System.Windows.Forms.OpenFileDialog())
            {
                dlg.AutoUpgradeEnabled = true;
                dlg.CheckFileExists = true;
                dlg.CheckPathExists = true;
                dlg.Multiselect = true;
                dlg.Title = "Import Playlist";
                dlg.Filter = Playlist.FileDialogFilter;
                var result = dlg.ShowDialog();

                if(result == System.Windows.Forms.DialogResult.OK)
                {
                    foreach (var file in dlg.FileNames)
                    {
                        var playlist = Task.Run(async () => await Playlist.Import(file)).Result;
                        Playlists.Add(playlist);
                        SelectedPlaylist = playlist;
                    }
                }
            }
        }

        private void btnAddTrack_Click(object sender, RoutedEventArgs e)
        {
            if(SelectedPlaylist != null)
            {
                using (var dlg = new System.Windows.Forms.OpenFileDialog())
                {
                    dlg.AutoUpgradeEnabled = true;
                    dlg.CheckFileExists = true;
                    dlg.CheckPathExists = true;
                    dlg.Multiselect = true;
                    dlg.Title = "Add Track";
                    dlg.Filter = Track.FileDialogFilter;
                    var result = dlg.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        foreach (var file in dlg.FileNames)
                            SelectedPlaylist.Tracks.Add(new Track(file));
                    }
                }
            }
        }

        private void playlistTracks_Sorting(object sender, DataGridSortingEventArgs e)
        {
            if(SelectedPlaylist != null)
            {
                var prop = typeof(Track).GetProperty(e.Column.SortMemberPath);
                if(prop != null)
                {
                    var trackList = new List<Track>();
                    if (e.Column.SortDirection == System.ComponentModel.ListSortDirection.Descending || e.Column.SortDirection == null)
                        trackList.AddRange(SelectedPlaylist.Tracks.OrderBy(t => prop.GetValue(t).ToString()));
                    else
                        trackList.AddRange(SelectedPlaylist.Tracks.OrderByDescending(t => prop.GetValue(t).ToString()));

                    SelectedPlaylist.Tracks.Clear();
                    foreach (var track in trackList)
                        SelectedPlaylist.Tracks.Add(track);
                }
            }
            //e.Handled = true;
        }

        private void btnDeletePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if(SelectedPlaylist != null)
            {
                var result = MessageBox.Show("Are you sure you want to delete this playlist?", "Delete Playlist", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
                if(result == MessageBoxResult.Yes)
                {
                    SelectedPlaylist.Delete();
                    Playlists.Remove(SelectedPlaylist);
                    SelectedPlaylist = null;
                }
            }
        }
    }
}
