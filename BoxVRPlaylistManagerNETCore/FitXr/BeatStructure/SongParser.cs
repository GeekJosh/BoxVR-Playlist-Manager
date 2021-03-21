using System;
using BoxVRPlaylistManagerNETCore.FitXr.Enums;
using BoxVRPlaylistManagerNETCore.FitXr.Models;
using log4net;

namespace BoxVRPlaylistManagerNETCore.FitXr.BeatStructure
{
    public class SongParser
    {
        private static ILog _log = LogManager.GetLogger(typeof(SongParser));

        public static SongParser instance = instance == null ? new SongParser() : instance;
        public MadmomProcess _madmomProcess;
        public VampProcess _vampProcessEnergies;
        public VampProcess _vampProcessSegments;
        public BeatStructureBase beatStructure;
        private UserSongClip songClip;
        private string passedPath;
        private LocationMode passedlocationMode;
        private bool busy;

        public event SongParser.OnParsingComplete onParsingComplete;

        public SongParser()
        {
            _madmomProcess = new MadmomProcess() { };
            _vampProcessEnergies = new VampProcess();
            _vampProcessSegments = new VampProcess();
        }

        /// <summary>
        /// Entry to processing tasks
        /// </summary>
        /// <param name="audioTrack">Original audio track</param>
        /// <param name="wavPath">File path to the .wav track</param>
        /// <param name="locationMode"></param>
        /// <returns></returns>
        public bool CreateSongClipFromAudioClip(
          TagLib.File audioTrack,
          string wavPath,
          LocationMode locationMode)
        {
            if(busy)
            {
                _log.Debug("Parsing a track already");
                return false;
            }
            busy = true;
            passedPath = wavPath;
            passedlocationMode = locationMode;
            WavLoaded(audioTrack);
            return true;
        }

        private void WavLoaded(TagLib.File clip)
        {
            songClip = new UserSongClip(clip);

            //Set filepath to .wav file
            songClip.wavPath = passedPath;
            ParseSong();
        }

        public void ParseSong()
        {
            this.beatStructure = (BeatStructureBase)new BeatStructureMadmom(this.songClip, this._madmomProcess, this._vampProcessEnergies, this._vampProcessSegments);
            this.beatStructure._buildCompleteEvent += OnAnalysisComplete;
            this.beatStructure.Build();
        }

        private void OnAnalysisComplete(object sender, EventArgs args)
        {
            this.busy = false;
            this.beatStructure._buildCompleteEvent -= OnAnalysisComplete;
            _log.Debug("Treack analysis complete");
            TrackData trackData = TrackDataManager.instance.NewTrackData(songClip, (BeatStructureMadmom)this.beatStructure, this.passedlocationMode);
            _log.Debug(trackData.originalTrackName);
            onParsingComplete?.Invoke(trackData);
        }

        public delegate void OnParsingComplete(TrackData trackData);

        public delegate void OnParsingFailed(string error);
    }
}
