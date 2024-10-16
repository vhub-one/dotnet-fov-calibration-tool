using FovCalibrationTool.FovCalculator;

namespace FovCalibrationTool.CalibrationTool
{
    public class CalibrationToolOptions
    {
        public EnvironmentOptions Environment { get; set; }
        public UserOptions User { get; set; }
        public PresetOptions Preset { get; set; }
    }
}
