namespace BoxVRPlaylistManagerNETCore.FitXr.MusicActions
{
    public class MusicActionBPM : MusicAction
    {
        public float segmentBPM;

        public MusicActionBPM(float trackBPM, double offsetOnArrival)
        {
            this.segmentBPM = trackBPM;
            this.startTime = offsetOnArrival;
            this.musicActiontype = MusicActionType.BPM;
        }
    }
}
