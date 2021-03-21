using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using BoxVRPlaylistManagerNETCore.Helpers;

namespace BoxVRPlaylistManagerNETCore.FitXr.BeatStructure
{
    public class VampProcess
    {
        public string[] _resultEntries;
        public StringBuilder output;
        private Process _process;
        private const bool SHOW_WINDOW = false;
        private char APO_LITERAL;
        private string outputPath;
        private string vampToolPath;
        private string vampPluginPath;
        private bool isProcessComplete;

        public VampProcess()
        {
            this.output = new StringBuilder();
            this.outputPath = Path.Combine(MadmomProcess.madmonOutputPath, "vampOutput.txt");
            this.vampToolPath = Path.Combine(Paths.StreamingAssetsPath, @"Vamp\sonic-annotator-v1.5-win32\start.bat");
            this.vampPluginPath = Path.Combine(Paths.StreamingAssetsPath, @"Vamp\Vamp Plugins");
            this.isProcessComplete = true;
        }

        public void Apply(
          string pluginId,
          string musicPath,
          Action onFinished)
        {
            ApplyCoroutine(pluginId, musicPath, onFinished);
        }

        public void ApplyCoroutine(
          string pluginId,
          string musicPath,
          Action onFinished)
        {
            VampProcess vampProcess = this;
            File.Delete(vampProcess.outputPath);
            string str = $" -d {pluginId} \"{musicPath}\" -w csv --csv-force --csv-one-file \"{vampProcess.outputPath}\"";
            vampProcess._process = new Process();
            vampProcess._process.StartInfo.FileName = vampProcess.vampToolPath;
            vampProcess._process.StartInfo.RedirectStandardOutput = true;
            vampProcess._process.StartInfo.UseShellExecute = false;
            vampProcess._process.StartInfo.CreateNoWindow = true;
            vampProcess._process.StartInfo.EnvironmentVariables["VAMP_PATH"] = vampProcess.vampPluginPath;
            vampProcess._process.StartInfo.Arguments = str;
            vampProcess._process.EnableRaisingEvents = true;
            // ISSUE: reference to a compiler-generated method
            //vampProcess._process.Exited += new EventHandler(vampProcess.\u003CApplyCoroutine\u003Eb__12_0);
            vampProcess.isProcessComplete = false;
            vampProcess._process.Start();
            while(!vampProcess.isProcessComplete && !vampProcess._process.HasExited) { }
            if((vampProcess._process.ExitCode != 0 ? 0 : (File.Exists(vampProcess.outputPath) ? 1 : 0)) != 0)
            {
                vampProcess._resultEntries = File.ReadAllLines(vampProcess.outputPath);
                vampProcess._process.Close();
                onFinished.Invoke();
            }
            else
            {
                vampProcess._process.Close();
                throw new Exception("PROCESOR_ANALYSISERROR: There was an error analyzing the music file.");
            }
        }

        private void OnDestroy()
        {
            if(this.isProcessComplete)
                return;
            this._process.Kill();
        }
    }
}
