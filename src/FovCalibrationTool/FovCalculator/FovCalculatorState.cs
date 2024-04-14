
namespace FovCalibrationTool.FovCalculator
{
    public record FovCalculatorState(
        bool Tracking,
        FovCalculatorMode Mode,
        double FovDistance,
        double ViewPortDistance,
        double PointsPer360Deg,
        double PointsPerFovDeg,
        double PointsPerViewPortDeg
    )
    {
        public static FovCalculatorState CreateDefault(double fovDistance, double viewPortDistance, double viewPortPoints)
        {
            return new FovCalculatorState(
                Tracking: false,
                Mode: FovCalculatorMode.Disabled,
                FovDistance: fovDistance,
                ViewPortDistance: viewPortDistance,
                PointsPer360Deg: double.NaN,
                PointsPerFovDeg: double.NaN,
                PointsPerViewPortDeg: viewPortPoints
            );
        }
    }
}
