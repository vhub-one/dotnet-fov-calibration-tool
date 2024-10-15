
namespace FovCalibrationTool.FovCalculator
{
    public record GameOptions(double PointsPer360Deg, double PointsPerFovDeg)
    {
        public static readonly GameOptions Default = new(double.NaN, double.NaN);
    }
}