using System;
using System.Collections.Generic;
using BoxVR_Playlist_Manager.FitXr.Tools;
using UnityEngine;

namespace BoxVR_Playlist_Manager.FitXr.BeatStructure
{
    public class BeatList
    {
        public List<BeatInfo> _beats;
        public float AverageBpm;

        private float CalcMedianBpm(List<BeatInfo> beatList)
        {
            List<BeatInfo> beatInfoList = new List<BeatInfo>((IEnumerable<BeatInfo>)beatList);
            beatInfoList.Sort((Comparison<BeatInfo>)((x, y) => (int)Mathf.Sign(x._bpm - y._bpm)));
            return beatInfoList[beatInfoList.Count / 2]._bpm;
        }

        public void CreateFromBars(BarList barList)
        {
            this._beats = new List<BeatInfo>();
            int index1 = 0;
            for(int index2 = 0; index2 < barList._bars.Count; ++index2)
            {
                Bar bar = barList._bars[index2];
                bar._beatIndex = index1;
                float startTime = bar._startTime;
                float num = bar._duration / 4f;
                for(int beatInBar = 1; beatInBar < 5; ++beatInBar)
                {
                    this._beats.Add(new BeatInfo(index1, startTime, -1f, -1f, -1f, beatInBar, false));
                    ++index1;
                    startTime += num;
                }
            }
            for(int index2 = 0; index2 < this._beats.Count; ++index2)
            {
                this._beats[index2]._beatLength = index2 >= this._beats.Count - 1 ? this._beats[index2 - 1]._beatLength : this._beats[index2 + 1]._triggerTime - this._beats[index2]._triggerTime;
                this._beats[index2]._bpm = 60f / this._beats[index2]._beatLength;
            }
            this.AverageBpm = this.CalcMedianBpm(this._beats);
            for(int index2 = 0; index2 < this._beats.Count; ++index2)
                this._beats[index2]._bpm = this.AverageBpm;
        }

        public void UpdateEnergiesFromLines(string[] lines)
        {
            int index1 = 0;
            int index2 = 0;
            this._beats[0]._magnitude = 0.0f;
            do
            {
                float val1 = -1f;
                float val2 = -1f;
                Format.FloatsFromCsvLine(lines[index1], out val1, out val2);
                while((double)val1 >= (double)this._beats[index2 + 1]._triggerTime)
                {
                    ++index2;
                    this._beats[index2]._magnitude = 0.0f;
                    if(index2 == this._beats.Count - 1)
                        break;
                }
                this._beats[index2]._magnitude += val2;
                ++index1;
            }
            while(index1 != lines.Length && index2 != this._beats.Count - 1);
        }

        public void CreateFromLines(string[] lines)
        {
            this._beats = new List<BeatInfo>();
            int index1 = 0;
            for(int index2 = 0; index2 < lines.Length; ++index2)
            {
                List<string> stringList = new List<string>((IEnumerable<string>)lines[index2].Split('\t'));
                float triggerTime = float.Parse(stringList[0]);
                int beatInBar = int.Parse(stringList[1]);
                this._beats.Add(new BeatInfo(index1, triggerTime, -1f, -1f, -1f, beatInBar, false));
                ++index1;
            }
            for(int index2 = 0; index2 < this._beats.Count - 1; ++index2)
            {
                this._beats[index2]._beatLength = this._beats[index2 + 1]._triggerTime - this._beats[index2]._triggerTime;
                this._beats[index2]._bpm = 60f / this._beats[index2]._beatLength;
            }
            if(this._beats.Count > 0)
            {
                this.AverageBpm = this.CalcMedianBpm(this._beats);
            }
            else
            {
                App.logger.Debug("MadMom failure");
                this.AverageBpm = 120f;
            }
            for(int index2 = 0; index2 < this._beats.Count; ++index2)
                this._beats[index2]._bpm = this.AverageBpm;
        }
    }
}
