using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading;
using BoxVRPlaylistManagerNETCore.FitXr.Enums;
using BoxVRPlaylistManagerNETCore.Helpers;
using log4net;

namespace BoxVRPlaylistManagerNETCore.FitXr.BeatStructure
{
    public class FFmpegQueue
    {
        private ILog _log = LogManager.GetLogger(typeof(FFmpegQueue));
        public static FFmpegQueue instance = instance == null ? new FFmpegQueue() : instance;

        public void Queue(FFmpegJob job) => RunFFMPEG(job);

        public static string binaryPath => Path.Combine(Paths.StreamingAssetsPath, @"ffmpeg\bin\ffmpeg.exe");

        private void RunFFMPEG(FFmpegJob job)
        {
            Directory.CreateDirectory(Paths.TrackDataFolder(LocationMode.PlayerData));
            _log.Debug(("FFMPEG start: " + FFmpegQueue.binaryPath + " " + job.GetCommand()));
            bool done = false;
            string exepath = FFmpegQueue.binaryPath;
            string command = job.GetCommand();
            new Thread((ThreadStart)(() =>
            {
                string output = "";
                Process process = new Process();
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.FileName = exepath;
                process.StartInfo.Arguments = command;
                process.OutputDataReceived += (DataReceivedEventHandler)((s, e) => output += e.Data);
                process.ErrorDataReceived += (DataReceivedEventHandler)((s, e) => output += e.Data);
                process.Start();
                process.WaitForExit();
                process.Close();
                job._message = output;
                done = true;
            })).Start();
            while(!done) { }
            job._onFinished.Invoke(job);
            _log.Debug("FFMEG done");
        }

        public bool ExtractDurationFromFFmpegOutput(string output, out int minutes, out int seconds)
        {
            minutes = -1;
            seconds = -1;
            string str1 = "Duration: ";
            string str2 = "xx:xx:xx.xx";
            int startIndex = output.IndexOf(str1) + str1.Length;
            string[] strArray = output.Substring(startIndex, str2.Length).Split(':', '.');
            if(strArray == null || strArray.Length != 4)
                return false;
            int[] numArray = new int[strArray.Length];
            for(int index = 0; index < strArray.Length; ++index)
            {
                if(!int.TryParse(strArray[index], out numArray[index]))
                    return false;
            }
            minutes = numArray[0] * 60 + numArray[1];
            seconds = numArray[2];
            return true;
        }
    }
}
