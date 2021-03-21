using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.IO;
using BoxVR_Playlist_Manager.FitXr.Models;
using BoxVR_Playlist_Manager.Helpers;
using BoxVR_Playlist_Manager.FitXr.Enums;
using Newtonsoft.Json;
using BoxVR_Playlist_Manager.FitXr.Utility;

namespace BoxVR_Playlist_Manager.FitXr.BeatStructure
{
    public class PlaylistManager
    {
        public static int MAX_NUM_ENTRIES = 20;
        public static PlaylistManager instance = instance == null ? new PlaylistManager() : instance;
        private string playlistsPath;
        public List<WorkoutPlaylist> workoutPlaylists;
        private List<PlaylistManager.PlaylistToAdd> playlistQueue;
        private bool isProcessing;

        public event PlaylistManager.OnTrackAdditionComplete onTrackAdditionComplete;

        public PlaylistManager()
        {
            Directory.CreateDirectory(Paths.RootDataFolder(LocationMode.PlayerData));
            Directory.CreateDirectory(Paths.WorkoutDefinitionFolder(LocationMode.PlayerData, Game.BoxVR));
            Directory.CreateDirectory(Paths.TrackDataFolder(LocationMode.PlayerData));
            Directory.CreateDirectory(Paths.DefinitionsFolder(LocationMode.PlayerData));
            Directory.CreateDirectory(Paths.ImgDataFolder(LocationMode.PlayerData));
            this.playlistsPath = Paths.WorkoutDefinitionFolder(LocationMode.PlayerData, Game.BoxVR);
            if(Directory.Exists(this.playlistsPath))
                return;
            Directory.CreateDirectory(this.playlistsPath);
        }


        public List<WorkoutPlaylist> GetAllWorkoutPlaylists() => this.workoutPlaylists;

        public List<string> ReturnAllPlaylistsFromDisk() => !Directory.Exists(this.playlistsPath) ? new List<string>() : new List<string>((IEnumerable<string>)Tools.IO.GetVisibleFiles(this.playlistsPath, "*.play"));

        public void LoadWorkoutPlaylists()
        {
            this.workoutPlaylists = this.LoadAllWorkoutPlaylistsFromDisk();
            LoadPlaylistTrackDefinition();
            foreach(var playlist in workoutPlaylists)
            {
                playlist.CalcTotalLength();
            }
        }

        private void LoadPlaylistTrackDefinition()
        {
            while(TrackDataManager.instance == null) { }
            while(!TrackDataManager.instance.trackListLoaded) { }
            for(int index1 = 0; index1 < this.workoutPlaylists.Count; ++index1)
            {
                for(int index2 = 0; index2 < this.workoutPlaylists[index1].songs.Count; ++index2)
                {
                    this.workoutPlaylists[index1].songs[index2].trackDefinition = new TrackDefinition();
                    this.workoutPlaylists[index1].songs[index2].trackDefinition.LoadTrackDefinition(this.workoutPlaylists[index1].songs[index2].trackDataName, LocationMode.PlayerData);
                }
            }
        }

        public List<WorkoutPlaylist> LoadAllWorkoutPlaylistsFromDisk()
        {
            List<WorkoutPlaylist> workoutPlaylistList = new List<WorkoutPlaylist>();
            if(Directory.Exists(this.playlistsPath))
            {
                foreach(string visibleFile in Tools.IO.GetVisibleFiles(this.playlistsPath, "*.workoutplaylist.txt"))
                {
                    WorkoutPlaylist workoutPlaylist = new WorkoutPlaylist();
                    string jsonString = File.ReadAllText(visibleFile);
                    workoutPlaylist.LoadFromJSON(jsonString);
                    workoutPlaylistList.Add(workoutPlaylist);
                }
            }
            return workoutPlaylistList;
        }

        public void Saveplaylists()
        {

            for(int index = 0; index < this.workoutPlaylists.Count; ++index)
                this.ExportPlaylistJson(this.workoutPlaylists[index]);
        }

        public void ExportPlaylistJson(WorkoutPlaylist playlist)
        {
            playlist.CalcTotalLength();
            if(playlist.definition.originalName != playlist.definition.workoutName)
            {
                DeleteJsonFile(playlist.definition.originalName);
            }
            File.WriteAllText(Path.Combine(this.playlistsPath, playlist.definition.workoutName + ".workoutplaylist.txt"), JsonConvert.SerializeObject(playlist));
        }

        public string PlaylistJsonFromName(string playlistName)
        {
            string str1 = "";
            string str2 = Path.Combine(this.playlistsPath, playlistName + ".play");
            if((!File.Exists(str2) ? 0 : (new FileInfo(str2).Length > 0L ? 1 : 0)) != 0)
                str1 = File.ReadAllText(str2);
            return str1;
        }

        public void DeletePlaylist(string playlistName)
        {
            WorkoutPlaylist playlistByName = this.GetPlaylistByName(playlistName);
            if(playlistByName == null)
                return;
            this.DeletePlaylist(playlistByName);
        }

        public void DeletePlaylist(WorkoutPlaylist playlist)
        {
            this.DeleteJsonFile(playlist.definition.workoutName);
            this.workoutPlaylists.Remove(playlist);
            //this.Saveplaylists();
        }

        public List<TrackDefinition> ReturnPlaylistTrackDefinitions(
          WorkoutPlaylist playlist)
        {
            List<TrackDefinition> trackDefinitionList = new List<TrackDefinition>();
            if(playlist == null || playlist.songs == null)
                return (List<TrackDefinition>)null;
            for(int index = 0; index < playlist.songs.Count; ++index)
            {
                playlist.songs[index].trackDefinition = TrackDataManager.instance.GetTrackDefinition(playlist.songs[index].trackDataName);
                trackDefinitionList.Add(playlist.songs[index].trackDefinition);
            }
            return trackDefinitionList;
        }

        public void PlaylistEditName(WorkoutPlaylist playlist, string newName)
        {
            string workoutName = playlist.definition.workoutName;
            if(newName == workoutName)
                return;
            playlist.definition.workoutName = newName;
            this.ExportPlaylistJson(playlist);
            this.DeleteJsonFile(workoutName);
        }

        private void DeleteJsonFile(string workoutName) => File.Delete(Path.Combine(this.playlistsPath, workoutName + ".workoutplaylist.txt"));

        public SongDefinition PlaylistAddEntry(WorkoutPlaylist playlist, string fileName, LocationMode location)
        {
            if(this.playlistQueue == null)
                this.playlistQueue = new List<PlaylistManager.PlaylistToAdd>();
            PlaylistManager.PlaylistToAdd playlistToAdd = new PlaylistManager.PlaylistToAdd()
            {
                playlistToAdd = playlist,
                songToAdd = new SongDefinition()
            };
            playlistToAdd.songToAdd.trackDefinition = new TrackDefinition();
            playlistToAdd.originalFilePath = fileName;
            playlistToAdd.location = location;
            this.playlistQueue.Add(playlistToAdd);
            if(this.isProcessing)
                throw new Exception("Can't batch add yet");
            this.PlaylistAddProcess();
            return playlistToAdd.songToAdd;
        }

        public void PlaylistAddProcess()
        {
            this.isProcessing = true;
            string originalFilePath = this.playlistQueue[0].originalFilePath;
            LocationMode location = this.playlistQueue[0].location;
            App.logger.Debug("Location mode = " + location.ToString());
            var trackHash = MD5.MD5Sum(Path.GetFileNameWithoutExtension(originalFilePath));
            if(this.playlistQueue[0].songToAdd.trackDefinition.LoadTrackDefinition(trackHash, location))
            {
                this.playlistQueue[0].songToAdd.trackDataName = this.playlistQueue[0].songToAdd.trackDefinition.trackId.trackId;
                this.playlistQueue[0].playlistToAdd.AddSong(this.playlistQueue[0].songToAdd);
                this.ExportPlaylistJson(this.playlistQueue[0].playlistToAdd);
                this.WaitingOver();
            }
            else
            {
                this.playlistQueue[0].songToAdd.trackDefinition.trackData = new TrackData();
                this.playlistQueue[0].songToAdd.trackDefinition.trackData.PopulateTrackDataFromAudioFile(originalFilePath, location, Game.BoxVR);
                WaitForTrackData();
            }
        }

        private void WaitForTrackData()
        {
            while(this.playlistQueue[0].songToAdd.trackDefinition.trackData.trackDataState == TrackDataState.Loading) { }
            if(this.playlistQueue[0].songToAdd.trackDefinition.trackData.trackDataState == TrackDataState.Ready)
            {
                this.playlistQueue[0].songToAdd.trackDataName = this.playlistQueue[0].songToAdd.trackDefinition.trackData.trackId.trackId;
                this.playlistQueue[0].songToAdd.trackDefinition.LoadTrackDefinition(playlistQueue[0].songToAdd.trackDataName, playlistQueue[0].location);
                this.playlistQueue[0].playlistToAdd.AddSong(this.playlistQueue[0].songToAdd);
                this.ExportPlaylistJson(this.playlistQueue[0].playlistToAdd);
            }
            else
                App.logger.Debug("TrackData failed");
            this.WaitingOver();
        }

        private void WaitingOver()
        {
            this.playlistQueue.RemoveAt(0);
            this.isProcessing = false;
            if(this.playlistQueue.Count > 0)
            {
                this.PlaylistAddProcess();
            }
            else
            {
                if(this.onTrackAdditionComplete == null)
                    return;
                this.onTrackAdditionComplete();
                this.onTrackAdditionComplete = (PlaylistManager.OnTrackAdditionComplete)null;
            }
        }

        public int ParsingQueueSize() => this.playlistQueue == null ? 0 : this.playlistQueue.Count;

        public void PlayistRemoveEntry(WorkoutPlaylist playlist, string filePath)
        {
            string withoutExtension = Path.GetFileNameWithoutExtension(filePath);
            for(int index = 0; index < playlist.songs.Count; ++index)
            {
                if(playlist.songs[index].trackDataName == withoutExtension)
                {
                    playlist.RemoveSong(index);
                    this.ExportPlaylistJson(playlist);
                    break;
                }
            }
        }

        public WorkoutPlaylist AddNewPlaylist(string name = "")
        {
            if(this.workoutPlaylists == null)
                this.workoutPlaylists = new List<WorkoutPlaylist>();
            if(name == "")
                name = this.GeneratePlaylistName();
            WorkoutPlaylist workoutPlaylist = new WorkoutPlaylist()
            {
                definition = new WorkoutInfo()
            };
            workoutPlaylist.definition.workoutName = name;
            workoutPlaylist.definition.originalName = name;
            workoutPlaylist.definition.workoutType = WorkoutType.BoxVR_Playlist;
            this.workoutPlaylists.Add(workoutPlaylist);
            return workoutPlaylist;
        }

        private string GeneratePlaylistName()
        {
            string str = "playlist";
            string id = str;
            int num = 1;
            while(this.IsIdInPlaylists(id))
            {
                id = str + " " + num.ToString();
                ++num;
            }
            return id;
        }

        private bool IsIdInPlaylists(string id)
        {
            foreach(WorkoutPlaylist workoutPlaylist in this.workoutPlaylists)
            {
                if(workoutPlaylist.definition.workoutName == id)
                    return true;
            }
            return false;
        }

        public WorkoutPlaylist GetPlaylistByName(string name)
        {
            if(this.workoutPlaylists != null)
            {
                for(int index = 0; index < this.workoutPlaylists.Count; ++index)
                {
                    if(this.workoutPlaylists[index].definition.workoutName == name)
                        return this.workoutPlaylists[index];
                }
            }
            return (WorkoutPlaylist)null;
        }

        public void ClearOrphanedFile(string playlistName)
        {
            File.Delete(Path.Combine(this.playlistsPath, playlistName + ".play"));
        }

        public void ShufflePlaylistTracks(WorkoutPlaylist playlist) => playlist.songs.Shuffle<SongDefinition>();

        public List<string> GetAllOldSaves()
        {
            string path = Path.Combine(Paths.PersistentDataPath, "Playlists");
            return !Directory.Exists(path) ? new List<string>() : new List<string>((IEnumerable<string>)Tools.IO.GetVisibleFiles(path, "*.playlist.txt"));
        }

        public List<string> ReturnSongPathsFromOldSavePath(string SaveFilePath)
        {
            List<string> stringList = new List<string>();
            foreach(string readAllLine in File.ReadAllLines(SaveFilePath))
            {
                if(File.Exists(readAllLine))
                    stringList.Add(readAllLine);
            }
            return stringList;
        }

        private void ConvertLegacyPlaylistsToGen3()
        {
        }

        public class PlaylistToAdd
        {
            public WorkoutPlaylist playlistToAdd;
            public SongDefinition songToAdd;
            public string originalFilePath;
            public LocationMode location;
        }

        public delegate void OnTrackAdditionComplete();
    }
}