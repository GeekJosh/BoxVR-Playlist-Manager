using System;
using System.Collections.Generic;
using System.IO;
using BoxVR_Playlist_Manager.FitXr.Enums;
using BoxVR_Playlist_Manager.FitXr.Models;
using BoxVR_Playlist_Manager.Helpers;
using Newtonsoft.Json;

namespace BoxVR_Playlist_Manager.FitXr
{
    public class WorkoutDefinitionManager
    {
        public static WorkoutDefinitionManager instance = instance == null ? new WorkoutDefinitionManager() : instance;
        private Game currentGameType;
        private string currentpath;
        public bool workoutsLoaded;

        public List<WorkoutPlaylist> workoutList { get; private set; }

        public WorkoutDefinitionManager()
        {
            if(MusicActionListSerializer.instance == null)
                 return;
        }

        public void LoadWorkoutPlaylists(LocationMode loadingMode, Game gameType, bool additive = false)
        {
            if(additive)
            {
                if(this.workoutList == null)
                    this.workoutList = new List<WorkoutPlaylist>();
            }
            else
                this.workoutList = new List<WorkoutPlaylist>();
            this.currentGameType = gameType;
            this.workoutsLoaded = false;
            this.SetCurrentPath(loadingMode);
            switch(loadingMode)
            {
                case LocationMode.PlayerData:
                case LocationMode.Editor:
                    LoadFromDataPath();
                    break;
                default:
                    throw new Exception($"{currentGameType} NOT SUPPORTED");
            }
        }

        public void UpdateWorkoutList(List<WorkoutPlaylist> newWorkouts)
        {
            for(int index = 0; index < newWorkouts.Count; ++index)
            {
                if(!this.workoutList.Contains(newWorkouts[index]))
                    this.workoutList.Add(newWorkouts[index]);
            }
            this.workoutList.Sort((Comparison<WorkoutPlaylist>)((p1, p2) => p1.definition.duration.CompareTo(p2.definition.duration)));
        }

        private void SetCurrentPath(LocationMode locationMode) => this.currentpath = Paths.WorkoutDefinitionFolder(locationMode, Game.BoxVR);

        private void LoadFromDataPath()
        {
            if(Directory.Exists(this.currentpath))
            {
                foreach(FileSystemInfo file in new DirectoryInfo(this.currentpath).GetFiles("*.txt"))
                {
                    WorkoutPlaylist workoutPlaylist = new WorkoutPlaylist();
                    string jsonString = File.ReadAllText(file.FullName);
                    workoutPlaylist.LoadFromJSON(jsonString);
                    if(!this.workoutList.Contains(workoutPlaylist))
                        this.workoutList.Add(workoutPlaylist);
                }
                this.workoutList.Sort((Comparison<WorkoutPlaylist>)((p1, p2) => p1.definition.duration.CompareTo(p2.definition.duration)));
            }
            this.workoutsLoaded = true;
        }

        public void SaveWorkout(WorkoutPlaylist workout, LocationMode savingMode)
        {
            string str = Paths.WorkoutDefinitionFolder(savingMode, workout.definition.game);
            for(int index = 0; index < workout.songs.Count; ++index)
            {
                workout.songs[index].serialisedActionList = new MusicActionListSerializable();
                workout.songs[index].serialisedActionList.actionList = MusicActionListSerializer.Instance.BuildSerializableMusicActionList(workout.songs[index].musicActionList);
            }
            this.currentpath = str + "/" + workout.definition.workoutName + ".workoutplaylist.txt";
            File.WriteAllText(this.currentpath, JsonConvert.SerializeObject(workout));
        }

        public WorkoutPlaylist GetWorkoutByIDOrName(string nameOrID)
        {
            if(this.workoutList == null)
            {
                App.logger.Debug("Workout List not loaded");
                return (WorkoutPlaylist)null;
            }
            for(int index = 0; index < this.workoutList.Count; ++index)
            {
                if(nameOrID == this.workoutList[index].workoutId._id || nameOrID == this.workoutList[index].definition.workoutName)
                    return this.workoutList[index];
            }
            return (WorkoutPlaylist)null;
        }
    }
}
