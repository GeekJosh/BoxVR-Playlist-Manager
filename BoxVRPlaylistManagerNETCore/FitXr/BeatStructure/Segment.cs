using System.Text;

namespace BoxVRPlaylistManagerNETCore.FitXr.BeatStructure
{
    public class Segment
    {
        public float _startTime;
        public int _startBeatIndex;
        public float _length;
        public int _numBeats;
        public float _averageEnergy;
        public int _index;
        public Segment.EnergyLevel _energyLevel;

        public Segment(float startTime, float length)
        {
            this._startTime = startTime;
            this._length = length;
        }

        public float endTime => this._startTime + this._length;

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Segment:");
            stringBuilder.AppendFormat("[{0, 6:F2} Start]", (object)this._startTime);
            stringBuilder.AppendFormat("[{0, 6:F2} Energy]", (object)this._averageEnergy);
            stringBuilder.AppendFormat("[{0, 6:F2} Range]", (object)this._energyLevel.ToString());
            return stringBuilder.ToString();
        }

        public enum EnergyLevel
        {
            UltraLow,
            Low,
            Medium,
            High,
            Size,
        }
    }
}
