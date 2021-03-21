using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading;
using BoxVR_Playlist_Manager.FitXr.Enums;
using BoxVR_Playlist_Manager.Helpers;

namespace BoxVR_Playlist_Manager.FitXr.BeatStructure
{
    public class FFmpegQueue
    {
        public static FFmpegQueue instance = instance == null ? new FFmpegQueue() : instance;

        public void Queue(FFmpegJob job) => RunFFMPEG(job);

        public static string binaryPath => Path.Combine(Paths.StreamingAssetsPath, @"ffmpeg\bin\ffmpeg.exe");

        private void RunFFMPEG(FFmpegJob job)
        {
            Directory.CreateDirectory(Paths.TrackDataFolder(LocationMode.PlayerData));
            App.logger.Debug(("FFMPEG start: " + FFmpegQueue.binaryPath + " " + job.GetCommand()));
            CancellationTokenSource doneCts = new CancellationTokenSource();
            var done = doneCts.Token;
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
                doneCts.Cancel();
            })).Start();
            while(!done.IsCancellationRequested) { }
            job._onFinished.Invoke(job);
            App.logger.Debug("FFMEG done");
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
