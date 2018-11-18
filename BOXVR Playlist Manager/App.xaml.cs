using Microsoft.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace BoxVR_Playlist_Manager
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


        public static Logger logger = LogManager.GetLogger("log");

        [STAThread]
        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => logger.Error(e.ExceptionObject as Exception);

            var app = new App();

            // check if BoxVRExePath is already set, if not try to locate it via registry or Steam
            if (string.IsNullOrWhiteSpace(BoxVR_Playlist_Manager.Properties.Settings.Default.BoxVRExePath))
            {
                logger.Trace("Starting search for BoxVR install location");
                var location = LocateBoxVRExe();
                logger.Trace($"Automatic search for BoxVR install location complete: {location}");
                if (!string.IsNullOrWhiteSpace(location))
                {
                    logger.Trace("Saving BoxVRExePath setting");
                    BoxVR_Playlist_Manager.Properties.Settings.Default.BoxVRExePath = location;
                    BoxVR_Playlist_Manager.Properties.Settings.Default.Save();
                }
            }
            logger.Trace($"BoxVRExePath setting: {BoxVR_Playlist_Manager.Properties.Settings.Default.BoxVRExePath}");

            // check if BoxVRAppDataPath is already set, if not set to default
            if (string.IsNullOrWhiteSpace(BoxVR_Playlist_Manager.Properties.Settings.Default.BoxVRAppDataPath))
            {
                logger.Debug("BoxVRAppDataPath not set, setting default");
                if (Directory.Exists(Environment.ExpandEnvironmentVariables(DEFAULT_BOXVR_APPDATA)))
                {
                    BoxVR_Playlist_Manager.Properties.Settings.Default.BoxVRAppDataPath = DEFAULT_BOXVR_APPDATA;
                    BoxVR_Playlist_Manager.Properties.Settings.Default.Save();
                }
                else
                {
                    logger.Debug($"Directory does not exist: {DEFAULT_BOXVR_APPDATA}");
                }
            }
            logger.Trace($"BoxVRAppDataPath setting: {BoxVR_Playlist_Manager.Properties.Settings.Default.BoxVRAppDataPath}");

            var mainWindow = new MainWindow();
            app.Run(mainWindow);
        }

        private static string LocateBoxVRExe()
        {
            logger.Debug("Searching registry for BoxVR install location");
            using (var key = Registry.LocalMachine.OpenSubKey(REGISTRY_UNINSTALL_KEY))
            {
                foreach (var subkey_name in key.GetSubKeyNames())
                {
                    using (var subkey = key.OpenSubKey(subkey_name))
                    {
                        logger.Trace($"Checking reg key HKLM\\{REGISTRY_UNINSTALL_KEY}\\{subkey_name}");
                        if (string.Equals(subkey.GetValue("DisplayName")?.ToString(), "BOXVR", StringComparison.OrdinalIgnoreCase))
                        {
                            var location = subkey.GetValue("InstallLocation")?.ToString();
                            if(location != null)
                            {
                                logger.Debug($"BoxVR location found in reg key HKLM\\{REGISTRY_UNINSTALL_KEY}\\{subkey_name}: {location}");
                                return location;
                            }
                        }
                    }
                }
            }

            logger.Debug("Searching registry for Oculus library location");
            using (var key = Registry.CurrentUser.OpenSubKey(REGISTRY_OCULUS_LIB_KEY))
            {
                if (key == null)
                {
                    logger.Trace($"Couldn't locate Oculus registry key: {REGISTRY_OCULUS_LIB_KEY}");
                }
                else
                {
                    var mountPoints = new List<string>();
                    foreach(var subkey_name in key.GetSubKeyNames())
                    {
                        using(var subkey = key.OpenSubKey(subkey_name))
                        {
                            logger.Trace($"Checking reg key HKCU\\{REGISTRY_OCULUS_LIB_KEY}\\{subkey_name}");
                            var libPath = subkey.GetValue("Path")?.ToString();
                            if(libPath == null)
                            {
                                logger.Trace($"No 'Path' value found, moving on");
                            }
                            else
                            {
                                logger.Trace($"Path found: {libPath}, searching for 'fitxr-boxvr'");
                                var volumeIdMatch = Regex.Match(libPath, @"(\\\\\?\\Volume{.*?}\\)(.*)");
                                if (volumeIdMatch.Success)
                                {
                                    logger.Trace($"Getting mount points for volume {volumeIdMatch.Groups[1].Value}");
                                    try
                                    {
                                        var _mountPoints = SafeNativeMethods.GetMountPointsForVolume(volumeIdMatch.Groups[1].Value);
                                        logger.Trace($"Found the following mountpoints for {volumeIdMatch.Groups[1].Value}:\r\n{string.Join("\r\n", _mountPoints)}");
                                        mountPoints.AddRange(_mountPoints.Select(m => Path.Combine(m, volumeIdMatch.Groups[2].Value)));
                                    }
                                    catch (Win32Exception ex)
                                    {
                                        logger.Error(ex);
                                    }
                                }
                                else
                                {
                                    logger.Trace("Failed to find volume GUID path");
                                }
                            }
                        }
                    }

                    foreach(var mountPoint in mountPoints)
                    {
                        var location = Path.Combine(mountPoint, "Software", "fitxr-boxvr", "BoxVR_Oculus");
                        if (File.Exists(Path.Combine(location, "BoxVR.exe")))
                        {
                            logger.Debug($"BoxVR located in Oculus library: {location}");
                            return location;
                        }
                    }
                }
            }

            logger.Debug("Searching registry for Steam install location");
            var steamPath = Registry.GetValue(REGISTRY_STEAM_KEY, "SteamPath", null)?.ToString();
            if (steamPath != null)
            {
                logger.Trace($"{REGISTRY_STEAM_KEY}/SteamPath: {steamPath}");
                var steamConfigPath = Path.Combine(steamPath, "config/config.vdf");
                if (File.Exists(steamConfigPath))
                {
                    var steamConfig = File.ReadAllText(steamConfigPath);
                    var matches = Regex.Matches(steamConfig, @"""BaseInstallFolder_\d\""\s+""(.*?)""");
                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            var libraryPath = match.Groups[1].Value;
                            if (Directory.Exists(libraryPath))
                            {
                                logger.Trace($"Searching Steam library for BoxVR: {libraryPath}");
                                var BoxVRExePath = Path.Combine(libraryPath, "steamapps", "common", "BoxVR");
                                if (File.Exists(Path.Combine(BoxVRExePath, "BoxVR.exe")))
                                {
                                    logger.Debug($"BoxVR.exe located at {BoxVRExePath}");
                                    return BoxVRExePath;
                                }
                                else
                                {
                                    logger.Trace($"Could not find BoxVR.exe in library: {libraryPath}");
                                }
                            }
                            else
                            {
                                logger.Trace($"Steam library does not exist: {libraryPath}");
                            }
                        }
                    }
                    else
                    {
                        logger.Debug($"Failed to find Steam library locations in {steamConfigPath}");
                        logger.Trace(steamConfig);
                    }
                }
                else
                {
                    logger.Debug($"No Steam config found at {steamConfigPath}");
                }
            }
            else
            {
                logger.Debug("Failed to load HKCU/Software/Valve/Steam/SteamPath");
            }

            logger.Debug("Could not automatically find BoxVR install location");
            return string.Empty;
        }
    }
}
