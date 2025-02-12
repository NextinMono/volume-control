using System.Reflection;
using System.Runtime.InteropServices;

namespace VolumeControls
{
    public static class Utils
    {
        private static Icon _defaultFormIcon;

        public static Icon DefaultFormIcon
        {
            get
            {
                if (_defaultFormIcon == null)
                    _defaultFormIcon = (Icon)typeof(Form)
                        .GetProperty("DefaultIcon",
                                     BindingFlags.NonPublic | BindingFlags.Static)
                        .GetValue(null, null);

                return _defaultFormIcon;
            }
        }
    }
    public static class Hooks
    {
        public const int WH_KEYBOARD_LL = 13;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;

        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
    }
}
