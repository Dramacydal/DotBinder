using System;

namespace DotBinder
{
    class HookException : Exception
    {
        public HookException(string message)
            : base(message)
        { }
    }
}
