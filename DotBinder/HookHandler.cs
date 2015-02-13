using System;
using System.Collections.Generic;

namespace DotBinder
{
    class HookContainer
    {
        public IntPtr Handle = IntPtr.Zero;
        public List<KeyHook> KeyHooks = new List<KeyHook>();
    }
}
