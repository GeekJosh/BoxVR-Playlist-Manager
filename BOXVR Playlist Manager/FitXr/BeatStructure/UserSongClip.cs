using System;
using System.IO;
using BoxVR_Playlist_Manager.FitXr.Models;

namespace BoxVR_Playlist_Manager.FitXr.BeatStructure
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

        public UserSongClip(ATL.Track track)
        {
            trackId = new TrackId() { trackId = Path.GetFileNameWithoutExtension(track.Path) };
            wavPath = track.Path;
            originalFilePath = track.Path;
            originalFileName = Path.GetFileNameWithoutExtension(track.Path);
            trackData = new TrackData()
            {
                originalArtist = track.Artist,
                duration = (float)track.Duration,
                originalTrackName = track.Title,
                trackId = trackId,
                originalFilePath = track.Path
            };
        }
    }
}
