using System;
using System.Collections.Generic;

namespace BoxVRPlaylistManagerNETCore.FitXr.BeatStructure
{
    public class BarList
    {
        public List<Bar> _bars;

        protected float CalcMedianLength()
        {
            List<Bar> barList = new List<Bar>((IEnumerable<Bar>)this._bars);
            barList.Sort((Comparison<Bar>)((x, y) => (int)Math.Sign(x._duration - y._duration)));
            return barList[barList.Count / 2]._duration;
        }

        public void HalfSpeed()
        {
            for(int index = 0; index < this._bars.Count - 1; ++index)
            {
                Bar bar1 = this._bars[index];
                Bar bar2 = this._bars[index + 1];
                bar1._duration += bar2._duration;
                bar1._avgEnergy = (float)(((double)bar1._avgEnergy + (double)bar2._avgEnergy) / 2.0);
                this._bars.RemoveAt(index + 1);
            }
        }

        public void DoubleSpeed(BeatList beats)
        {
            for(int index = 0; index < this._bars.Count; index += 2)
            {
                Bar bar1 = this._bars[index];
                Bar bar2 = new Bar();
                this._bars.Insert(index + 1, bar2);
                float num = bar1._duration / 2f;
                bar1._duration = num;
                bar2._duration = num;
                bar2._startTime = bar1._startTime + num;
            }
        }

        public void UpdateEnergies(List<BeatInfo> beats)
        {
            for(int index1 = 0; index1 < this._bars.Count; ++index1)
            {
                Bar bar = this._bars[index1];
                for(int index2 = 0; index2 <= 3; ++index2)
                {
                    int index3 = bar._beatIndex + index2;
                    if(index3 < beats.Count && beats[index3]._beatInBar == index2 + 1)
                        bar._avgEnergy += beats[index3]._magnitude;
                }
                bar._avgEnergy /= 4f;
            }
        }

        public void FillEmptyRegions(float deltaBpm, float songLength)
        {
            float duration1 = this.CalcMedianLength();
            while(true)
            {
                float num = this._bars[0]._startTime - duration1;
                if((double)num >= 0.0)
                    this._bars.Insert(0, new Bar()
                    {
                        _duration = duration1,
                        _startTime = num
                    });
                else
                    break;
            }
            float duration2 = Bar.BpmToDuration(Bar.DurationToBpm(duration1) + deltaBpm);
            for(int index = 0; index < this._bars.Count - 1; ++index)
            {
                double startTime = (double)this._bars[index + 1]._startTime;
                float endTime = this._bars[index].EndTime;
                double num1 = (double)endTime;
                float num2 = (float)(startTime - num1);
                if((double)num2 >= (double)duration1)
                    this._bars.Insert(index + 1, new Bar()
                    {
                        _duration = duration1,
                        _startTime = endTime
                    });
                else if((double)num2 >= (double)duration2)
                    this._bars.Insert(index + 1, new Bar()
                    {
                        _duration = num2,
                        _startTime = endTime
                    });
                else if((double)num2 > 0.0)
                    this._bars[index]._duration += num2;
            }
            while(true)
            {
                Bar bar = this._bars[this._bars.Count - 1];
                if((double)bar.EndTime + (double)duration1 <= (double)songLength)
                    this._bars.Add(new Bar()
                    {
                        _startTime = bar.EndTime,
                        _duration = duration1
                    });
                else
                    break;
            }
        }

        public void RemoveDeviants(float bpmDelta)
        {
            float num = 240f / this.CalcMedianLength();
            int index = 0;
            while(index < this._bars.Count)
            {
                Bar bar = this._bars[index];
                if((double)Math.Abs(bar.Bpm - num) > (double)bpmDelta)
                    this._bars.Remove(bar);
                else
                    ++index;
            }
        }

        public void CreateFromBeats(List<BeatInfo> beats)
        {
            this._bars = new List<Bar>();
            Bar bar = (Bar)null;
            for(int index = 0; index < beats.Count; ++index)
            {
                BeatInfo beat = beats[index];
                if(beat._beatInBar == 1)
                {
                    bar = new Bar()
                    {
                        _startTime = beat._triggerTime,
                        _beatIndex = index
                    };
                    this._bars.Add(bar);
                }
                if(bar != null && beat._beatInBar == 4)
                    bar._duration = beat._triggerTime + beat._beatLength - bar._startTime;
            }
            this.UpdateEnergies(beats);
        }

        public int FindClosestBar(float time, List<BeatInfo> beats)
        {
            float num1 = float.MaxValue;
            int num2 = -1;
            for(int index = 0; index < this._bars.Count; ++index)
            {
                float num3 = Math.Abs(this._bars[index]._startTime - time);
                if((double)num3 < (double)num1)
                {
                    num2 = index;
                    num1 = num3;
                }
            }
            return num2;
        }
    }
}
