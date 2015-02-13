using DotBinder.Actions.ExtraConditions;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DotBinder.Actions
{
    public class HookCondition
    {
        public KeyModifiers ModifierMask = KeyModifiers.None;
        public Keys Key = Keys.None;
        public MessageEvent Event = MessageEvent.WM_NONE;
        public List<ExtraCondition> ExtraConditions = new List<ExtraCondition>();

        public bool Meets(Keys key, MessageEvent ev, KeyModifiers modifies)
        {
            if (modifies != ModifierMask)
                return false;
            if (key != Key)
                return false;
            if (ev != Event)
                return false;

            foreach (var extra in ExtraConditions)
                if (!extra.Meets())
                    return false;

            return true;
        }
    }
}
