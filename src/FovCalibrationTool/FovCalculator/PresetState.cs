
namespace FovCalibrationTool.FovCalculator
{
    public record PresetState(double MoveDistancePer360Deg, double MoveDistancePerFovDeg)
    {
        public static readonly PresetState Default = new(double.NaN, double.NaN);
    }
}