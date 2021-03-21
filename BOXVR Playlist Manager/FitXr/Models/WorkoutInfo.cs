using BoxVR_Playlist_Manager.FitXr.Enums;
using Newtonsoft.Json;

namespace BoxVR_Playlist_Manager.FitXr.Models
{
    public class WorkoutInfo
    {
        [JsonProperty("workoutName")]
        public string workoutName { get; set; }
        [JsonProperty("game")]
        public Game game { get; set; }
        [JsonProperty("workoutStyle")]
        public int workoutStyle { get; set; }
        [JsonProperty("authorName")]
        public string authorName { get; set; }
        [JsonProperty("trackGenre")]
        public TrackGenre trackGenre { get; set; }
        [JsonProperty("leaderboardId")]
        public string leaderboardId { get; set; } = "undefined";
        [JsonProperty("sonyLeaderboardId")]
        public int sonyLeaderboardId { get; set; } = -1;
        [JsonProperty("workoutId")]
        public string workoutId { get; set; }
        [JsonProperty("hasSquats")]
        public bool hasSquats { get; set; }
        [JsonProperty("hasJumps")]
        public bool hasJumps { get; set; }
        [JsonProperty("workoutType")]
        public WorkoutType workoutType { get; set; }
        [JsonProperty("duration")]
        public float duration { get; set; }
        [JsonIgnore]
        public string originalName { get; set; }

        public WorkoutInfo()
        {

        }

        public WorkoutInfo(WorkoutInfo workoutInfo)
        {
            workoutName = workoutInfo.workoutName;
            originalName = workoutInfo.originalName ?? workoutInfo.workoutName;
            game = workoutInfo.game;
            workoutStyle = workoutInfo.workoutStyle;
            authorName = workoutInfo.authorName;
            trackGenre = workoutInfo.trackGenre;
            leaderboardId = workoutInfo.leaderboardId;
            sonyLeaderboardId = workoutInfo.sonyLeaderboardId;
            workoutId = workoutInfo.workoutId;
            hasSquats = workoutInfo.hasSquats;
            hasJumps = workoutInfo.hasJumps;
            workoutType = workoutInfo.workoutType;
            duration = workoutInfo.duration;
        }
    }
}
