using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace DotBinder.Actions.ActionTypes
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ParentProcessUtilities
    {
        // These members must match PROCESS_BASIC_INFORMATION
        internal IntPtr Reserved1;
        internal IntPtr PebBaseAddress;
        internal IntPtr Reserved2_0;
        internal IntPtr Reserved2_1;
        internal IntPtr UniqueProcessId;
        internal IntPtr InheritedFromUniqueProcessId;

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref ParentProcessUtilities processInformation, int processInformationLength, out int returnLength);

        /// <summary>
        /// Gets the parent process of the current process.
        /// </summary>
        /// <returns>An instance of the Process class.</returns>
        public static int GetParentProcess()
        {
            return GetParentProcessId(Process.GetCurrentProcess().Handle);
        }

        /// <summary>
        /// Gets the parent process of specified process.
        /// </summary>
        /// <param name="id">The process id.</param>
        /// <returns>An instance of the Process class.</returns>
        public static int GetParentProcess(int id)
        {
            Process process = Process.GetProcessById(id);
            return GetParentProcessId(process.Handle);
        }

        /// <summary>
        /// Gets the parent process of a specified process.
        /// </summary>
        /// <param name="handle">The process handle.</param>
        /// <returns>An instance of the Process class.</returns>
        public static int GetParentProcessId(IntPtr handle)
        {
            ParentProcessUtilities pbi = new ParentProcessUtilities();
            int returnLength;
            int status = NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out returnLength);
            if (status != 0)
                throw new Win32Exception(status);

            try
            {
                return pbi.InheritedFromUniqueProcessId.ToInt32();
            }
            catch (ArgumentException)
            {
                // not found
                return 0;
            }
        }
    }

    public class SimulateKeypress : HookAction
    {
        public Keys Key = Keys.None;
        public KeyModifiers ModifiersMask = KeyModifiers.None;

        public string FocusProcessName = string.Empty;
        public bool FirstOnly = false;

        public static void SimulateKey(Keys key, bool press = true, bool release = true)
        {
            if (press)
                HookAPI.keybd_event((byte)key, 0, (int)1, 0);
            if (release)
                HookAPI.keybd_event((byte)key, 0, (int)2, 0);
        }

        public override void Do()
        {
            var keys = new List<Keys>();
            if (ModifiersMask.HasFlag(KeyModifiers.Control) && !Hook.ActiveModifiers.HasFlag(KeyModifiers.Control))
                keys.Add(Keys.LControlKey);
            if (ModifiersMask.HasFlag(KeyModifiers.Shift) && !Hook.ActiveModifiers.HasFlag(KeyModifiers.Shift))
                keys.Add(Keys.LShiftKey);
            if (ModifiersMask.HasFlag(KeyModifiers.Alt) && !Hook.ActiveModifiers.HasFlag(KeyModifiers.Alt))
                keys.Add(Keys.LMenu);

            keys.Add(Key);

            if (string.IsNullOrEmpty(FocusProcessName))
            {
                foreach (var key in keys)
                    SimulateKey(key, true, false);
                foreach (var key in keys.Reverse<Keys>())
                    SimulateKey(key, false, true);
            }
            else
            {
                var foregroundWindow = HookAPI.GetForegroundWindow();
                var processes = Process.GetProcessesByName(FocusProcessName);
                if (processes.Length == 0)
                    return;

                var parents = processes.Select(it => ParentProcessUtilities.GetParentProcess(it.Id)).Where(it => it != 0);
                var mains = processes.Where(it => parents.Contains(it.Id)).ToList();

                foreach (var parent in mains)
                {
                    if (parent.MainWindowHandle == null)
                        continue;

                    HookAPI.SetForegroundWindow(parent.MainWindowHandle);
                    Thread.Sleep(200);

                    foreach (var key in keys)
                        SimulateKey(key, true, false);
                    foreach (var key in keys.Reverse<Keys>())
                        SimulateKey(key, false, true);

                    if (FirstOnly)
                        break;
                }
                if (foregroundWindow != IntPtr.Zero)
                    HookAPI.SetForegroundWindow(foregroundWindow);
            }
        }
    }
}
