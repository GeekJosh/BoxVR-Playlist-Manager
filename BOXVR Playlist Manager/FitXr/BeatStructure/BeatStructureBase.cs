using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BoxVR_Playlist_Manager.FitXr.BeatStructure
{
    public class BeatStructureBase
    {

        public event EventHandler _buildCompleteEvent;

        protected bool _isBuilt;

        public virtual List<BeatInfo> Beats => (List<BeatInfo>)null;

        [JsonProperty("AverageBpm")]
        public float AverageBpm { get; protected set; }

        [JsonProperty("_isBuilt")]
        public bool IsBuilt => this._isBuilt;

        public virtual void Build()
        {
        }

        public virtual BeatInfo GetBeatInInterval(float fromTime, float toTime)
        {
            foreach(BeatInfo beat in this.Beats)
            {
                if((double)beat._triggerTime >= (double)fromTime && (double)beat._triggerTime < (double)toTime)
                    return beat;
            }
            return (BeatInfo)null;
        }

        protected float CalcAverageBpm(List<BeatInfo> beatList)
        {
            float num = 0.0f;
            foreach(BeatInfo beat in this.Beats)
                num += beat._bpm;
            return num / (float)this.Beats.Count;
        }

        protected void RaiseBuildCompleteEvent()
        {
            _buildCompleteEvent?.Invoke(this, EventArgs.Empty);
        }

        public override string ToString()
        {
            string str = "";
            foreach(BeatInfo beat in this.Beats)
                str += beat.ToString();
            return str;
        }
    }
}
