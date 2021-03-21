using System;
using System.Collections.Generic;
using System.ComponentModel;
using BoxVR_Playlist_Manager.FitXr.BeatStructure;
using BoxVR_Playlist_Manager.FitXr.MusicActions;
using Newtonsoft.Json;

namespace BoxVR_Playlist_Manager.FitXr.Models
{
    public class SongDefinition
    {
        public SongDefinition()
        {
            serialisedActionList = new MusicActionListSerializable();
        }

        [JsonProperty("trackDataName")]
        public string trackDataName { get; set; }

        //From my minor testing this has always contained an empty list
        [JsonProperty("serialisedActionList")]
        public MusicActionListSerializable serialisedActionList { get; set; }

        [JsonIgnore]
        public List<MusicAction> musicActionList;
        [JsonIgnore]
        public TrackDefinition trackDefinition { get; set; }

        public void EditMoveAction(MoveChannel channel, float beatNumber, MoveType moveType)
        {
            App.logger.Debug("Editing " + (object)channel + "/" + (object)beatNumber + " = " + (object)moveType);
            MusicActionMoveCue musicActionMoveCue = (MusicActionMoveCue)null;
            int index1 = -1;
            for(int index2 = 0; index2 < this.musicActionList.Count; ++index2)
            {
                if(this.musicActionList[index2] is MusicActionMoveCue)
                {
                    MusicActionMoveCue musicAction = (MusicActionMoveCue)this.musicActionList[index2];
                    if((double)musicAction.beatNumber == (double)beatNumber && musicAction.moveAction.moveChannel == channel)
                    {
                        index1 = index2;
                        musicActionMoveCue = musicAction;
                        break;
                    }
                    if(index1 == 0 && (double)this.musicActionList[index2].beatNumber > (double)beatNumber)
                        index1 = index2;
                }
            }
            if(moveType == MoveType.None)
            {
                if(index1 != -1)
                {
                    App.logger.Debug("Deleting entry at " + (object)index1);
                    this.musicActionList.RemoveAt(index1);
                }
            }
            else
            {
                if(musicActionMoveCue == null)
                {
                    musicActionMoveCue = new MusicActionMoveCue(new MoveAction(moveType, channel), beatNumber, 0.0);
                    if(index1 > 0)
                        this.musicActionList.Insert(index1, (MusicAction)musicActionMoveCue);
                    else
                        this.musicActionList.Add((MusicAction)musicActionMoveCue);
                }
                App.logger.Debug("modified cue");
                musicActionMoveCue.moveAction.moveType = moveType;
            }
            this.RecalculateCueTiming();
        }

        public void RecalculateCueTiming()
        {
            if(this.trackDefinition == null)
                this.trackDefinition = TrackDataManager.instance.GetTrackDefinition(this.trackDataName);
            this.musicActionList.Sort((Comparison<MusicAction>)((x, y) => x.beatNumber.CompareTo(y.beatNumber)));
            float num = 60f / this.trackDefinition.bpm;
            for(int index = 0; index < this.musicActionList.Count; ++index)
            {
                if(this.musicActionList[index] is MusicActionAudio)
                    this.musicActionList[index].startTime = 0.0;
                if(this.musicActionList[index] is MusicActionBPM)
                    this.musicActionList[index].startTime = 0.0;
                if(this.musicActionList[index] is MusicActionMoveCue)
                {
                    MusicActionMoveCue musicAction = (MusicActionMoveCue)this.musicActionList[index];
                    musicAction.startTime = (double)num * (double)musicAction.beatNumber + (double)this.trackDefinition.firstBeatStartDelay;
                }
            }
        }

        public MusicActionAudio GetFirstAudioAction()
        {
            if(this.musicActionList == null)
                return (MusicActionAudio)null;
            for(int index = 0; index < this.musicActionList.Count; ++index)
            {
                if(this.musicActionList[index] is MusicActionAudio)
                    return (MusicActionAudio)this.musicActionList[index];
            }
            return (MusicActionAudio)null;
        }
    }
}
