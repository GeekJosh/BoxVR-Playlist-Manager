using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using BoxVRPlaylistManagerNETCore.Helpers;
using BoxVRPlaylistManagerNETCore.UI;
using log4net;
using Microsoft.Win32;

namespace BoxVRPlaylistManagerNETCore
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        const string DEFAULT_BOXVR_APPDATA = @"%userprofile%\AppData\LocalLow\FITXR\BOXVR";
        const string REGISTRY_UNINSTALL_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        const string REGISTRY_OCULUS_LIB_KEY = @"Software\Oculus VR, LLC\Oculus\Libraries";
        const string REGISTRY_STEAM_KEY = @"HKEY_CURRENT_USER\Software\Valve\Steam";

        public const string BoxVRExePath = "BoxVRExePath";
        public const string BoxVRAppDataPath = "BoxVRAppDataPath";

        private static ILog _log = LogManager.GetLogger(typeof(App));
        public static JsonConfiguration Configuration;

        [STAThread]
        public static void Main()
        { 
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => _log.Error(e.ExceptionObject as Exception);

            var app = new App();

            Configuration = new JsonConfiguration("appsettings.json");
            
            
            // check if BoxVRExePath is already set, if not try to locate it via registry or Steam
            if(string.IsNullOrWhiteSpace(Configuration.BoxVRExePath))
            {
                _log.Debug("Starting search for BoxVR install location");
                var location = LocateBoxVRExe();
                _log.Debug($"Automatic search for BoxVR install location complete: {location}");
                if(!string.IsNullOrWhiteSpace(location))
                {
                    _log.Debug("Saving BoxVRExePath setting");
                    Configuration.BoxVRExePath = location;
                    //BoxVRPlaylistManagerNETCore.Properties.Settings.Default.Save();
                }
            }
            _log.Debug($"BoxVRExePath setting: {Configuration.BoxVRExePath}");

            // check if BoxVRAppDataPath is already set, if not set to default
            if(string.IsNullOrWhiteSpace(Configuration.BoxVRAppDataPath))
            {
                _log.Debug("BoxVRAppDataPath not set, setting default");
                if(Directory.Exists(Environment.ExpandEnvironmentVariables(DEFAULT_BOXVR_APPDATA)))
                {
                    Configuration.BoxVRAppDataPath = DEFAULT_BOXVR_APPDATA;
                    //BoxVRPlaylistManagerNETCore.Properties.Settings.Default.Save();
                }
                else
                {
                    _log.Debug($"Directory does not exist: {DEFAULT_BOXVR_APPDATA}");
                }
            }
            _log.Debug($"BoxVRAppDataPath setting: {Configuration.BoxVRAppDataPath}");

            var mainWindow = new MainWindow();
            app.Run(mainWindow);
        }

        private static string LocateBoxVRExe()
        {
            _log.Debug("Searching registry for BoxVR install location");
            using(var key = Registry.LocalMachine.OpenSubKey(REGISTRY_UNINSTALL_KEY))
            {
                foreach(var subkey_name in key.GetSubKeyNames())
                {
                    using(var subkey = key.OpenSubKey(subkey_name))
                    {
                        _log.Debug($"Checking reg key HKLM\\{REGISTRY_UNINSTALL_KEY}\\{subkey_name}");
                        if(string.Equals(subkey.GetValue("DisplayName")?.ToString(), "BOXVR", StringComparison.OrdinalIgnoreCase))
                        {
                            var location = subkey.GetValue("InstallLocation")?.ToString();
                            if(location != null)
                            {
                                _log.Debug($"BoxVR location found in reg key HKLM\\{REGISTRY_UNINSTALL_KEY}\\{subkey_name}: {location}");
                                return location;
                            }
                        }
                    }
                }
            }

            _log.Debug("Searching registry for Oculus library location");
            using(var key = Registry.CurrentUser.OpenSubKey(REGISTRY_OCULUS_LIB_KEY))
            {
                if(key == null)
                {
                    _log.Debug($"Couldn't locate Oculus registry key: {REGISTRY_OCULUS_LIB_KEY}");
                }
                else
                {
                    var mountPoints = new List<string>();
                    foreach(var subkey_name in key.GetSubKeyNames())
                    {
                        using(var subkey = key.OpenSubKey(subkey_name))
                        {
                            _log.Debug($"Checking reg key HKCU\\{REGISTRY_OCULUS_LIB_KEY}\\{subkey_name}");
                            var libPath = subkey.GetValue("Path")?.ToString();
                            if(libPath == null)
                            {
                                _log.Debug($"No 'Path' value found, moving on");
                            }
                            else
                            {
                                _log.Debug($"Path found: {libPath}, searching for 'fitxr-boxvr'");
                                var volumeIdMatch = Regex.Match(libPath, @"(\\\\\?\\Volume{.*?}\\)(.*)");
                                if(volumeIdMatch.Success)
                                {
                                    _log.Debug($"Getting mount points for volume {volumeIdMatch.Groups[1].Value}");
                                    try
                                    {
                                        var _mountPoints = SafeNativeMethods.GetMountPointsForVolume(volumeIdMatch.Groups[1].Value);
                                        _log.Debug($"Found the following mountpoints for {volumeIdMatch.Groups[1].Value}:\r\n{string.Join("\r\n", _mountPoints)}");
                                        mountPoints.AddRange(_mountPoints.Select(m => Path.Combine(m, volumeIdMatch.Groups[2].Value)));
                                    }
                                    catch(Win32Exception ex)
                                    {
                                        _log.Error(ex);
                                    }
                                }
                                else
                                {
                                    _log.Debug("Failed to find volume GUID path");
                                }
                            }
                        }
                    }

                    foreach(var mountPoint in mountPoints)
                    {
                        var location = Path.Combine(mountPoint, "Software", "fitxr-boxvr", "BoxVR_Oculus");
                        if(File.Exists(Path.Combine(location, "BoxVR.exe")))
                        {
                            _log.Debug($"BoxVR located in Oculus library: {location}");
                            return location;
                        }
                    }
                }
            }

            _log.Debug("Searching registry for Steam install location");
            var steamPath = Registry.GetValue(REGISTRY_STEAM_KEY, "SteamPath", null)?.ToString();
            if(steamPath != null)
            {
                _log.Debug($"{REGISTRY_STEAM_KEY}/SteamPath: {steamPath}");
                var steamConfigPath = Path.Combine(steamPath, "config/config.vdf");
                if(File.Exists(steamConfigPath))
                {
                    var steamConfig = File.ReadAllText(steamConfigPath);
                    var matches = Regex.Matches(steamConfig, @"""BaseInstallFolder_\d\""\s+""(.*?)""");
                    if(matches.Count > 0)
                    {
                        foreach(Match match in matches)
                        {
                            var libraryPath = match.Groups[1].Value;
                            if(Directory.Exists(libraryPath))
                            {
                                _log.Debug($"Searching Steam library for BoxVR: {libraryPath}");
                                var BoxVRExePath = Path.Combine(libraryPath, "steamapps", "common", "BoxVR");
                                if(File.Exists(Path.Combine(BoxVRExePath, "BoxVR.exe")))
                                {
                                    _log.Debug($"BoxVR.exe located at {BoxVRExePath}");
                                    return BoxVRExePath;
                                }
                                else
                                {
                                    _log.Debug($"Could not find BoxVR.exe in library: {libraryPath}");
                                }
                            }
                            else
                            {
                                _log.Debug($"Steam library does not exist: {libraryPath}");
                            }
                        }
                    }
                    else
                    {
                        _log.Debug($"Failed to find Steam library locations in {steamConfigPath}");
                        _log.Debug(steamConfig);
                    }
                }
                else
                {
                    _log.Debug($"No Steam config found at {steamConfigPath}");
                }
            }
            else
            {
                _log.Debug("Failed to load HKCU/Software/Valve/Steam/SteamPath");
            }

            _log.Debug("Could not automatically find BoxVR install location");
            return string.Empty;
        }
    }
}
