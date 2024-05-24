
namespace FovCalibrationTool.FovCalculator
{
    public record FovCalculatorState(
        bool Tracking,
        FovCalculatorMode Mode,
        DisplayType DisplayType,
        double DisplayDistance,
        double FovWidth,
        double ViewPortDeg,
        double ViewPortObserveDeg,
        double PointsPer360Deg,
        double PointsPerFovDeg,
        double PointsPerViewPortDeg
    )
    {
        public static FovCalculatorState CreateDefault(DisplayType displayType, double fovWidth, double displayDistance, double viewPortObserveDeg, double viewPortDeg, double viewPortPoints)
        {
            return new FovCalculatorState(
                Tracking: false,
                Mode: FovCalculatorMode.Disabled,
                DisplayType: displayType,
                DisplayDistance: displayDistance,
                FovWidth: fovWidth,
                ViewPortDeg: viewPortDeg,
                ViewPortObserveDeg: viewPortObserveDeg,
                PointsPer360Deg: double.NaN,
                PointsPerFovDeg: double.NaN,
                PointsPerViewPortDeg: viewPortPoints
            );
        }
    }
}
