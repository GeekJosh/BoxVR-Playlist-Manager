using System.IO;
using Newtonsoft.Json;

namespace BoxVRPlaylistManagerNETCore.Helpers
{
    public class JsonConfiguration
    {
        private string _configurationPath;
        public JsonConfiguration(string jsonPath)
        {
            _configurationPath = jsonPath;
            LoadConfiguration(jsonPath);
        }

        public JsonConfiguration()
        {

        }

        private string _boxVRExePath;
        private string _boxVRAppDataPath;
        public string BoxVRExePath
        {
            get => _boxVRExePath;
            set
            {
                if(_boxVRExePath != value)
                {
                    _boxVRExePath = value;
                    SaveConfiguration();
                }
            }
        }
        public string BoxVRAppDataPath 
        {
            get => _boxVRAppDataPath;
            set
            {
                if(_boxVRAppDataPath != value)
                {
                    _boxVRAppDataPath = value;
                    SaveConfiguration();
                }
            }
        }

        public void LoadConfiguration()
        {
            LoadConfiguration(_configurationPath);
        }

        public void LoadConfiguration(string jsonPath)
        {
            if(string.IsNullOrEmpty(jsonPath))
            {
                return;
            }
            if(!File.Exists(jsonPath))
            {
                return;
            }
            var fileString = File.ReadAllText(jsonPath);
            var config = JsonConvert.DeserializeObject<JsonConfiguration>(fileString);
            CopyProperties(config);
        }

        public void SaveConfiguration()
        {
            SaveConfiguration(_configurationPath);
        }

        public void SaveConfiguration(string jsonPath)
        {
            if(string.IsNullOrEmpty(jsonPath))
            {
                return;
            }
            var serializedConfig = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(jsonPath, serializedConfig);
        }

        private void CopyProperties(JsonConfiguration configuration)
        {
            _boxVRExePath = configuration.BoxVRExePath;
            _boxVRAppDataPath = configuration.BoxVRAppDataPath;
        }
    }
}
