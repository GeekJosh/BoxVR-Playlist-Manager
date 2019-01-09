using ATL.PlaylistReaders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoxVR_Playlist_Manager
{
    public class Playlist : INotifyPropertyChanged
    {
        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                IsModified = true;
            }
        }

        private int _durationInt;
        public int DurationInt {
            get => _durationInt;
            private set
            {
                _durationInt = value;
                OnPropertyChanged(nameof(DurationInt));
                OnPropertyChanged(nameof(Duration));
            }
        }

        public TimeSpan Duration => new TimeSpan(0, 0, DurationInt);

        public ObservableCollection<Track> Tracks { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private bool _isModified;
        public bool IsModified {
            get => _isModified;
            private set
            {
                if (!IsLoading)
                {
                    _isModified = value;
                    OnPropertyChanged(nameof(IsModified));
                }
            }
        }

        private bool _isLoading;
        public bool IsLoading {
            get => _isLoading;
            private set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        private bool _isGeneratingBeatmaps;
        public bool IsGeneratingBeatmaps
        {
            get => _isGeneratingBeatmaps;
            private set
            {
                _isGeneratingBeatmaps = value;
                OnPropertyChanged(nameof(IsGeneratingBeatmaps));
            }
        }

        private Track _selectedTrack;
        public Track SelectedTrack
        {
            get => _selectedTrack;
            set {
                _selectedTrack = value;
                OnPropertyChanged(nameof(SelectedTrack));
            }
        }

        public string Filename { get; private set; }
        private string _originalPath;
        private string SavePath => Path.Combine(Environment.ExpandEnvironmentVariables(Properties.Settings.Default.BoxVRAppDataPath), "Playlists");

        public Playlist()
        {
            IsLoading = true;

            _title = $"Playlist {DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            Tracks = new ObservableCollection<Track>();
            Tracks.CollectionChanged += Tracks_CollectionChanged;

            IsLoading = false;
        }

        public Playlist(string path) : this()
        {
            if (File.Exists(Environment.ExpandEnvironmentVariables(path)))
            {
                _originalPath = path;
                Reset();
            }
        }

        public void Reset()
        {
            IsLoading = true;

            Filename = Path.GetFileName(_originalPath);
            Title = Filename.Substring(0, Filename.Length - 13);

            Tracks.Clear();
            foreach (var track in File.ReadAllLines(_originalPath))
                Tracks.Add(new Track(track));

            IsLoading = false;
            IsModified = false;

        }

        public void Delete()
        {
            File.Delete(Path.Combine(SavePath, Filename));
        }

        private void Tracks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IsModified = true;
            OnPropertyChanged(nameof(Tracks));

            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    DurationInt += SumTrackDuration(e.NewItems);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    DurationInt -= SumTrackDuration(e.OldItems);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    DurationInt -= SumTrackDuration(e.OldItems);
                    DurationInt += SumTrackDuration(e.NewItems);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    DurationInt = SumTrackDuration(Tracks);
                    break;
            }
        }

        private int SumTrackDuration(System.Collections.IList items)
        {
            var secs = 0;
            foreach (var item in items)
            {
                if (!(item is Track track)) continue;
                secs += track.DurationInt;
            }
            return secs;
        }

        public async Task Save()
        {
            var newFilename = Title + ".playlist.txt";
            using (var stream = File.CreateText(Path.Combine(SavePath, newFilename)))
            {
                foreach (var track in Tracks)
                    stream.WriteLine(track.Path);
            }
            
            if (!string.IsNullOrEmpty(Filename) && !Filename.Equals(newFilename, StringComparison.OrdinalIgnoreCase))
                File.Delete(Path.Combine(SavePath, Filename));

            Filename = newFilename;

            await GenerateBeatTracks();

            IsModified = false;
        }

        public async Task GenerateBeatTracks()
        {
            IsGeneratingBeatmaps = true;

            await Task.Run(() =>
            {
                foreach (var track in Tracks)
                {
                    // run included BAT file - doing it this way will make it easier for folk to tweak
                    // what happens when beat tracks are run, especially if new tools are added to BoxVR in future
                    using (var p = new Process())
                    {
                        p.StartInfo = new ProcessStartInfo()
                        {
                            FileName = "cmd.exe",
                            Arguments = string.Format("/c \"\"{0}\" \"{1}\" \"{2}\" \"{3}\" \"{4}\" \"{5}\"\"",
                                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "beatmap.bat"),
                                Environment.ExpandEnvironmentVariables(Properties.Settings.Default.BoxVRExePath),
                                Environment.ExpandEnvironmentVariables(Properties.Settings.Default.BoxVRAppDataPath),
                                Path.GetDirectoryName(track.Path),
                                Path.GetFileNameWithoutExtension(track.Path),
                                Path.GetExtension(track.Path)),
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            WindowStyle = ProcessWindowStyle.Minimized
                        };

                        p.OutputDataReceived += (sender, args) => App.logger.Debug($"beatmap.bat STDOUT:    {args.Data}");
                        p.ErrorDataReceived += (sender, args) => App.logger.Error($"beatmap.bat STDERR:    {args.Data}");

                        p.Start();
                        p.BeginOutputReadLine();
                        p.BeginErrorReadLine();
                        p.WaitForExit();
                    }
                }
            });

            IsGeneratingBeatmaps = false;
        }

        private static Dictionary<string, string[]> _supportedPlaylistFormats;
        public static Dictionary<string, string[]> SupportedPlaylistFormats
        {
            get
            {
                if (_supportedPlaylistFormats == null || !_supportedPlaylistFormats.Any())
                {
                    _supportedPlaylistFormats = new Dictionary<string, string[]>();
                    foreach (var format in PlaylistReaderFactory.GetInstance().getFormats().Where(f => f.Readable))
                    {
                        var extensions = new List<string>();
                        foreach (string ext in format)
                            extensions.Add(ext);

                        _supportedPlaylistFormats.Add(format.Name, extensions.ToArray());

                    }
                }
                    

                return _supportedPlaylistFormats;
            }
        }

        private static string _fileDialogFilter;
        public static string FileDialogFilter
        {
            get
            {
                if (string.IsNullOrEmpty(_fileDialogFilter))
                {
                    var allFiletypes = new List<string>();
                    foreach (var format in SupportedPlaylistFormats)
                        allFiletypes.AddRange(format.Value.Select(ext => $"*{ext}"));

                    var filter = new StringBuilder();
                    filter.Append($"All supported files|{string.Join(";", allFiletypes)}");

                    foreach (var format in SupportedPlaylistFormats)
                        filter.Append($"|{format.Key}|{string.Join(";", format.Value.Select(ext => $"*{ext}"))}");

                    _fileDialogFilter = filter.ToString();
                }

                return _fileDialogFilter;
            }
        }

        public async static Task<Playlist> Import(string filename)
        {
            var playlist = new Playlist
            {
                Title = Path.GetFileNameWithoutExtension(filename),
            };

            var _dupCount = 0;
            var _title = playlist.Title;
            while (File.Exists(Path.Combine(playlist.SavePath, playlist.Title + ".playlist.txt")))
            {
                _dupCount++;
                playlist.Title = $"{_title} ({_dupCount})";
            }

            var reader = PlaylistReaderFactory.GetInstance().GetPlaylistReader(filename);
            foreach (var track in reader.GetFiles())
                playlist.Tracks.Add(new Track(track));

            await playlist.Save();
            return playlist;
        }
    }
}
