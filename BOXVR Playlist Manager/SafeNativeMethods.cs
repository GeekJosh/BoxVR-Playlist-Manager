using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BoxVR_Playlist_Manager
{
    internal static class SafeNativeMethods
    {
        #region GetVolumePathNamesForVolumeNameW Win32 API sample from https://www.pinvoke.net/default.aspx/kernel32.getvolumepathnamesforvolumename
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetVolumePathNamesForVolumeNameW (
            [MarshalAs(UnmanagedType.LPWStr)]
            string lpszVolumeName,
            [MarshalAs(UnmanagedType.LPWStr)]
            string lpszVolumePathNames,
            uint cchBuferLength,
            ref uint lpcchReturnLength
        );

        public static List<string> GetMountPointsForVolume(string volumeDeviceName)
        {
            List<string> result = new List<string>();

            // GetVolumePathNamesForVolumeName is only available on Windows XP/2003 and above
            int osVersionMajor = Environment.OSVersion.Version.Major;
            int osVersionMinor = Environment.OSVersion.Version.Minor;
            if (osVersionMajor < 5 || (osVersionMajor == 5 && osVersionMinor < 1))
            {
                return result;
            }

            uint lpcchReturnLength = 0;
            string buffer = "";

            GetVolumePathNamesForVolumeNameW(volumeDeviceName, buffer, (uint)buffer.Length, ref lpcchReturnLength);
            if (lpcchReturnLength == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            buffer = new string(new char[lpcchReturnLength]);

            if (!GetVolumePathNamesForVolumeNameW(volumeDeviceName, buffer, lpcchReturnLength, ref lpcchReturnLength))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            result.AddRange(buffer.Split('\0').Where(m => m.Length > 0));

            return result;
        }
        #endregion
    }
}
