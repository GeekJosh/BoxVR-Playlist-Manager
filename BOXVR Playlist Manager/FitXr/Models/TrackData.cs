using System.IO;
using BoxVR_Playlist_Manager.FitXr.BeatStructure;
using BoxVR_Playlist_Manager.FitXr.Enums;
using BoxVR_Playlist_Manager.Helpers;
using Newtonsoft.Json;

namespace BoxVR_Playlist_Manager.FitXr.Models
{
    public class TrackData : TrackBase
    {
        public TrackData()
        {

        }


        [JsonProperty("originalFilePath")]
        public string originalFilePath { get; set; }
        [JsonProperty("originalTrackName")]
        public string originalTrackName { get; set; }
        [JsonProperty("originalArtist")]
        public string originalArtist { get; set; }
        [JsonProperty("firstBeatOffset")]
        public float firstBeatOffset { get; set; }
        [JsonProperty("locationMode")]
        public LocationMode locationMode { get; set; }
        [JsonProperty("beatStrucureJSON")]
        public string beatStrucureJSON { get; set; }

        [JsonIgnore]
        public BeatStructureMadmom beatStructure;
        [JsonIgnore]
        public TrackDataState trackDataState;

        public event TrackData.OnTrackDataReady onTrackDataReady;

        public void LoadTrackData(string trackId, LocationMode locationMode)
        {
            string str = "";
            switch(locationMode)
            {
                case LocationMode.PlayerData:
                case LocationMode.Editor:
                    string path = Paths.TrackDataFolder(locationMode) + trackId + ".trackdata.txt";
                    if(File.Exists(path))
                    {
                        str = File.ReadAllText(path);
                    }
                    break;
            }
            if(str != "")
            {
                TrackData trackData = JsonConvert.DeserializeObject<TrackData>(str);
                this.originalTrackName = trackData.originalTrackName;
                this.trackId = trackData.trackId;
                this.duration = trackData.duration;
                this.bpm = trackData.bpm;
                this.locationMode = locationMode;
                this.beatStrucureJSON = trackData.beatStrucureJSON;
                this.originalFilePath = trackData.originalFilePath;
                this.originalArtist = trackData.originalArtist;
                this.LoadBeatStructure();
                if(this.onTrackDataReady != null)
                    this.onTrackDataReady(this);
                this.trackDataState = TrackDataState.Ready;
            }
            else
            {
                this.trackDataState = TrackDataState.Failed;
            }
        }

        public void LoadBeatStructure()
        {
            this.beatStructure = TrackDataManager.instance.ReadBeatStructureFromJSON(this.beatStrucureJSON);
        }

        public string WavFilePath()
        {
            string str = "";
            switch(this.locationMode)
            {
                case LocationMode.PlayerData:
                    str = MadmomProcess.madmonOutputPath + this.trackId.trackId + ".wav";
                    break;
                case LocationMode.Workouts:
                case LocationMode.MyWorkout:
                    str = this.trackId.trackId.ToString();
                    break;
                case LocationMode.Editor:
                    str = Paths.WavDataFolder(this.locationMode) + "/" + this.trackId.trackId + ".wav";
                    break;
            }
            return str;
        }

        public void PopulateTrackDataFromAudioFile(
          string filePath,
          LocationMode locationMode,
          Game gameType)
        {
            this.trackDataState = TrackDataState.Loading;
            TrackDataManager.instance.onTrackDataComplete += new TrackDataManager.OnTrackDataComplete(OnTrackDataCreated);
            if(TrackDataManager.instance.DoesTrackDataExist(filePath, locationMode, gameType))
            {
                this.LoadTrackData(new TrackId(filePath).trackId, locationMode);
            }
            else
            {
                TrackDataManager.instance.PopulateTrackDataFromAudioFile(filePath, locationMode, filePath);
            }
        }

        public void OnTrackDataCreated(TrackData trackData)
        {
            TrackDataManager.instance.onTrackDataComplete -= new TrackDataManager.OnTrackDataComplete(this.OnTrackDataCreated);
            this.originalFilePath = trackData.originalFilePath;
            this.originalTrackName = trackData.originalTrackName;
            this.originalArtist = trackData.originalArtist;
            this.trackId = trackData.trackId;
            this.duration = trackData.duration;
            this.bpm = trackData.bpm;
            this.locationMode = trackData.locationMode;
            this.beatStrucureJSON = trackData.beatStrucureJSON;
            this.trackDataState = TrackDataState.Ready;
            onTrackDataReady?.Invoke(this);
        }


        public delegate void OnTrackDataReady(TrackData trackData);
    }
}
