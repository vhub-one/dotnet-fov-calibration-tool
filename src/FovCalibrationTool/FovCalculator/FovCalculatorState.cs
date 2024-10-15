
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
    );
}
