using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using BoxVRPlaylistManagerNETCore.FitXr.Enums;
using BoxVRPlaylistManagerNETCore.FitXr.Models;
using BoxVRPlaylistManagerNETCore.Helpers;
using log4net;
using Newtonsoft.Json;

namespace BoxVRPlaylistManagerNETCore.FitXr.BeatStructure
{
    class TrackDataManager
    {
        private static ILog _log = LogManager.GetLogger(nameof(TrackDataManager));
        public static TrackDataManager instance = instance == null ? new TrackDataManager() : instance;
        private TagLib.File audioTrack;
        public string currentPath;
        private Game currentGameType;
        private List<TrackData> myWorkoutTrackDataList;
        private List<TrackDefinition> trackDefinitionsResources;
        private List<TrackDefinition> trackDefinitionsEditor;
        private List<TrackDefinition> trackDefinitionsPlayer;
        private List<TrackDefinition> trackDefinitionsDLC;
        private string originalFileNameBackup;
        private bool trackDataLoadComplete;
        public bool trackListLoaded => this.trackDataLoadComplete;

        public event TrackDataManager.OnTrackDataComplete onTrackDataComplete;

        public TrackDataManager()
        { 
            LoadTrackDefinitionLists();
        }

        public string GetLocationpathPath(
          TrackId trackId,
          LocationMode locationMode,
          Game gameType)
        {
            string str = Paths.TrackDataFolder(locationMode);
            switch(locationMode)
            {
                case LocationMode.PlayerData:
                case LocationMode.Editor:
                    if(!Directory.Exists(Paths.RootDataFolder(locationMode)))
                        Directory.CreateDirectory(Paths.RootDataFolder(locationMode));
                    str = str + trackId.trackId + ".trackdata.txt";
                    break;
                case LocationMode.Workouts:
                case LocationMode.Downloadable:
                case LocationMode.MyWorkout:
                    str = str + trackId.trackId + ".trackdata";
                    break;
            }
            return str;
        }

        public bool DoesTrackDataExist(string filePath, LocationMode locationMode, Game gameType)
        {
            TrackId trackId = new TrackId(filePath);
            string locationpathPath = this.GetLocationpathPath(trackId, locationMode, gameType);

            return System.IO.File.Exists(locationpathPath) && new FileInfo(locationpathPath).Length > 0L;
        }

        public bool DoesTrackDataExistByID(
          TrackId trackId,
          LocationMode locationMode,
          Game gameType)
        {
            string locationpathPath = this.GetLocationpathPath(trackId, locationMode, gameType);
            return System.IO.File.Exists(locationpathPath) && new FileInfo(locationpathPath).Length > 0L;
        }

        public bool DoesFileExist(TrackId trackId, string originalFilePath)
        {
            string path = Path.Combine(MadmomProcess.madmonOutputPath, trackId.trackId + ".madmom.txt");
            bool flag1 = false;
            if(System.IO.File.Exists(path))
                flag1 = true;
            string locationpathPath = this.GetLocationpathPath(trackId, LocationMode.PlayerData, Game.BoxVR);
            bool flag2 = false;
            if((!System.IO.File.Exists(locationpathPath) ? 0 : (new FileInfo(locationpathPath).Length > 0L ? 1 : 0)) != 0)
                flag2 = true;
            if(flag1 && !flag2)
            {
                this.LegacyToTrackData(trackId.trackId, originalFilePath);
                this.ClearLegacyFiles(trackId.trackId);
                return true;
            }
            return flag2;
        }

        public TrackData LegacyToTrackData(string trackMD5, string originalFilePath)
        {
            TrackData trackData = new TrackData();
            var file = TagLib.File.Create(originalFilePath);
            trackData.originalTrackName = file.Tag.Title;
            trackData.trackId = new TrackId();
            trackData.trackId.trackId = trackMD5;
            trackData.duration = (float)file.Properties.Duration.TotalSeconds;
            trackData.locationMode = LocationMode.PlayerData;
            trackData.originalFilePath = originalFilePath;
            if(string.IsNullOrEmpty(trackData.originalTrackName))
                trackData.originalTrackName = Path.GetFileNameWithoutExtension(originalFilePath);
            BeatStructureMadmom beatStructure = this.ReadBeatStructureFromFile(trackMD5);
            trackData.bpm = beatStructure.AverageBpm;
            trackData.beatStrucureJSON = this.BeatStructureToJSON(beatStructure);
            this.SaveTrackData(trackData);
            return trackData;
        }

        public TrackData NewTrackData(
          UserSongClip clip,
          BeatStructureMadmom bsm,
          LocationMode locationMode)
        {
            TrackData trackData = new TrackData();
            trackData.originalFilePath = clip.originalFilePath;
            trackData.originalTrackName = clip.trackData.originalTrackName;
            trackData.originalArtist = clip.trackData.originalArtist;
            trackData.trackId = new TrackId(clip.originalFilePath);
            trackData.bpm = bsm._beatList.AverageBpm;
            trackData.beatStrucureJSON = BeatStructureToJSON(bsm);
            trackData.duration = clip.trackData.duration;
            trackData.locationMode = locationMode;
            trackData.firstBeatOffset = bsm.Beats[0]._triggerTime;
            SaveTrackData(trackData);
            TrackDefinition trackDefinition = new TrackDefinition();
            trackDefinition.locationMode = locationMode;
            trackDefinition.trackId = trackData.trackId;
            trackDefinition.tagLibTitle = trackData.originalTrackName;
            trackDefinition.tagLibArtist = trackData.originalArtist;
            trackDefinition.duration = trackData.duration;
            trackDefinition.bpm = trackData.bpm;
            trackDefinition.trackData = trackData;
            trackDefinition.SaveTrackDefinition();
            this.trackDefinitionsPlayer.Add(trackDefinition);
            return trackData;
        }

        public void PopulateTrackDataFromAudioFile(string filePath, LocationMode locationMode, string originalFilePath, bool converted = false)
        {
            _log.Debug("Loading " + filePath);
            string extension = Path.GetExtension(filePath);
            if(extension == ".wav")
            {
                string filename = Path.GetFileNameWithoutExtension(filePath);
                //Should never happen, but if we have a wav with a non md5 name, make it md5
                if(!converted)
                {
                    TrackId trackId = new TrackId(filePath);
                    filename = trackId.trackId;
                }
                string str = Paths.WavDataFolder(locationMode) + filename + ".wav";
                if(!System.IO.File.Exists(str) && filePath != str)
                {
                    System.IO.File.Copy(filePath, str);
                }
                LoadWav(str, locationMode, originalFilePath);
            }
            else
            {
                if(!(extension == ".mp3") && !(extension == ".m4a") && !(extension == ".ogg"))
                {
                    return;
                }
                
                TrackId trackId = new TrackId(filePath);
                string outputPath = Paths.WavDataFolder(locationMode) + trackId.trackId + ".wav";
                //Convert to WAV
                FFmpegQueue.instance.Queue((FFmpegJob)new FFmpegJobConvert(filePath, outputPath, OnConvertedToWav));
            }
        }

        private void OnConvertedToWav(FFmpegJob job)
        {
            _log.Debug("FFMPEG complete: " + job._outputPath);
            LocationMode locationMode = LocationMode.PlayerData;
            this.originalFileNameBackup = Path.GetFileNameWithoutExtension(job._inputPath);
            this.PopulateTrackDataFromAudioFile(job._outputPath, locationMode, job._inputPath, true);
        }

        public void WavLoaded(TagLib.File track, string wavPath, LocationMode locationMode)
        {
            _log.Debug("Wav Loaded");
            this.audioTrack = track;
            SongParser.instance.onParsingComplete += new SongParser.OnParsingComplete(OnParsingComplete);
            SongParser.instance.CreateSongClipFromAudioClip(audioTrack, wavPath, locationMode);
        }

        private void OnParsingComplete(TrackData trackdata)
        {
            SongParser.instance.onParsingComplete -= new SongParser.OnParsingComplete(this.OnParsingComplete);
            _log.Debug("Track data created");
            onTrackDataComplete?.Invoke(trackdata);
        }

        public void ClearLegacyFiles(string trackMD5)
        {
            string path1 = Path.Combine(MadmomProcess.madmonOutputPath, trackMD5 + ".madmom.txt");
            if(System.IO.File.Exists(path1))
                System.IO.File.Delete(path1);
            string path2 = Path.Combine(MadmomProcess.madmonOutputPath, trackMD5 + ".beatstructuremadmom.txt");
            if(!System.IO.File.Exists(path2))
                return;
            System.IO.File.Delete(path2);
        }

        public string GetMadmomTextFromFilename(string trackMD5)
        {
            string str1 = "";
            string str2 = Path.Combine(MadmomProcess.madmonOutputPath, trackMD5 + ".madmom.txt");
            if((!System.IO.File.Exists(str2) ? 0 : (new FileInfo(str2).Length > 0L ? 1 : 0)) != 0)
                str1 = System.IO.File.ReadAllText(str2);
            return str1;
        }

        public string GetBeatStructureXMLFromFilename(string trackMD5)
        {
            string str1 = "";
            string str2 = Path.Combine(MadmomProcess.madmonOutputPath, trackMD5 + ".beatstructuremadmom.txt");
            if((!System.IO.File.Exists(str2) ? 0 : (new FileInfo(str2).Length > 0L ? 1 : 0)) != 0)
                str1 = System.IO.File.ReadAllText(str2);
            return str1;
        }

        public void SaveTrackData(TrackData trackData) => System.IO.File.WriteAllText(this.GetLocationpathPath(trackData.trackId, trackData.locationMode, Game.BoxVR), JsonConvert.SerializeObject(trackData));

        public string TrackDataJsonFromFileName(TrackId trackId, LocationMode locationMode) => this.TrackDataJsonFromFilePath(this.GetLocationpathPath(trackId, locationMode, Game.BoxVR));

        public TrackData TrackDataFromJson(string json) => JsonConvert.DeserializeObject<TrackData>(json);

        public string TrackDataJsonFromFilePath(string filePath)
        {
            string str = "";
            if((!System.IO.File.Exists(filePath) ? 0 : (new FileInfo(filePath).Length > 0L ? 1 : 0)) != 0)
                str = System.IO.File.ReadAllText(filePath);
            return str;
        }
        public string BeatStructureToJSON(BeatStructureMadmom beatStructure) => JsonConvert.SerializeObject(beatStructure);

        public BeatStructureMadmom ReadBeatStructureFromFile(string trackMD5)
        {
            string inputUri = Path.Combine(MadmomProcess.madmonOutputPath, trackMD5 + ".beatstructuremadmom.txt");
            using(XmlReader reader = XmlReader.Create(inputUri))
                return (BeatStructureMadmom)new DataContractSerializer(typeof(BeatStructureMadmom)).ReadObject(reader);
        }

        public BeatStructureMadmom ReadBeatStructureFromJSON(string json) => JsonConvert.DeserializeObject<BeatStructureMadmom>(json);

        public void LoadWav(string wavPath, LocationMode locationMode, string originalFileName)
        {
            var atlTrack = TagLib.File.Create(originalFileName);
            WavLoaded(atlTrack, wavPath, locationMode);
        }

        public void LoadTrackDefinitionLists()
        {
            this.trackDataLoadComplete = false;
            this.trackDefinitionsResources = new List<TrackDefinition>();
            this.trackDefinitionsPlayer = new List<TrackDefinition>();
            this.trackDefinitionsEditor = new List<TrackDefinition>();
            this.myWorkoutTrackDataList = new List<TrackData>();
            this.trackDefinitionsDLC = new List<TrackDefinition>();
            string str = Paths.DefinitionsFolder(LocationMode.Workouts);
            int count;
            string path1 = Paths.DefinitionsFolder(LocationMode.PlayerData);
            string[] fileNames;
            if(Directory.Exists(path1))
            {
                fileNames = Directory.GetFiles(path1, "*.wdef.txt");
                for(count = 0; count < fileNames.Length; ++count)
                {
                    TrackDefinition trackDefinition = JsonConvert.DeserializeObject<TrackDefinition>(System.IO.File.ReadAllText(fileNames[count]));
                    if(this.GetTrackDefinition(trackDefinition.trackId.trackId) == null)
                    {
                        trackDefinition.locationMode = LocationMode.PlayerData;
                        this.trackDefinitionsPlayer.Add(trackDefinition);
                    }
                }
                fileNames = (string[])null;
            }
            string path2 = Paths.DefinitionsFolder(LocationMode.Editor);
            if(Directory.Exists(path2))
            {
                fileNames = Directory.GetFiles(path2, "*.wdef.txt");
                for(count = 0; count < fileNames.Length; ++count)
                {
                    TrackDefinition trackDefinition = JsonConvert.DeserializeObject<TrackDefinition>(System.IO.File.ReadAllText(fileNames[count]));
                    if(this.GetTrackDefinition(trackDefinition.trackId.trackId) == null)
                        this.trackDefinitionsEditor.Add(trackDefinition);
                    trackDefinition.locationMode = LocationMode.Editor;
                }
                fileNames = (string[])null;
            }
            this.trackDataLoadComplete = true;
        }


        public List<TrackDefinition> GetTrackDefinitionList()
        {
            List<TrackDefinition> trackDefinitionList = new List<TrackDefinition>();
            trackDefinitionList.AddRange((IEnumerable<TrackDefinition>)this.trackDefinitionsResources);
            trackDefinitionList.AddRange((IEnumerable<TrackDefinition>)this.trackDefinitionsPlayer);
            trackDefinitionList.AddRange((IEnumerable<TrackDefinition>)this.trackDefinitionsEditor);
            trackDefinitionList.AddRange((IEnumerable<TrackDefinition>)this.trackDefinitionsDLC);
            return trackDefinitionList;
        }

        public void UpdateDLCTrackDefinitionList(List<TrackDefinition> newList) => this.trackDefinitionsDLC = newList;

        public TrackDefinition GetTrackDefinition(string trackId)
        {
            TrackDefinition trackDefinition = (TrackDefinition)null;
            for(int index = 0; index < this.trackDefinitionsResources.Count; ++index)
            {
                if(this.trackDefinitionsResources[index].trackId.trackId == trackId)
                {
                    trackDefinition = this.trackDefinitionsResources[index];
                    break;
                }
            }
            if(trackDefinition == null)
            {
                for(int index = 0; index < this.trackDefinitionsPlayer.Count; ++index)
                {
                    if(this.trackDefinitionsPlayer[index].trackId.trackId == trackId)
                    {
                        trackDefinition = this.trackDefinitionsPlayer[index];
                        break;
                    }
                }
            }
            if(trackDefinition == null)
            {
                for(int index = 0; index < this.trackDefinitionsEditor.Count; ++index)
                {
                    if(this.trackDefinitionsEditor[index].trackId.trackId == trackId)
                    {
                        trackDefinition = this.trackDefinitionsEditor[index];
                        break;
                    }
                }
            }
            if(trackDefinition == null)
            {
                for(int index = 0; index < this.trackDefinitionsDLC.Count; ++index)
                {
                    if(this.trackDefinitionsDLC[index].trackId.trackId == trackId)
                    {
                        trackDefinition = this.trackDefinitionsDLC[index];
                        break;
                    }
                }
            }
            return trackDefinition;
        }

        public TrackData GetTrackDataByID(string trackDataId)
        {
            TrackDefinition trackDefinition = this.GetTrackDefinition(trackDataId);
            if(trackDefinition == null)
                return (TrackData)null;
            trackDefinition.LoadTrackData();
            return trackDefinition.trackData;
        }

        public delegate void OnWavLoaded(TagLib.File track, string filePath, LocationMode locationMode);

        public delegate void OnWavLoadFailed(string error);

        public delegate void OnTrackDataComplete(TrackData trackData);

        public delegate void OnTrackDataFailed(string error);

    }
}
