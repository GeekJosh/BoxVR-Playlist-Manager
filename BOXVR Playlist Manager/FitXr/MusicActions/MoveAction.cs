namespace BoxVR_Playlist_Manager.FitXr.MusicActions
{
    public class MoveAction
    {
        public MoveType moveType;
        public MoveChannel moveChannel;
        public int sequenceBeat;
        public float actionOffset;

        public MoveAction(MoveType type, MoveChannel channel)
        {
            this.moveType = type;
            this.moveChannel = channel;
        }

        public override string ToString() => this.moveChannel.ToString() + "_" + this.moveType.ToString();
    }
}
