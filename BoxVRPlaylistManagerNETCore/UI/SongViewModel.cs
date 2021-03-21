using System;
using System.Windows.Threading;
using BoxVRPlaylistManagerNETCore.FitXr.Models;
using BoxVRPlaylistManagerNETCore.UI.Enums;

namespace BoxVRPlaylistManagerNETCore.UI
{
    public class SongViewModel : NotifyingObject
    {
        public SongDefinition SongDefinition { get; set; }

        public string Title => SongDefinition.trackDefinition.tagLibTitle;
        public string Artist => SongDefinition.trackDefinition.tagLibArtist;

        public TimeSpan Duration => SongDefinition.trackDefinition.DurationTimeSpan;

        public float Bpm => SongDefinition.trackDefinition.bpm;


        private PlaylistType _songType;
        public PlaylistType SongType
        {
            get => _songType;
            set
            {
                if(SetProperty(ref _songType, value))
                {
                    OnPropertyChanged(nameof(DisplayProgressBar));
                }
            }
        }

        private bool _downloaded;
        public bool Downloaded
        {
            get => _downloaded;
            set
            {
                if(SetProperty(ref _downloaded, value))
                {
                    OnPropertyChanged(nameof(DisplayProgressBar));
                }
            }
        }

        private int _downloadProgress;
        public int DownloadProgress { get => _downloadProgress; set => SetProperty(ref _downloadProgress, value); }

        public bool DisplayProgressBar => SongType == PlaylistType.Spotify && !Downloaded;
        public SongViewModel(SongDefinition songDefinition, PlaylistType songType, Dispatcher dispatcher) : base(dispatcher)
        {
            SongDefinition = songDefinition;
            SongType = songType;
        }
    }
}
