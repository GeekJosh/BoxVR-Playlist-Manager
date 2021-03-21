using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BoxVR_Playlist_Manager.FitXr.Tools
{
    public class IO
    {
        public static void WriteStringToDisk(string filename, string content)
        {
            if(File.Exists(filename))
                File.Delete(filename);
            StreamWriter text = File.CreateText(filename);
            text.Write(content);
            text.Close();
        }

        public static void DeleteFilesWithExtension(string directory, string extension)
        {
            foreach(FileInfo fileInfo in ((IEnumerable<FileInfo>)new DirectoryInfo(directory).GetFiles("*." + extension)).Where<FileInfo>((Func<FileInfo, bool>)(p => p.Extension == "." + extension)).ToArray<FileInfo>())
            {
                try
                {
                    fileInfo.Attributes = FileAttributes.Normal;
                    File.Delete(fileInfo.FullName);
                }
                catch
                {
                }
            }
        }

        public static string[] GetVisibleFiles(string path, string fileFilter)
        {
            List<string> stringList = new List<string>();
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            string str = fileFilter;
            char[] chArray = new char[1] { ';' };
            foreach(string searchPattern in str.Split(chArray))
            {
                foreach(FileInfo fileInfo in ((IEnumerable<FileInfo>)directoryInfo.GetFiles(searchPattern)).Where<FileInfo>((Func<FileInfo, bool>)(f => (f.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)))
                    stringList.Add(fileInfo.FullName);
            }
            stringList.Sort();
            return stringList.ToArray();
        }

        public static string[] GetDrives() => Directory.GetLogicalDrives();

        public static string[] GetVisibleDirectories(string path)
        {
            string[] directories = Directory.GetDirectories(path);
            List<string> stringList = new List<string>();
            foreach(string path2 in directories)
            {
                string path1 = Path.Combine(path, path2);
                if((new DirectoryInfo(path1).Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                    stringList.Add(path1);
            }
            return ((IEnumerable<string>)directories).ToArray<string>();
        }
    }
}
