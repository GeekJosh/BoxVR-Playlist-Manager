using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using BoxVRPlaylistManagerNETCore.FitXr.BeatStructure;
using BoxVRPlaylistManagerNETCore.FitXr.Models;
using BoxVRPlaylistManagerNETCore.UI.Enums;
using Microsoft.Win32;

namespace BoxVRPlaylistManagerNETCore.UI
{
    public class PlaylistViewModel : NotifyingObject
    {
        public WorkoutPlaylist _workoutPlaylist;

        public WorkoutPlaylist _originalWorkoutPlaylist;

        public ObservableCollection<SongViewModel> Tracks { get; private set; }

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

        private SongViewModel _selectedTrack;
        public SongViewModel SelectedTrack
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
            Tracks = new ObservableCollection<SongViewModel>();
            foreach(var song in workoutPlaylist.songs)
            {
                Tracks.Add(new SongViewModel(song, workoutPlaylist.definition.PlaylistType, _dispatcher));
            }
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
            var dlg = new OpenFileDialog();
            
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.Multiselect = true;
            dlg.Title = "Add Track";

            //Taken from BOXVR source code.
            dlg.Filter = "MP3 Files (*.mp3)|*.mp3|M4A Files (*.m4a)|*.m4a|OGG Files (*.ogg)|*.ogg";
            var result = dlg.ShowDialog();

            if(result ?? false)
            {
                foreach(var file in dlg.FileNames)
                {
                    AddSong(file);
                }
            }
            
        }

        public void AddSong(string fileName)
        {
            //Create a dummy track object with all required properties for display and for later import
            var track = TagLib.File.Create(fileName);
            var songDefinition = new SongDefinition()
            {
                trackDefinition = new TrackDefinition()
                {
                    tagLibArtist = track.Tag.JoinedPerformers,
                    tagLibTitle = track.Tag.Title,
                    duration = (float)track.Properties.Duration.TotalSeconds,
                    trackData = new TrackData()
                    {
                        originalFilePath = fileName
                    }
                }
            };

            _workoutPlaylist.AddSong(songDefinition);

            Tracks.Add(new SongViewModel(songDefinition, PlaylistType.Local, _dispatcher));
        }

        public void RemoveSongCommandExecute(object arg)
        {
            if(arg is SongViewModel songViewModel)
            {
                _workoutPlaylist.RemoveSong(songViewModel.SongDefinition);
                Tracks.Remove(songViewModel);
            }
        }

        public void SongMoveUpCommandExecute(object arg)
        {
            if(arg is SongViewModel songViewModel)
            {
                var index = Tracks.IndexOf(songViewModel);
                if(index > 0)
                {
                    var prev = Tracks[index - 1];
                    Tracks[index - 1] = songViewModel;
                    Tracks[index] = prev;
                }
                SelectedTrack = songViewModel;
            }
        } 
        
        public void SongMoveDownCommandExecute(object arg)
        {
            if(arg is SongViewModel songViewModel)
            {
                var index = Tracks.IndexOf(songViewModel);
                if(index < Tracks.Count)
                {
                    var next = Tracks[index + 1];
                    Tracks[index + 1] = songViewModel;
                    Tracks[index] = next;
                }
                SelectedTrack = songViewModel;
            }
        }

        public void SaveCommandExecute(object arg)
        {
            IsGeneratingBeatmaps = true;
            Task.Run(() =>
            {
                List<SongDefinition> list = new List<SongDefinition>();
                foreach(var songViewModel in Tracks)
                {
                    if(!_originalWorkoutPlaylist.songs.Contains(songViewModel.SongDefinition))
                    {
                        var addedSong = PlaylistManager.instance.PlaylistAddEntry(_workoutPlaylist, songViewModel.SongDefinition.trackDefinition.trackData.originalFilePath, FitXr.Enums.LocationMode.PlayerData);
                        list.Add(addedSong);
                    }
                    else
                    {
                        list.Add(songViewModel.SongDefinition);
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
