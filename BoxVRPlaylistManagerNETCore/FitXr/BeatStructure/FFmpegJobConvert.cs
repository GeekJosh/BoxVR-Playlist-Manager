using System;

namespace BoxVRPlaylistManagerNETCore.FitXr.BeatStructure
{
    public class FFmpegJobConvert : FFmpegJob
    {
        public FFmpegJobConvert(string inputPath, string outputPath, Action<FFmpegJob> onFinished)
        {
            this._inputPath = inputPath;
            this._outputPath = outputPath;
            this._onFinished = onFinished;
        }

        public override string GetCommand() => string.Join(" ", new string[7]
        {
    "-y",
    "-i",
    "\"" + this._inputPath + "\"",
    "-ar 44100 ",
    "-acodec pcm_s16le ",
    "-nostdin -loglevel error",
    "\"" + this._outputPath + "\""
        });
    }
}
