
namespace FovCalibrationTool.FovCalculator
{
    public static class FovCalculatorUtils
    {
        public static FovStatistics CalculateStatistics(FovCalculatorState state)
        {
            var pointsPer360Deg = Math.Abs(state.PointsPer360Deg);
            var pointsPer1Deg = pointsPer360Deg / 360d;
            var pointsPerCustomDeg = Math.Abs(state.PointsPerCustomDeg);

            var customDeg = default(double);

            if (pointsPer1Deg > 0)
            {
                customDeg = pointsPerCustomDeg / pointsPer1Deg;
            }

            return new FovStatistics
            {
                PointsPer360Deg  = pointsPer360Deg,
                PointsPer1Deg = pointsPer1Deg,
                PointsPerCustomDeg = pointsPerCustomDeg,

                CustomDeg = customDeg
            };
        }
    }
}
