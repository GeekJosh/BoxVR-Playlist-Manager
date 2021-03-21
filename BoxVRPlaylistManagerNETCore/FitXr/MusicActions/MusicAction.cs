namespace BoxVRPlaylistManagerNETCore.FitXr.MusicActions
{
    public class MusicAction
    {
        public double startTime;
        public float beatNumber;
        public MusicActionType musicActiontype;

        public virtual void Perform()
        {
        }
    }
}
