using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoxVR_Playlist_Manager.FitXr.Models;
using BoxVR_Playlist_Manager.FitXr.MusicActions;
using BoxVR_Playlist_Manager.Helpers;
using Newtonsoft.Json;

namespace BoxVR_Playlist_Manager.FitXr
{
    public class MusicActionListSerializer
    {
        public static MusicActionListSerializer instance = instance == null ? new MusicActionListSerializer() : instance;

        public static MusicActionListSerializer Instance => instance;

        public void SaveWorkoutActionSequence(
          List<MusicAction> musicActions,
          string workoutName,
          string difficulty = "")
        {
            string str = Paths.PersistentDataPath + "/Playlists/Workouts/";
            string path = Path.Combine(str, workoutName + "_" + difficulty + ".actions");
            if(difficulty == "")
            {
                str = Paths.PersistentDataPath + "/Workouts/";
                path = Path.Combine(str, workoutName + ".actions");
            }
            if(!Directory.Exists(str))
                Directory.CreateDirectory(str);
            List<MusicActionSerializable> actionSerializableList = this.BuildSerializableMusicActionList(musicActions);
            string json = JsonConvert.SerializeObject(new MusicActionListSerializable()
            {
                actionList = actionSerializableList
            });
            File.WriteAllText(path, json);
        }

        public List<MusicAction> LoadWorkoutActionSequence(
          string workoutName,
          string difficulty = "")
        {
            List<MusicAction> musicActionList = new List<MusicAction>();
            string str = Paths.PersistentDataPath + "/Playlists/Workouts/" + workoutName + "_" + difficulty + ".actions";
            if(difficulty == "")
                str = Paths.PersistentDataPath + "/Workouts/" + workoutName + ".actions";
            return (!File.Exists(str) ? 0 : (new FileInfo(str).Length > 0L ? 1 : 0)) != 0 ? this.ReadSerializedActionList(JsonConvert.DeserializeObject<MusicActionListSerializable>(File.ReadAllText(str))) : (List<MusicAction>)null;
        }

        public List<MusicAction> ReadSerializedActionList(
          MusicActionListSerializable listWrapper)
        {
            List<MusicAction> musicActionList = new List<MusicAction>();
            if(listWrapper == null)
            {
                return musicActionList;
            }    
            for(int index = 0; index < listWrapper.actionList.Count; ++index)
            {
                switch(listWrapper.actionList[index].musicActionType)
                {
                    case MusicActionType.MoveCue:
                        MusicActionMoveCue musicActionMoveCue = JsonConvert.DeserializeObject<MusicActionMoveCue>(listWrapper.actionList[index].musicActionJSON);
                        musicActionMoveCue.musicActiontype = MusicActionType.MoveCue;
                        musicActionList.Add((MusicAction)musicActionMoveCue);
                        break;
                    case MusicActionType.Message:
                        MusicActionMessage musicActionMessage = JsonConvert.DeserializeObject<MusicActionMessage>(listWrapper.actionList[index].musicActionJSON);
                        musicActionMessage.musicActiontype = MusicActionType.Message;
                        musicActionList.Add((MusicAction)musicActionMessage);
                        break;
                    case MusicActionType.BPM:
                        MusicActionBPM musicActionBpm = JsonConvert.DeserializeObject<MusicActionBPM>(listWrapper.actionList[index].musicActionJSON);
                        musicActionBpm.musicActiontype = MusicActionType.BPM;
                        musicActionList.Add((MusicAction)musicActionBpm);
                        break;
                    case MusicActionType.Audio:
                        MusicActionAudio musicActionAudio = JsonConvert.DeserializeObject<MusicActionAudio>(listWrapper.actionList[index].musicActionJSON);
                        musicActionAudio.musicActiontype = MusicActionType.Audio;
                        musicActionList.Add((MusicAction)musicActionAudio);
                        break;
                }
            }
            return musicActionList;
        }

        public List<MusicActionSerializable> BuildSerializableMusicActionList(
          List<MusicAction> musicActions)
        {
            List<MusicActionSerializable> actionSerializableList = new List<MusicActionSerializable>();
            for(int index = 0; index < musicActions.Count; ++index)
            {
                MusicActionSerializable actionSerializable = new MusicActionSerializable();
                if(musicActions[index] is MusicActionMoveCue)
                    actionSerializable.musicActionType = MusicActionType.MoveCue;
                else if(musicActions[index] is MusicActionMessage)
                    actionSerializable.musicActionType = MusicActionType.Message;
                else if(musicActions[index] is MusicActionBPM)
                    actionSerializable.musicActionType = MusicActionType.BPM;
                else if(musicActions[index] is MusicActionAudio)
                    actionSerializable.musicActionType = MusicActionType.Audio;
                actionSerializable.musicActionJSON = this.SerializeMusicAction(musicActions[index], actionSerializable.musicActionType);
                actionSerializableList.Add(actionSerializable);
            }
            return actionSerializableList;
        }

        private string SerializeMusicAction(MusicAction musicAction, MusicActionType musicActionType)
        {
            string str = "";
            switch(musicActionType)
            {
                case MusicActionType.MoveCue:
                    str = JsonConvert.SerializeObject((MusicActionMoveCue)musicAction);
                    break;
                case MusicActionType.Message:
                    str = JsonConvert.SerializeObject((MusicActionMessage)musicAction);
                    break;
                case MusicActionType.BPM:
                    str = JsonConvert.SerializeObject((MusicActionBPM)musicAction);
                    break;
                case MusicActionType.Audio:
                    str = JsonConvert.SerializeObject((MusicActionAudio)musicAction);
                    break;
            }
            return str;
        }
    }
}
