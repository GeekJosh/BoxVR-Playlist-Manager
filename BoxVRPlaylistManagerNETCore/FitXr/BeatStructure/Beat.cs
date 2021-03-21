namespace BoxVRPlaylistManagerNETCore.FitXr.BeatStructure
{
    public class Beat
    {
        public float length;
        public float bpm;
        public int index;

        public Beat(float length, float bpm, int index)
        {
            this.length = length;
            this.bpm = bpm;
            this.index = index;
        }
    }
}
