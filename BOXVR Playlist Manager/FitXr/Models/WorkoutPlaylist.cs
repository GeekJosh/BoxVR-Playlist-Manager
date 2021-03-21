using System.Collections.Generic;
using System.IO;
using BoxVR_Playlist_Manager.FitXr.BeatStructure;
using BoxVR_Playlist_Manager.FitXr.Enums;
using BoxVR_Playlist_Manager.FitXr.MusicActions;
using Newtonsoft.Json;

namespace BoxVR_Playlist_Manager.FitXr.Models
{
    public class WorkoutPlaylist
    {
        public WorkoutPlaylist(WorkoutPlaylist playlist)
        {
            definition = new WorkoutInfo(playlist.definition);

            songs = new List<SongDefinition>(playlist.songs);
        }

        public WorkoutPlaylist()
        {
            definition = new WorkoutInfo();
            songs = new List<SongDefinition>();
        }

        [JsonProperty("definition")]
        public WorkoutInfo definition { get; set; }

        [JsonProperty("songs")]
        public List<SongDefinition> songs { get; set; }

        public WorkoutId workoutId => new WorkoutId(this.definition.workoutName.Replace("_", "").Replace(" ", ""));

        public void SaveToPath(string path)
        {
            if(!Directory.Exists(path))
            {
                App.logger.Debug("Path failure");
            }
            else
            {
                for(int index = 0; index < this.songs.Count; ++index)
                {
                    if(this.songs[index].musicActionList != null)
                    {
                        this.songs[index].serialisedActionList = new MusicActionListSerializable();
                        this.songs[index].serialisedActionList.actionList = MusicActionListSerializer.Instance.BuildSerializableMusicActionList(this.songs[index].musicActionList);
                    }
                }
                File.WriteAllText(path + this.definition.workoutName + ".workoutplaylist.txt", JsonConvert.SerializeObject(this));
            }
        }

        public void Save(LocationMode locationMode)
        {
            this.PreSave();
            WorkoutDefinitionManager.instance.SaveWorkout(this, locationMode);
        }

        private void PreSave()
        {
            this.definition.hasSquats = false;
            this.definition.trackGenre = TrackGenre.None;
            this.definition.duration = 0.0f;
        label_9:
            for(int index1 = 0; index1 < this.songs.Count; ++index1)
            {
                this.definition.trackGenre |= this.songs[index1].trackDefinition.genreMask;
                this.definition.duration += this.songs[index1].trackDefinition.duration;
                if(!this.definition.hasSquats)
                {
                    for(int index2 = 0; index2 < this.songs[index1].musicActionList.Count; ++index2)
                    {
                        if(this.songs[index1].musicActionList[index2] is MusicActionMoveCue)
                        {
                            switch(((MusicActionMoveCue)this.songs[index1].musicActionList[index2]).moveAction.moveType)
                            {
                                case MoveType.Boxing_Dodge:
                                case MoveType.Boxing_Squat:
                                    this.definition.hasSquats = true;
                                    goto label_9;
                                default:
                                    continue;
                            }
                        }
                    }
                }
            }
        }

        public void LoadFromJSON(string jsonString)
        {
            if(string.IsNullOrEmpty(jsonString))
                return;
            WorkoutPlaylist workoutPlaylist = JsonConvert.DeserializeObject<WorkoutPlaylist>(jsonString);
            if(workoutPlaylist == null)
                return;
            this.definition = workoutPlaylist.definition;
            this.songs = workoutPlaylist.songs;
            for(int index = 0; index < this.songs.Count; ++index)
                this.songs[index].musicActionList = MusicActionListSerializer.instance.ReadSerializedActionList(this.songs[index].serialisedActionList);
        }

        public void LoadFromPath(string path) => this.LoadFromJSON(this.WorkoutJsonFromPath(path));

        public string WorkoutJsonFromPath(string workoutPath)
        {
            string str = "";
            if((!File.Exists(workoutPath) ? 0 : (new FileInfo(workoutPath).Length > 0L ? 1 : 0)) != 0)
                str = File.ReadAllText(workoutPath);
            return str;
        }

        public SongDefinition AddSong(TrackId trackId)
        {
            if(this.songs == null)
                this.songs = new List<SongDefinition>();
            SongDefinition songDefinition = new SongDefinition();
            songDefinition.trackDataName = trackId.trackId;
            this.songs.Add(songDefinition);
            return songDefinition;
        }

        public void AddSong(SongDefinition song)
        {
            if(this.songs == null)
                this.songs = new List<SongDefinition>();
            this.songs.Add(song);
        }

        public void RemoveSong(int index)
        {
            string trackDataName = this.songs[index].trackDataName;
            this.songs.RemoveAt(index);
        }

        public bool RemoveSong(SongDefinition song)
        {
            return songs.Remove(song);
        }

        public float CalcTotalLength()
        {
            float num = 0.0f;
            if(this.songs != null)
            {
                for(int index = 0; index < this.songs.Count; ++index)
                {
                    if(this.songs[index].trackDefinition != null)
                    {
                        num += this.songs[index].trackDefinition.duration;
                    }
                    else
                    {
                        this.songs[index].trackDefinition = TrackDataManager.instance.GetTrackDefinition(this.songs[index].trackDataName);
                    }  
                }
            }
            this.definition.duration = num;
            return num;
        }
    }
}
