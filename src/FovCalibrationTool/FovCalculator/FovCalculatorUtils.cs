
namespace FovCalibrationTool.FovCalculator
{
    public static class FovCalculatorUtils
    {
        public static int GetPoints(FovCalculatorState state)
        {
            if (state.Mode == FovCalculatorMode.Capture360)
            {
                return (int)Math.Abs(state.PointsPer360Deg);
            }
            if (state.Mode == FovCalculatorMode.CaptureFov)
            {
                return (int)Math.Abs(state.PointsPerFovDeg);
            }
            if (state.Mode == FovCalculatorMode.CaptureViewPort)
            {
                return (int)Math.Abs(state.PointsPerViewPortDeg);
            }

            return 0;
        }

        public static FovStatistics CalculateStatistics(FovCalculatorState state)
        {
            var pointsPer360Deg = Math.Abs(state.PointsPer360Deg);
            var pointsPerFovDeg = Math.Abs(state.PointsPerFovDeg);
            var pointsPerViewPortDeg = Math.Abs(state.PointsPerViewPortDeg);

            var fovDeg = double.NaN;

            var pointsPer1Deg = pointsPer360Deg / 360d;

            if (pointsPer1Deg > 0)
            {
                fovDeg = pointsPerFovDeg / pointsPer1Deg;
            }

            var viewPortDegEstimate = double.NaN;

            if (fovDeg < 180)
            {
                var viewPortFraction = state.ViewPortDistance / state.FovDistance;

                if (viewPortFraction < 1)
                {
                    var fovRad = fovDeg * Math.PI / 180;
                    var viewPortRad = Math.Atan(Math.Tan(fovRad / 2) * viewPortFraction) * 2;

                    viewPortDegEstimate = viewPortRad * 180 / Math.PI;
                }
            }

            var pointsPer1DegEstimate = double.NaN;

            if (viewPortDegEstimate > 0)
            {
                pointsPer1DegEstimate = pointsPerViewPortDeg / viewPortDegEstimate;
            }

            var pointsPerViewPortDegEstimate = viewPortDegEstimate * pointsPer1Deg;
            var pointsPer360DegEstimate = pointsPer1DegEstimate * 360;
            var pointsPerFovDegEstimate = pointsPer1DegEstimate * fovDeg;

            return new FovStatistics
            {
                FovDeg = fovDeg,
                ViewPortDeg = viewPortDegEstimate,
                PointsPerViewPortDeg = pointsPerViewPortDeg,
                PointsPer360Deg = pointsPer360Deg,
                PointsPerFovDeg = pointsPerFovDeg,
                PointsPerViewPortDegEstimate = pointsPerViewPortDegEstimate,
                PointsPer360DegEstimate = pointsPer360DegEstimate,
                PointsPerFovDegEstimate = pointsPerFovDegEstimate
            };
        }
    }
}