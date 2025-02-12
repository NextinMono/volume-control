using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VolumeControls
{
    public class VolumeControlTrayApp : ApplicationContext
    {
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);


        private const int TAP_THRESHOLD_MS = 200; // Max time for a tap

        // Keyboard Hook
        private static IntPtr _hookID = IntPtr.Zero;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static Stopwatch keyTimer = new Stopwatch();

        private static bool isRightCtrlPressed = false; // Track Right Ctrl state
        private static NotifyIcon trayIcon;


        public VolumeControlTrayApp()
        {
            trayIcon = new NotifyIcon()
            {
                Icon = Utils.DefaultFormIcon,
                ContextMenuStrip = new ContextMenuStrip()
                {
                    Items = { new ToolStripMenuItem("Made by HKRY", null),new ToolStripMenuItem("Stop", null, Exit) }
                },
                Visible = true,
                Text = "Artificial Volume Keys"
            };


            // Set up global keyboard hook
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                _hookID = SetWindowsHookEx(Hooks.WH_KEYBOARD_LL, _proc,
                    Hooks.GetModuleHandle(curModule.ModuleName), 0);
            }
            Application.ApplicationExit += (sender, e) => Hooks.UnhookWindowsHookEx(_hookID);
        }
        /// <summary>
        /// When the key is only tapped with Right Ctrl held down, this will fire an action
        /// </summary>
        /// <param name="pressedKey"></param>
        /// <param name="required"></param>
        /// <param name="wParam"></param>
        /// <param name="action"></param>
        private static bool OnKeyComboPressed(Keys pressedKey, Keys required, IntPtr wParam, float tapThreshold, Action action)
        {
            bool executed = false;
            if (pressedKey == required)
            {
                if (wParam == (IntPtr)Hooks.WM_KEYDOWN)
                {
                    keyTimer.Restart();
                }
                else if (wParam == (IntPtr)Hooks.WM_KEYUP)
                {
                    keyTimer.Stop();
                    if (isRightCtrlPressed)
                    {
                        if (keyTimer.ElapsedMilliseconds <= tapThreshold || keyTimer.ElapsedMilliseconds >= tapThreshold * 2)
                        {
                            if (isRightCtrlPressed)
                            {
                                action.Invoke();
                                executed = true;
                            }
                        }
                    }
                }
            }
            return executed;
        }
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                Keys currentKey = (Keys)Marshal.ReadInt32(lParam);
                if (currentKey == Keys.RControlKey)
                    isRightCtrlPressed = wParam == (IntPtr)Hooks.WM_KEYDOWN && wParam != (IntPtr)Hooks.WM_KEYUP;

                bool executed = 
                    OnKeyComboPressed(currentKey, Keys.F10, wParam, TAP_THRESHOLD_MS, delegate { Hooks.keybd_event((byte)Keys.VolumeDown, 0, 0, 0); }) ||
                    OnKeyComboPressed(currentKey, Keys.F11, wParam, TAP_THRESHOLD_MS, delegate { Hooks.keybd_event((byte)Keys.VolumeUp, 0, 0, 0); }) ||
                    OnKeyComboPressed(currentKey, Keys.F9, wParam, TAP_THRESHOLD_MS, delegate { Hooks.keybd_event((byte)Keys.VolumeMute, 0, 0, 0); })|| 
                    OnKeyComboPressed(currentKey, Keys.F6, wParam, TAP_THRESHOLD_MS, delegate { Hooks.keybd_event((byte)Keys.MediaPlayPause, 0, 0, 0); });
                
                if (executed)
                    return (IntPtr)1;
            }

            return Hooks.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
        private void Exit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.Exit();
        }
    }
}
