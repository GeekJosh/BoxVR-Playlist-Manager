using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoxVR_Playlist_Manager.FitXr.Enums
{
    [Flags]
    public enum TrackGenre
    {
        None = 0,
        Pop = 1,
        Rock = 2,
        Electronic = 4,
        HipHop = 8,
    }
}
