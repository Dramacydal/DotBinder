using System;

namespace DotBinder
{
    [Flags]
    public enum KeyModifiers
    {
        None = 0x0,
        Control = 0x1,
        Shift = 0x2,
        Alt = 0x4,
        All = Control | Shift | Alt,
    }
}
