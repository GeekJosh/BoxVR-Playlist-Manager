using System;
using System.IO;
using BoxVR_Playlist_Manager.FitXr.Enums;

namespace BoxVR_Playlist_Manager.Helpers
{
    public class Paths
    {
        private static string _applicationPath => Environment.ExpandEnvironmentVariables(Properties.Settings.Default.BoxVRExePath);
        private static string _persistentDataPath => Environment.ExpandEnvironmentVariables(Properties.Settings.Default.BoxVRAppDataPath);

        public static string StreamingAssetsPath => Path.Combine(_applicationPath, "BoxVR_Data", "StreamingAssets");

        public static string PersistentDataPath => _persistentDataPath;

        public static string ApplicationPath => _applicationPath;

        public static string RootDataFolder(LocationMode locationMode)
        {
            string str = "";
            switch(locationMode)
            {
                case LocationMode.PlayerData:
                    str = _persistentDataPath + "/Playlists";
                    break;
                case LocationMode.Workouts:
                case LocationMode.Downloadable:
                case LocationMode.MyWorkout:
                    App.logger.Error("No root path for these locations");
                    break;
                case LocationMode.Editor:
                    str = _persistentDataPath + "/WorkoutEditor";
                    break;
            }
            return str.Replace("/", "\\");
        }


        public static string WorkoutDefinitionFolder(LocationMode locationMode, Game gameType)
        {
            string str = "";
            switch(locationMode)
            {
                case LocationMode.PlayerData:
                case LocationMode.Editor:
                    str = Paths.RootDataFolder(locationMode) + "/WorkoutPlaylists/" + gameType.ToString();
                    break;
                case LocationMode.Workouts:
                    str = "WorkoutData/WorkoutPlaylists/" + gameType.ToString();
                    break;
            }
            return str.Replace("/", "\\");
        }

        public static string TrackDataFolder(LocationMode locationMode)
        {
            string str = "";
            switch(locationMode)
            {
                case LocationMode.PlayerData:
                case LocationMode.Editor:
                    str = Paths.RootDataFolder(locationMode) + "/TrackData/";
                    break;
                case LocationMode.Workouts:
                case LocationMode.MyWorkout:
                    str = "WorkoutData/TrackData/";
                    break;
            }
            return str.Replace("/", "\\");
        }

        public static string WavDataFolder(LocationMode locationMode) => Paths.TrackDataFolder(locationMode);

        public static string ImgDataFolder(LocationMode locationMode) => Paths.TrackDataFolder(locationMode);

        public static string DefinitionsFolder(LocationMode locationMode)
        {
            string str = "";
            switch(locationMode)
            {
                case LocationMode.PlayerData:
                case LocationMode.Editor:
                    str = Paths.RootDataFolder(locationMode) + "/TrackDefinitions/";
                    break;
                case LocationMode.Workouts:
                case LocationMode.MyWorkout:
                    str = "WorkoutData/TrackDefinitions/";
                    break;
            }
            return str.Replace("/", "\\");
        }
    }
}
