using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;

namespace BoxVRPlaylistManagerNETCore.FitXr.BeatStructure
{
    public class BeatStructureMadmom : BeatStructureBase
    {
        private ILog _log = LogManager.GetLogger(typeof(BeatStructureMadmom));


        [JsonProperty("_barList")]
        public BarList _barList;
        [JsonProperty("_segmentList")]
        public SegmentList _segmentList;
        [JsonProperty("")]
        public int _dataVersion = BeatStructureMadmom.DATA_VERSION;
        public static int DATA_VERSION = 3;
        [JsonProperty("_beatList")]
        public BeatList _beatList;
        private const float MAX_SPEED_BPM = 190f;
        private const float MIN_SPEED_BPM = 90f;
        private UserSongClip _userSong;
        private MadmomProcess _madmomProcess;
        private VampProcess _vampProcessSegments;
        private VampProcess _vampProcessEnergies;

        public override List<BeatInfo> Beats => this._beatList._beats;

        public BeatStructureMadmom()
        {
        }

        public BeatStructureMadmom(
          UserSongClip userSong,
          MadmomProcess madmomProcess,
          VampProcess vampProcessSegments,
          VampProcess vampProcessEnergies)
        {
            this._madmomProcess = madmomProcess;
            this._userSong = userSong;
            this._vampProcessSegments = vampProcessSegments;
            this._vampProcessEnergies = vampProcessEnergies;
            this._isBuilt = false;
        }

        public override void Build() => this._madmomProcess.Apply(this._userSong.wavPath, OnBeatsTracked);

        private void OnBeatsTracked()
        {
            this._beatList = new BeatList();
            this._beatList.CreateFromLines(this._madmomProcess._resultEntries);
            this._barList = new BarList();
            this._barList.CreateFromBeats(this._beatList._beats);
            this._barList.RemoveDeviants(2f);
            this._barList.FillEmptyRegions(8f, this._userSong.trackData.duration);
            if((double)this._barList._bars[0].Bpm > 190.0)
                this._barList.HalfSpeed();
            else if((double)this._barList._bars[0].Bpm < 90.0)
                this._barList.DoubleSpeed(this._beatList);
            this._beatList.CreateFromBars(this._barList);
            this.AverageBpm = this._beatList.AverageBpm;
            // ISSUE: method pointer
            this._vampProcessEnergies.Apply("vamp:bbc-vamp-plugins:bbc-energy", this._userSong.wavPath, OnEnergiesTracked);
        }

        private void OnEnergiesTracked()
        {
            this._beatList.UpdateEnergiesFromLines(this._vampProcessEnergies._resultEntries);
            this._barList.UpdateEnergies(this.Beats);
            // ISSUE: method pointer
            this._vampProcessSegments.Apply("vamp:qm-vamp-plugins:qm-segmenter", this._userSong.wavPath,OnSegmentsTracked);
        }

        private void OnSegmentsTracked()
        {
            this._segmentList = new SegmentList(this._vampProcessSegments._resultEntries, this.Beats, this._barList);
            _log.Debug(this._segmentList.ToString());
            List<Segment> segments = this._segmentList._segments;
            for(int index1 = 0; index1 < segments.Count; ++index1)
            {
                for(int index2 = 0; index2 < segments[index1]._numBeats; ++index2)
                    this.Beats[segments[index1]._startBeatIndex + index2]._segment = segments[index1];
            }
            this.OnBuildComplete();        }

        private void OnBuildComplete()
        {
            this._isBuilt = true;
            RaiseBuildCompleteEvent();
        }
    }
    }
