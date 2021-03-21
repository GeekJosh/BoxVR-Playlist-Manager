using Newtonsoft.Json;

namespace BoxVRPlaylistManagerNETCore.FitXr.Models
{
    public class TrackBase
    {
        public TrackBase()
        {

        }
        [JsonProperty("trackId")]
        public TrackId trackId { get; set; }
        [JsonProperty("duration")]
        public float duration { get; set; }
        [JsonProperty("bpm")]
        public float bpm { get; set; }
    }
}
