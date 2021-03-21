using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using BoxVRPlaylistManagerNETCore.FitXr.BeatStructure;
using BoxVRPlaylistManagerNETCore.Helpers;
using log4net;
using SpotiSharp;
using YoutubeExplode;

namespace BoxVRPlaylistManagerNETCore.UI
{
    public class MainWindowViewModel : NotifyingObject
    {
        private ILog _log = LogManager.GetLogger(typeof(MainWindowViewModel));
        private Dispatcher _dispatcher;
        public ObservableCollection<PlaylistViewModel> Playlists { get; set; }

        private PlaylistViewModel _selectedPlaylist;
        public PlaylistViewModel SelectedPlaylist
        {
            get => _selectedPlaylist;
            set => SetProperty(ref _selectedPlaylist, value);
        }

        public ICommand AddLocalPlaylistCommand { get; set; }
        public ICommand AddSpotifyPlaylistCommand { get; set; }
        public ICommand SettingsCommand { get; set; }
        public ICommand RemovePlaylistCommand { get; set; }
        public ICommand ExpandPlaylistSubmenuCommand { get; set; }

        private bool _pathsNotSetup;
        public bool PathsNotSetup { get => _pathsNotSetup; set => SetProperty(ref _pathsNotSetup, value); }

        private bool _displayAddPlaylistSubmenu;
        public bool DisplayAddPlayListSubmenu { get => _displayAddPlaylistSubmenu; set => SetProperty(ref _displayAddPlaylistSubmenu, value); }
        

        public MainWindowViewModel(Dispatcher dispatcher):base(dispatcher)
        {
            _dispatcher = dispatcher;
            AddLocalPlaylistCommand = new RelayCommand(NewLocalPlaylistCommandExecute);
            AddSpotifyPlaylistCommand = new RelayCommand(NewSpotifyPlaylistCommandExecute);
            RemovePlaylistCommand = new RelayCommand(RemovePlaylistCommandExecute);
            SettingsCommand = new RelayCommand(SettingsCommandExecute);
            ExpandPlaylistSubmenuCommand = new RelayCommand(ExpandPlaylistSubmenuCommandExecute);
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

        public void ExpandPlaylistSubmenuCommandExecute(object arg)
        {
            DisplayAddPlayListSubmenu = !DisplayAddPlayListSubmenu;
        }

        public void NewLocalPlaylistCommandExecute(object arg)
        {
            DisplayAddPlayListSubmenu = false;
            var workoutPlaylist = PlaylistManager.instance.AddNewPlaylist();
            Playlists.Add(new PlaylistViewModel(workoutPlaylist, _dispatcher));
        }

        public void NewSpotifyPlaylistCommandExecute(object arg)
        {
            List<TrackInfo> spotifyPlaylistTracks = new List<TrackInfo>();
            var client = SpotifyHelpers.ConnectToSpotify();
            var trackQueue = new ConcurrentQueue<TrackInfo>();
            var youTube = new YoutubeClient();
            //if(input.IsSpotifyUrl())
            //{
            //    var (type, url) = input.GetSpotifyUrlId();
            //    switch(type)
            //    {
            //        case UrlType.Playlist:
            //            var taskPlaylist = client.QueueSpotifyTracksFromPlaylist(url, trackQueue);
            //            while(!taskPlaylist.IsCompleted)
            //            {
            //                while(trackQueue.TryDequeue(out var info))
            //                {
            //                    _log.Debug($"Downloading ::::: {info.Artist} - {info.Title}".Truncate());
            //                    _log.Debug($"[Queue: {trackQueue.Count}]".MoveToRight());
            //                    youTube.DownloadAndConvertTrack(info);
            //                    _log.Debug($"Done        ::::: {info.Artist} - {info.Title}".Truncate());
            //                }
            //                Thread.Sleep(200);
            //            }
            //            while(trackQueue.TryDequeue(out var info))
            //            {
            //                _log.Debug($"Downloading ::::: {info.Artist} - {info.Title}".Truncate());
            //                _log.Debug($"[Queue: {trackQueue.Count}]".MoveToRight());
            //                youTube.DownloadAndConvertTrack(info);
            //                _log.Debug($"Done        ::::: {info.Artist} - {info.Title}".Truncate());
            //            }
            //            break;
            //        case UrlType.Album:
            //            var taskAlbum = client.QueueSpotifyTracksFromAlbum(url, trackQueue);
            //            while(!taskAlbum.IsCompleted)
            //            {
            //                while(trackQueue.TryDequeue(out var info))
            //                {
            //                    _log.Debug($"Downloading ::::: {info.Artist} - {info.Title}".Truncate());
            //                    _log.Debug($"[Queue: {trackQueue.Count}]".MoveToRight());
            //                    youTube.DownloadAndConvertTrack(info);
            //                    _log.Debug($"Done        ::::: {info.Artist} - {info.Title}".Truncate());
            //                }
            //                Thread.Sleep(200);
            //            }
            //            while(trackQueue.TryDequeue(out var info))
            //            {
            //                _log.Debug($"Downloading ::::: {info.Artist} - {info.Title}".Truncate());
            //                _log.Debug($"[Queue: {trackQueue.Count}]".MoveToRight());
            //                youTube.DownloadAndConvertTrack(info);
            //                _log.Debug($"Done        ::::: {info.Artist} - {info.Title}".Truncate());
            //            }
            //            break;
            //        case UrlType.Track:
            //            var track = client.GetSpotifyTrack(input).GetAwaiter().GetResult();
            //            if(track == null)
            //            {
            //                Environment.Exit(1);
            //            }
            //            _log.Debug($"Downloading ::::: {track.Artist} - {track.Title}".Truncate());
            //            youTube.DownloadAndConvertTrack(track);
            //            _log.Debug($"Done        ::::: {track.Artist} - {track.Title}".Truncate());
            //            break;
            //    }
            //}
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
