using System;
using System.IO;
using BoxVRPlaylistManagerNETCore.FitXr.Models;

namespace BoxVRPlaylistManagerNETCore.FitXr.BeatStructure
{
    public class UserSongClip
    {
        public Action<UserSongClip> onClipCreated;
        public string wavPath;
        public TrackId trackId;
        public string originalFileName;
        public string originalFilePath;
        public TrackData trackData;

        public UserSongClip()
        {
        }

        public UserSongClip(TagLib.File track)
        {
            trackId = new TrackId() { trackId = Path.GetFileNameWithoutExtension(track.Name) };
            wavPath = track.Name;
            originalFilePath = track.Name;
            originalFileName = Path.GetFileNameWithoutExtension(track.Name);
            trackData = new TrackData()
            {
                originalArtist = track.Tag.JoinedPerformers,
                duration = (float)track.Properties.Duration.TotalSeconds,
                originalTrackName = track.Tag.Title,
                trackId = trackId,
                originalFilePath = track.Name
            };
        }
    }
}
