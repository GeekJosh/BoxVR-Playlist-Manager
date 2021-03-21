using Newtonsoft.Json;

namespace BoxVR_Playlist_Manager.FitXr.BeatStructure
{
    public class Bar
    {
        [JsonProperty("_avgEnergy")]
        public float _avgEnergy;
        [JsonProperty("_duration")]
        public float _duration;
        [JsonProperty("_startTime")]
        public float _startTime;
        [JsonProperty("_beatIndex")]
        public int _beatIndex;

        public float Bpm => Bar.DurationToBpm(this._duration);

        public float EndTime => this._startTime + this._duration;

        public static float DurationToBpm(float duration) => 240f / duration;

        public static float BpmToDuration(float bpm) => 240f / bpm;

        public override string ToString() => "Start:" + (object)this._startTime + " Length:" + (object)this._duration;
    }
}
