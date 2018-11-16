using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace BoxVR_Playlist_Manager
{
    public static class Extensions
    {
        #region modal folder browser code courtesy of Craig Shearer @ stackoverflow https://stackoverflow.com/a/315436
        public static System.Windows.Forms.IWin32Window GetIWin32Window(this System.Windows.Media.Visual visual)
        {
            var source = System.Windows.PresentationSource.FromVisual(visual) as HwndSource;
            var win = new OldWindow(source.Handle);
            return win;
        }

        private class OldWindow : System.Windows.Forms.IWin32Window
        {
            private readonly IntPtr _handle;
            IntPtr System.Windows.Forms.IWin32Window.Handle => _handle;

            public OldWindow(IntPtr handle)
            {
                _handle = handle;
            }
        }
        #endregion
    }
}
