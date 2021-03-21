namespace BoxVRPlaylistManagerNETCore.FitXr.BeatStructure
{
    public class BeatInfo
    {
        public int _index;
        public int _beatInBar;
        public float _magnitude;
        public float _triggerTime;
        public float _beatLength;
        public float _bpm;
        public bool _isLastBeat;
        public Segment _segment;

        public BeatInfo(
          int index,
          float triggerTime,
          float mag,
          float beatLength,
          float bpm,
          int beatInBar,
          bool isLastBeat)
        {
            this._beatInBar = beatInBar;
            this._index = index;
            this._triggerTime = triggerTime;
            this._magnitude = mag;
            this._beatLength = beatLength;
            this._bpm = bpm;
            this._isLastBeat = isLastBeat;
        }

        public BeatInfo(int index, float triggerTime, float mag, Beat beat, bool isLastBeat)
          : this(index, triggerTime, mag, beat.length, beat.bpm, -1, isLastBeat)
        {
        }

        public override string ToString()
        {
            string str = string.Format("{2,3} [{0,6:F2} s][{1,6} bpm][{3, 6:F2} magnitude]", (object)this._triggerTime, (object)this._bpm, (object)this._index, (object)this._magnitude);
            if(this._segment != null)
                str += this._segment.ToString();
            return str + "\n";
        }
    }
}
