namespace BoxVR_Playlist_Manager.FitXr.MusicActions
{
    public class MusicActionAudio : MusicAction
    {
        public string wavFilePath;
        public string trackId;

        public MusicActionAudio(string wavPath, string trackid, double startOffset)
        {
            this.wavFilePath = wavPath;
            this.startTime = startOffset;
            this.trackId = trackid;
            this.musicActiontype = MusicActionType.Audio;
        }

        public override void Perform()
        {
        }
    }
}
