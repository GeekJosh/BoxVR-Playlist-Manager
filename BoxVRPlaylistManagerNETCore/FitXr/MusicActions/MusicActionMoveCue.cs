namespace BoxVRPlaylistManagerNETCore.FitXr.MusicActions
{
    public class MusicActionMoveCue : MusicAction
    {
        public MoveAction moveAction;

        public MusicActionMoveCue(MoveAction action, float beat, double time)
        {
            this.moveAction = action;
            this.startTime = time;
            this.beatNumber = beat;
            this.musicActiontype = MusicActionType.MoveCue;
        }

        //public override void Perform() => MoveCueFactoryBoxVR.instance.Spawn(this.moveAction);
    }
}
