using System;
using System.IO;
using BoxVRPlaylistManagerNETCore.FitXr.Enums;
using BoxVRPlaylistManagerNETCore.Helpers;
using log4net;
using Newtonsoft.Json;

namespace BoxVRPlaylistManagerNETCore.FitXr.Models
{
    public class TrackDefinition : TrackBase
    {
        public TrackDefinition() => this.audioClipStatus = AudioClipStatus.Pending;

        private ILog _log = LogManager.GetLogger(typeof(TrackDefinition));

        [JsonProperty("firstBeatStartDelay")]
        public double firstBeatStartDelay { get; set; }
        [JsonProperty("tagLibArtist")]
        public string tagLibArtist { get; set; }
        [JsonProperty("tagLibTitle")]
        public string tagLibTitle { get; set; }
        [JsonProperty("genreMask")]
        public TrackGenre genreMask { get; set; }
        [JsonProperty("audioClipStatus")]
        public AudioClipStatus audioClipStatus { get; set; }

        [JsonIgnore]
        public LocationMode locationMode;

        [JsonIgnore]
        public byte[] waveTextureData;
        [JsonIgnore]
        public TrackData trackData;

        [JsonIgnore]
        public TimeSpan DurationTimeSpan => TimeSpan.FromSeconds(duration);


        public void LoadTrackData()
        {
            if(this.trackData != null)
                return;
            this.trackData = new TrackData();
            this.trackData.LoadTrackData(this.trackId.trackId, this.locationMode);
        }

        public bool LoadTrackDefinition(string trackHash, LocationMode mode)
        {
            bool flag = false;
            this.locationMode = mode;

            string str1 = Paths.DefinitionsFolder(mode) + trackHash + ".wdef";
            string str2 = "";
            switch(this.locationMode)
            {
                case LocationMode.PlayerData:
                case LocationMode.Editor:
                    string path = str1 + ".txt";
                    if(System.IO.File.Exists(path))
                    {
                        str2 = System.IO.File.ReadAllText(path);
                        break;
                    }
                    break;
                    //TODO double cheeck as i think it needs .txt
                case LocationMode.Workouts:
                case LocationMode.MyWorkout:
                    str2 = System.IO.File.ReadAllText(str1);
                    break;
            }
            if(str2 != "")
            {
                //TrackDefinition trackDefinition = (TrackDefinition)JsonUtility.FromJson<TrackDefinition>(str2);
                var trackDefinition = JsonConvert.DeserializeObject<TrackDefinition>(str2);
                flag = true;
                this.trackId = trackDefinition.trackId;
                this.firstBeatStartDelay = trackDefinition.firstBeatStartDelay;
                this.tagLibTitle = trackDefinition.tagLibTitle;
                this.tagLibArtist = trackDefinition.tagLibArtist;
                this.duration = trackDefinition.duration;
                this.bpm = trackDefinition.bpm;
            }
            return flag;
        }

        public bool SaveTrackDefinition()
        {
            if(this.locationMode == LocationMode.MyWorkout || this.locationMode == LocationMode.Workouts)
            {
                _log.Error("Cant save to resourses you potatoe head !!!!");
                return false;
            }
            Directory.CreateDirectory(Paths.DefinitionsFolder(this.locationMode));
            //string json = JsonUtility.ToJson((object)this, true);
            string json = JsonConvert.SerializeObject(this);
            File.WriteAllText(Paths.DefinitionsFolder(this.locationMode) + this.trackId.trackId + ".wdef.txt", json);
            return true;
        }

        //public IEnumerator GenerateWavImageData(int numSamples = 131071)
        //{
        //    if(this.audioClipStatus != AudioClipStatus.Loaded)
        //        yield return (object)this.LoadAudioClip();
        //    while(this.audioClipStatus != AudioClipStatus.Loaded || this.audioClipStatus != AudioClipStatus.Failed)
        //        yield return (object)null;
        //    if(Object.op_Implicit((Object)this.audioClip))
        //    {
        //        Debug.Log((object)"generating wave image data");
        //        this.waveTextureData = new byte[numSamples];
        //        float[] audioSamples = new float[this.audioClip.get_samples() * this.audioClip.get_channels()];
        //        this.audioClip.LoadAudioData();
        //        while(this.audioClip.get_loadState() != 2)
        //            yield return (object)null;
        //        this.audioClip.GetData(audioSamples, 0);
        //        double num1 = (double)(audioSamples.Length / this.waveTextureData.Length);
        //        for(int index1 = 0; index1 < this.waveTextureData.Length; ++index1)
        //        {
        //            double num2 = (double)index1 * num1 - num1 / 2.0;
        //            double num3 = num2 + num1;
        //            float num4 = 0.0f;
        //            for(int index2 = (int)num2; index2 < (int)num3; ++index2)
        //            {
        //                if(index2 > 0 && index2 < audioSamples.Length && (double)Mathf.Abs(audioSamples[index2]) > (double)num4)
        //                    num4 = Mathf.Abs(audioSamples[index2]);
        //            }
        //            this.waveTextureData[index1] = (byte)((double)num4 * 256.0);
        //        }
        //        if(this.locationMode == LocationMode.Editor)
        //        {
        //            File.WriteAllBytes(Paths.ImgDataFolder(this.locationMode) + this.trackId.trackId + ".imgdata.bytes", this.waveTextureData);
        //            Debug.Log((object)"Written wave image data");
        //        }
        //        audioSamples = (float[])null;
        //    }
        //}
    }
}
