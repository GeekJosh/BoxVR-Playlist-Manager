using System.Collections.Generic;
using Newtonsoft.Json;

namespace BoxVR_Playlist_Manager.FitXr.Models
{
    public class MusicActionListSerializable
    {
        [JsonProperty("actionList")]
        public List<MusicActionSerializable> actionList { get; set; } = new List<MusicActionSerializable>();
    }
}
