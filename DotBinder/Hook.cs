using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DotBinder
{
    public static class Hook
    {
        private static HookContainer hook = new HookContainer();

        public static KeyModifiers ActiveModifiers = KeyModifiers.None;
        private static HookProc hookProc = new HookProc(KeyboardHookCallback);

        private static int KeyboardHookCallback(int code, IntPtr wParam, IntPtr lParam)
        {
            var ret = false;

            if (code >= 0)
            {
                var key = (Keys)Marshal.ReadInt32(lParam);
                var mEvent = (MessageEvent)wParam.ToInt32();
                switch (key)
                {
                    case Keys.LControlKey:
                    case Keys.RControlKey:
                        if (mEvent == MessageEvent.WM_KEYDOWN)
                            ActiveModifiers |= KeyModifiers.Control;
                        else
                            ActiveModifiers &= ~KeyModifiers.Control;
                        break;
                    case Keys.LShiftKey:
                    case Keys.RShiftKey:
                        if (mEvent == MessageEvent.WM_KEYDOWN)
                            ActiveModifiers |= KeyModifiers.Shift;
                        else
                            ActiveModifiers &= ~KeyModifiers.Shift;
                        break;
                    case Keys.LMenu:
                    case Keys.RMenu:
                        if (mEvent == MessageEvent.WM_KEYDOWN)
                            ActiveModifiers |= KeyModifiers.Alt;
                        else
                            ActiveModifiers &= ~KeyModifiers.Alt;
                        break;
                }

                foreach (var keyHook in hook.KeyHooks)
                {
                    if (keyHook == null)
                        continue;

                    if (keyHook.Condition.Meets(key, mEvent, ActiveModifiers))
                    {
                        ret |= keyHook.Blocker;
                        keyHook.Action.Do();
                    }
                }
            }

            return ret ? 1 : HookAPI.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }

        private static IntPtr InstallHook(KeyHook keyHook)
        {
            using (var curProc = Process.GetCurrentProcess())
            using (var curModule = curProc.MainModule)
            {
                var handle = HookAPI.SetWindowsHookEx(HookType.WH_KEYBOARD_LL, hookProc, HookAPI.GetModuleHandle(curModule.ModuleName), 0);
                if (handle == IntPtr.Zero)
                    throw new HookException("Failed to install hook");

                hook = new HookContainer
                    {
                        Handle = handle,
                        KeyHooks = new List<KeyHook> { keyHook }
                    };
                return handle;
            }
        }

        public static void UnistallHooks()
        {
            HookAPI.UnhookWindowsHookEx(hook.Handle);
        }

        public static void RegisterHook(KeyHook keyHook)
        {
            if (hook == null || hook.Handle == IntPtr.Zero)
            {
                InstallHook(keyHook);
                return;
            }

            hook.KeyHooks.Add(keyHook);
        }

        public static void UnregisterHook(KeyHook keyHook)
        {
            if (hook == null)
                return;

            hook.KeyHooks.Remove(keyHook);
        }
    }
}
