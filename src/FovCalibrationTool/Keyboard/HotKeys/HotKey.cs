using System.Windows.Forms;

namespace FovCalibrationTool.Keyboard.HotKeys
{
    public record HotKey(Keys Keys, Keys KeysModifier = default)
    {
        public bool HasHotKey(HotKey target)
        {
            if (Keys == target.Keys)
            {
                return KeysModifier.HasFlag(target.KeysModifier);
            }

            return false;
        }
    }
}
