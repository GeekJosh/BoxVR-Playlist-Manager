using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BoxVR_Playlist_Manager.FitXr.BeatStructure
{
    public class SegmentList
    {
        public List<Segment> _segments;

        public SegmentList(string[] csvLines, List<BeatInfo> beats, BarList bars)
        {
            this._segments = new List<Segment>();
            foreach(string csvLine in csvLines)
            {
                List<string> stringList = new List<string>((IEnumerable<string>)csvLine.Split(','));
                int num = stringList.Count - 1;
                if(num < 4)
                    App.logger.Error(("Not enough entries in line " + csvLine));
                this._segments.Add(new Segment(float.Parse(stringList[num - 3]), float.Parse(stringList[num - 2])));
            }
            this.AlignSegmentsWithBeats(ref this._segments, beats, bars);
            this.CalculateSegmentEnergies(ref this._segments, beats);
        }

        private void CalculateSegmentEnergies(ref List<Segment> segments, List<BeatInfo> beats)
        {
            for(int index1 = 0; index1 < segments.Count; ++index1)
            {
                float num1 = 0.0f;
                for(int index2 = 0; index2 < segments[index1]._numBeats; ++index2)
                {
                    int index3 = segments[index1]._startBeatIndex + index2;
                    num1 += beats[index3]._magnitude;
                }
                float num2 = num1 / (float)segments[index1]._numBeats;
                segments[index1]._averageEnergy = num2;
            }
            float[] numArray1 = new float[4]
            {
              0.1f,
              0.3f,
              0.3f,
              0.3f
            };
            int[] numArray2 = new int[4];
            List<Segment> segmentList = new List<Segment>((IEnumerable<Segment>)segments);
            segmentList.Sort((Comparison<Segment>)((x, y) => (int)Mathf.Sign(x._averageEnergy - y._averageEnergy)));
            int num3 = this._segments[this._segments.Count - 1]._startBeatIndex + this._segments[this._segments.Count - 1]._numBeats;
            int num4 = 0;
            for(int index = 0; index < 4; ++index)
            {
                if(index == 3)
                {
                    numArray2[index] = num3 - num4;
                }
                else
                {
                    numArray2[index] = (int)((double)num3 * (double)numArray1[index]);
                    num4 += numArray2[index];
                }
            }
            int index4 = 0;
            int[] numArray3 = new int[4] { 0, 0, 0, 0 };
            for(int index1 = 0; index1 < segmentList.Count; ++index1)
            {
                segmentList[index1]._energyLevel = (Segment.EnergyLevel)index4;
                numArray3[index4] += segmentList[index1]._numBeats;
                if(numArray3[index4] > numArray2[index4])
                {
                    ++index4;
                    numArray3[index4] = 0;
                }
            }
        }

        private int FindLargestNeighbourEnergyDelta(List<Bar> bars, int centerBarIndex, int range)
        {
            float[] numArray = new float[2 * (range + 1)];
            for(int index1 = -range - 1; index1 <= range; ++index1)
            {
                numArray[index1 + range + 1] = float.MinValue;
                int index2 = index1 + centerBarIndex;
                if(index2 < bars.Count && index2 >= 0)
                    numArray[index1 + range + 1] = bars[index2]._avgEnergy;
            }
            int length = 2 * range + 1;
            float[] array = new float[length];
            for(int index = 0; index < length; ++index)
                array[index] = Mathf.Abs(numArray[index + 1] - numArray[index]);
            float num = ((IEnumerable<float>)array).Max();
            return Array.IndexOf<float>(array, num) - range;
        }

        private void AlignSegmentsWithBeats(
          ref List<Segment> segments,
          List<BeatInfo> beats,
          BarList bars)
        {
            for(int index1 = 0; index1 < segments.Count; ++index1)
            {
                int closestBar = bars.FindClosestBar(segments[index1]._startTime, beats);
                int neighbourEnergyDelta = this.FindLargestNeighbourEnergyDelta(bars._bars, closestBar, 2);
                int index2 = Mathf.Clamp(closestBar + neighbourEnergyDelta, 0, bars._bars.Count - 1);
                int beatIndex = bars._bars[index2]._beatIndex;
                segments[index1]._startBeatIndex = beatIndex;
            }
            List<Segment> segmentList = new List<Segment>();
            List<int> intList = new List<int>();
            foreach(Segment segment in this._segments)
            {
                if(intList.Contains(segment._startBeatIndex))
                    segmentList.Add(segment);
                else
                    intList.Add(segment._startBeatIndex);
            }
            foreach(Segment segment in segmentList)
                this._segments.Remove(segment);
            for(int index = 0; index < segments.Count; ++index)
            {
                int num = (index < segments.Count - 1 ? segments[index + 1]._startBeatIndex : beats.Count) - segments[index]._startBeatIndex;
                segments[index]._numBeats = num;
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach(Segment segment in this._segments)
                stringBuilder.AppendLine(segment.ToString());
            return stringBuilder.ToString();
        }
    }
}
