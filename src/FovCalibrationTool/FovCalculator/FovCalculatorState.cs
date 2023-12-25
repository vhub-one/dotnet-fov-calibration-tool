namespace FovCalibrationTool.FovCalculator
{
    public record FovCalculatorState(
        bool Enabled,
        FovCalculatorMode Mode,
        int PointsPer360Deg,
        int PointsPerCustomDeg
    );
}
