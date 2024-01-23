
namespace FovCalibrationTool.FovCalculator
{
    public record FovCalculatorState(
        bool Tracking,
        FovCalculatorMode Mode,
        int PointsPer360Deg,
        int PointsPerCustomDeg
    );
}
