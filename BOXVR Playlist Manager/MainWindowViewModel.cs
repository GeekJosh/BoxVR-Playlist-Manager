﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using BoxVR_Playlist_Manager.FitXr.BeatStructure;
using BoxVR_Playlist_Manager.Helpers;
using NLog;

namespace BoxVR_Playlist_Manager
{
    public class MainWindowViewModel : NotifyingObject
    {
        private Logger _log = LogManager.GetLogger(nameof(MainWindowViewModel));
        private Dispatcher _dispatcher;
        public ObservableCollection<PlaylistViewModel> Playlists { get; set; }

        private PlaylistViewModel _selectedPlaylist;
        public PlaylistViewModel SelectedPlaylist
        {
            get => _selectedPlaylist;
            set => SetProperty(ref _selectedPlaylist, value);
        }

        public ICommand AddPlaylistCommand { get; set; }
        public ICommand SettingsCommand { get; set; }
        public ICommand RemovePlaylistCommand { get; set; }

        private bool _pathsNotSetup;
        public bool PathsNotSetup { get => _pathsNotSetup; set => SetProperty(ref _pathsNotSetup, value); }
        

        public MainWindowViewModel(Dispatcher dispatcher):base(dispatcher)
        {
            _dispatcher = dispatcher;
            AddPlaylistCommand = new RelayCommand(NewPlaylistCommandExecute);
            RemovePlaylistCommand = new RelayCommand(RemovePlaylistCommandExecute);
            SettingsCommand = new RelayCommand(SettingsCommandExecute);
            if(string.IsNullOrEmpty(Paths.PersistentDataPath) || string.IsNullOrEmpty(Paths.ApplicationPath))
            {
                PathsNotSetup = true;
                return;
            }
            Task.Run(LoadPlaylists);
        }

        private void LoadPlaylists()
        {
            Playlists = new ObservableCollection<PlaylistViewModel>();
            PlaylistManager.instance.LoadWorkoutPlaylists();
            var playlists = PlaylistManager.instance.GetAllWorkoutPlaylists();

            _dispatcher.Invoke(() =>
            {
                foreach(var playlist in playlists)
                {
                    Playlists.Add(new PlaylistViewModel(playlist, _dispatcher));
                }
                SelectedPlaylist = Playlists.First();
            });

            _log.Debug($"{Playlists.Count} playlists loaded from");
        }

        public void NewPlaylistCommandExecute(object arg)
        {
            var workoutPlaylist = PlaylistManager.instance.AddNewPlaylist();
            Playlists.Add(new PlaylistViewModel(workoutPlaylist, _dispatcher));
        }

        public void RemovePlaylistCommandExecute(object arg)
        {
            var result = MessageBox.Show($"Are you sure you want to remove playlist {SelectedPlaylist.Title}?", "Removal confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if(result == MessageBoxResult.Yes)
            {
                PlaylistManager.instance.DeletePlaylist(SelectedPlaylist.Title);
            }
            Playlists.Remove(SelectedPlaylist);
            SelectedPlaylist = Playlists.First();
        }

        private void SettingsCommandExecute(object arg)
        {
            _log.Debug("Settings window opened");
            var settingsWindow = new SettingsWindow();
            var settingsChanged = settingsWindow.ShowDialog();
            if(string.IsNullOrEmpty(Paths.PersistentDataPath) || string.IsNullOrEmpty(Paths.ApplicationPath))
            {
                PathsNotSetup = true;
                return;
            }
            PathsNotSetup = false;
            if(settingsChanged.HasValue && settingsChanged.Value)
            {
                _log.Debug("Settings were changed");
                LoadPlaylists();
            }
            else
            {
                _log.Debug("No settings changed");
            }
        }
    }
}