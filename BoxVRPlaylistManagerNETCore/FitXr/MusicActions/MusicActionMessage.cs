namespace BoxVRPlaylistManagerNETCore.FitXr.MusicActions
{
    public class MusicActionMessage : MusicAction
    {
        public MessagePosition position;
        public string message;

        public MusicActionMessage(
          string message,
          MessagePosition position,
          double displayTime)
        {
            this.message = message;
            this.position = position;
            this.startTime = displayTime;
        }
    }
}
