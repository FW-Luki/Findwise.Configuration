using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Findwise.Connector.ConfigEditor
{
    public static class Helpers
    {
        public static bool IsOnScreen(this Form form)
        {
            var screens = Screen.AllScreens;
            foreach (Screen screen in screens)
            {
                var formTopLeft = new Point(form.Left, form.Top);
                if (screen.WorkingArea.Contains(formTopLeft))
                {
                    return true;
                }
            }
            return false;
        }


        public class IconExtractor
        {
            [DllImport("Shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
            private static extern int ExtractIconEx(string sFile, int iIndex, out IntPtr piLargeVersion, out IntPtr piSmallVersion, int amountIcons);

            public static Icon GetIcon(string file, int number, bool largeIcon)
            {
                IntPtr large;
                IntPtr small;
                ExtractIconEx(file, number, out large, out small, 1);
                try
                {
                    return Icon.FromHandle(largeIcon ? large : small);
                }
                catch
                {
                    return null;
                }
            }

            public static Bitmap GetBitmap(string file, int number, bool largeIcon)
            {
                IntPtr large;
                IntPtr small;
                ExtractIconEx(file, number, out large, out small, 1);
                try
                {
                    return Bitmap.FromHicon(largeIcon ? large : small);
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
