
namespace FovCalibrationTool.FovCalculator
{
    public record CalculatorState(
        TrackingState Tracking,
        EnvironmentOptions Environment,
        UserOptions User,
        GameOptions Game
    );
}
