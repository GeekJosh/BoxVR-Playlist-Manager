using BoxVR_Playlist_Manager.FitXr.BeatStructure;
using BoxVR_Playlist_Manager.FitXr.Models;
using BoxVR_Playlist_Manager.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace BoxVR_Playlist_Manager
{
    public class PlaylistViewModel : NotifyingObject
    {
        public WorkoutPlaylist _workoutPlaylist;

        public WorkoutPlaylist _originalWorkoutPlaylist;

        public ObservableCollection<SongDefinition> Tracks { get; private set; }

        private string _title;

        public string Title
        {
            get => _title;
            set
            {
                if(SetProperty(ref _title, value))
                {
                    _workoutPlaylist.definition.workoutName = value;
                    IsModified = true;
                }
            }
        }


        public float Duration
        {
            get => _workoutPlaylist.definition.duration;
        }

        public TimeSpan DurationTimeSpan => TimeSpan.FromSeconds(Duration);


        private bool _isModified;
        public bool IsModified
        {
            get => _isModified;
            private set
            {
                if(!IsLoading)
                {
                    SetProperty(ref _isModified, value);
                }
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        private bool _isGeneratingBeatmaps;
        public bool IsGeneratingBeatmaps
        {
            get => _isGeneratingBeatmaps;
            private set => SetProperty(ref _isGeneratingBeatmaps, value);
        }

        private SongDefinition _selectedTrack;
        public SongDefinition SelectedTrack
        {
            get => _selectedTrack;
            set => SetProperty(ref _selectedTrack, value);
        }


        // ------------------ Commands ---------------------

        public ICommand AddSongCommand { get; set; }
        public ICommand SongMoveUpCommand { get; set; }
        public ICommand SongMoveDownCommand { get; set; }
        public ICommand RemoveSongCommand { get; set; }
        public ICommand SavePlaylistCommand { get; set; }

        public PlaylistViewModel(WorkoutPlaylist workoutPlaylist, Dispatcher dispatcher) : base(dispatcher)
        {
            IsLoading = true;
            AddSongCommand = new RelayCommand(AddSongCommandExecute);
            RemoveSongCommand = new RelayCommand(RemoveSongCommandExecute);
            SavePlaylistCommand = new RelayCommand(SaveCommandExecute);
            SongMoveUpCommand = new RelayCommand(SongMoveUpCommandExecute);
            SongMoveDownCommand = new RelayCommand(SongMoveDownCommandExecute);
            
            _originalWorkoutPlaylist = workoutPlaylist;
            _workoutPlaylist = new WorkoutPlaylist(workoutPlaylist);
            Title = workoutPlaylist.definition.workoutName;
            Tracks = new ObservableCollection<SongDefinition>(workoutPlaylist.songs);
            Tracks.CollectionChanged += Tracks_CollectionChanged;
            IsLoading = false;
        }

        private void Tracks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IsModified = true;
            _workoutPlaylist.CalcTotalLength();
            OnPropertyChanged(nameof(DurationTimeSpan));
        }

        public void AddSongCommandExecute(object arg)
        {
            using(var dlg = new System.Windows.Forms.OpenFileDialog())
            {
                dlg.AutoUpgradeEnabled = true;
                dlg.CheckFileExists = true;
                dlg.CheckPathExists = true;
                dlg.Multiselect = true;
                dlg.Title = "Add Track";

                //Taken from BOXVR source code.
                dlg.Filter = "MP3 Files (*.mp3)|*.mp3|M4A Files (*.m4a)|*.m4a|OGG Files (*.ogg)|*.ogg";
                var result = dlg.ShowDialog();

                if(result == System.Windows.Forms.DialogResult.OK)
                {
                    foreach(var file in dlg.FileNames)
                        AddSong(file);
                }
            }
        }

        public void AddSong(string fileName)
        { 
            //Create a dummy track object with all required properties for display and for later import
            ATL.Track track = new ATL.Track(fileName);
            var songDefinition = new SongDefinition()
            {
                trackDefinition = new TrackDefinition()
                {
                    tagLibArtist = track.Artist,
                    tagLibTitle = track.Title,
                    duration = track.Duration,
                    trackData = new TrackData()
                    {
                        originalFilePath = fileName
                    }
                }
            };

            _workoutPlaylist.AddSong(songDefinition);

            Tracks.Add(songDefinition);
        }

        public void RemoveSongCommandExecute(object arg)
        {
            if(arg is SongDefinition song)
            {
                _workoutPlaylist.RemoveSong(song);
                Tracks.Remove(song);
            }
        }

        public void SongMoveUpCommandExecute(object arg)
        {
            if(arg is SongDefinition song)
            {
                var index = Tracks.IndexOf(song);
                if(index > 0)
                {
                    var prev = Tracks[index - 1];
                    Tracks[index - 1] = song;
                    Tracks[index] = prev;
                }
                SelectedTrack = song;
            }
        } 
        
        public void SongMoveDownCommandExecute(object arg)
        {
            if(arg is SongDefinition song)
            {
                var index = Tracks.IndexOf(song);
                if(index < Tracks.Count)
                {
                    var next = Tracks[index + 1];
                    Tracks[index + 1] = song;
                    Tracks[index] = next;
                }
                SelectedTrack = song;
            }
        }

        public void SaveCommandExecute(object arg)
        {
            IsGeneratingBeatmaps = true;
            Task.Run(() =>
            {
                List<SongDefinition> list = new List<SongDefinition>();
                foreach(var track in Tracks)
                {
                    if(!_originalWorkoutPlaylist.songs.Contains(track))
                    {
                        var addedSong = PlaylistManager.instance.PlaylistAddEntry(_workoutPlaylist, track.trackDefinition.trackData.originalFilePath, FitXr.Enums.LocationMode.PlayerData);
                        list.Add(addedSong);
                    }
                    else
                    {
                        list.Add(track);
                    }
                }
                _workoutPlaylist.songs = list;
                PlaylistManager.instance.ExportPlaylistJson(_workoutPlaylist);
                IsGeneratingBeatmaps = false;
                IsModified = false;
            });
        }
    }
}
