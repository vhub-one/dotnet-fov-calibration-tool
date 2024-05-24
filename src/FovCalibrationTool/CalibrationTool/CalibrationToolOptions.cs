using FovCalibrationTool.FovCalculator;

namespace FovCalibrationTool.CalibrationTool
{
    public class CalibrationToolOptions
    {
        public DisplayType DisplayType { get; set; }
        public double DisplayDistance { get; set; }
        public double FovWidth { get; set; }
        public double ViewPortObserveDeg { get; set; }
        public double ViewPortDeg { get; set; }
        public double ViewPortPoints { get; set; }
    }
}
