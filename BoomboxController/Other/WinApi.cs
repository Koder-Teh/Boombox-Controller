using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BoomboxController
{
    public class WinApi
    {
        public enum MessageBoxResult // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-messagebox#return-value
        {
            Error = 0,
            Abort = 3,     // The Abort button was selected.
            Cancel = 2,    // The Cancel button was selected.
            Continue = 11, // The Continue button was selected.
            Ignore = 5,    // The Ignore button was selected.
            No = 7,        // The No button was selected.
            OK = 1,        // The OK button was selected.
            Retry = 4,     // The Retry button was selected.
            TryAgain = 10, // The Try Again button was selected.
            Yes = 6        // The Yes button was selected. 
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern MessageBoxResult MessageBox(IntPtr hwnd, string text, string caption, uint type);

        public void SizeConsole(int width, int height)
        {
            MoveWindow(GetConsoleWindow(), 200, 200, width, height, true);
        }

        public static MessageBoxResult SendMessageBox(string text, string caption, uint type)
        {
            MessageBoxResult result = MessageBox(GetConsoleWindow(), text, caption, type);
            return result;
        }
    }
}
