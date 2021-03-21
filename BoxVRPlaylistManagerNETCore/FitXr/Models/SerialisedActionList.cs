using System.Collections.Generic;
using Newtonsoft.Json;

namespace BoxVRPlaylistManagerNETCore.FitXr.Models
{
    public class MusicActionListSerializable
    {
        [JsonProperty("actionList")]
        public List<MusicActionSerializable> actionList { get; set; } = new List<MusicActionSerializable>();
    }
}
