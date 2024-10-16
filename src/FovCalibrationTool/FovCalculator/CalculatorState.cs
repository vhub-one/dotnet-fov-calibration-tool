
namespace FovCalibrationTool.FovCalculator
{
    public record CalculatorState(
        EnvironmentOptions Environment,
        UserOptions User,
        TrackingState TrackingState,
        PresetState PresetState
    );
}
