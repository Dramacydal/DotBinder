using DotBinder.Actions;

namespace DotBinder
{
    public class KeyHook
    {
        public HookCondition Condition { get; set; }
        public HookAction Action { get; set; }
        public bool Blocker { get; set; }
    }
}
