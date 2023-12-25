using System.Windows.Forms;

namespace FovCalibrationTool.Keyboard.HotKeys
{
    public record HotKey(Keys Keys, Keys KeysModifier = default);
}
