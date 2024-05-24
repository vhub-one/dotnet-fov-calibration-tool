
namespace FovCalibrationTool.FovCalculator
{
    public static class FovCalculatorUtils
    {
        public static int GetPoints(FovCalculatorState state)
        {
            if (state.Mode == FovCalculatorMode.Capture360)
            {
                return CalculateAbsolutePoints(state.PointsPer360Deg);
            }
            if (state.Mode == FovCalculatorMode.CaptureFov)
            {
                return CalculateAbsolutePoints(state.PointsPerFovDeg);
            }

            return 0;
        }

        public static FovStatistics CalculateStatistics(FovCalculatorState state)
        {
            var pointsPer360Deg = Math.Abs(state.PointsPer360Deg);
            var pointsPerFovDeg = Math.Abs(state.PointsPerFovDeg);

            var fovDeg = double.NaN;

            var pointsPer1Deg = pointsPer360Deg / 360d;

            if (pointsPer1Deg > 0)
            {
                fovDeg = pointsPerFovDeg / pointsPer1Deg;
            }

            var viewPortWidth = CalculateViewPortWidth(state);
            var viewPortDeg = CalculateTargetAngle(fovDeg, state.FovWidth, viewPortWidth);

            var fovDegAngleBased = CalculateTargetAngle(state.ViewPortDeg, viewPortWidth, state.FovWidth);

            var pointsPer1DegSensBased = double.NaN;

            if (viewPortDeg > 0)
            {
                pointsPer1DegSensBased = state.PointsPerViewPortDeg / viewPortDeg;
            }

            var pointsPerViewPortDeg = pointsPer1Deg * viewPortDeg;
            var pointsPerFovDegAngleBased = pointsPer1Deg * fovDegAngleBased;

            var pointsPer360DegSensBased = pointsPer1DegSensBased * 360;
            var pointsPerFovDegSensBased = pointsPer1DegSensBased * fovDeg;

            return new FovStatistics
            {
                FovDeg = fovDeg,
                FovDegAngleBased = fovDegAngleBased,

                PointsPer360Deg = pointsPer360Deg,
                PointsPer1Deg = pointsPer1Deg,
                PointsPerFovDeg = pointsPerFovDeg,
                PointsPerViewPortDeg = pointsPerViewPortDeg,

                ViewPortWidth = viewPortWidth,
                ViewPortDeg = viewPortDeg,

                PointsPer360DegSensBased = pointsPer360DegSensBased,
                PointsPer1DegSensBased = pointsPer1DegSensBased,
                PointsPerFovDegSensBased = pointsPerFovDegSensBased,
                PointsPerFovDegAngleBased = pointsPerFovDegAngleBased,
            };
        }

        private static int CalculateAbsolutePoints(double points)
        {
            if (double.IsFinite(points))
            {
                return (int)Math.Round(Math.Abs(points));
            }

            return 0;
        }

        private static double CalculateViewPortWidth(FovCalculatorState state)
        {
            var viewPortRad = state.ViewPortObserveDeg * Math.PI / 180;

            if (state.DisplayType == DisplayType.Flat)
            {
                return Math.Tan(viewPortRad / 2) * 2 * state.DisplayDistance;
            }

            if (state.DisplayType == DisplayType.Curved)
            {
                return viewPortRad * state.DisplayDistance;
            }

            throw new InvalidOperationException();
        }

        private static double CalculateTargetAngle(double sourceAngleDeg, double sourceWidth, double targetWidth)
        {
            if (sourceAngleDeg <= 0 || sourceAngleDeg >= 180)
            {
                return double.NaN;
            }

            if (sourceWidth <= 0)
            {
                return double.NaN;
            }

            if (targetWidth <= 0)
            {
                return double.NaN;
            }

            var sourceAngleRad = sourceAngleDeg * Math.PI / 180;

            var viewPortFraction = targetWidth / sourceWidth;
            var viewPortRad = Math.Atan(Math.Tan(sourceAngleRad / 2) * viewPortFraction) * 2;

            return viewPortRad * 180 / Math.PI;
        }
    }
}