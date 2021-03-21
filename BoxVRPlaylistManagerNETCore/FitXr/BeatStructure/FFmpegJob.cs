using System;

namespace BoxVRPlaylistManagerNETCore.FitXr.BeatStructure
{
    public class FFmpegJob
    {
        public Action<FFmpegJob> _onFinished;
        public string _inputPath;
        public string _outputPath;
        public string _message;

        public virtual string GetCommand() => "";
    }
}
