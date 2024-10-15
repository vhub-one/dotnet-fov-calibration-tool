using FovCalibrationTool.CalibrationTool;

namespace FovCalibrationTool.FovCalculator
{
    public static class FovCalculatorUtils
    {
        public static GameStatistics CalculateGameStats(EnvironmentOptions environment, UserOptions user, GameOptions game)
        {
            var pointsPer360Deg = double.NaN;
            var pointsPerFovDeg = double.NaN;

            var viewPortDeg = game.ViewPortDeg;

            if (viewPortDeg > 0)
            {
                var viewPortWidth = CalculateViewPortWidth(environment.DisplayType, environment.DisplayDistance, user.ViewPortObserveDeg);

                var fovDeg = CalculateTargetAngle(viewPortDeg, viewPortWidth, environment.DisplayWidth);

                pointsPer360Deg = 360 / viewPortDeg * game.ViewPortPoints;
                pointsPerFovDeg = fovDeg / viewPortDeg * game.ViewPortPoints;
            }

            return new GameStatistics
            {
                PointsPer360Deg = pointsPer360Deg,
                PointsPerFovDeg = pointsPerFovDeg
            };
        }

        public static FovStatistics CalculateFovStats(FovCalculatorState state)
        {
            var pointsPer360Deg = Math.Abs(state.PointsPer360Deg);
            var pointsPerFovDeg = Math.Abs(state.PointsPerFovDeg);

            var fovDeg = double.NaN;

            var pointsPer1Deg = pointsPer360Deg / 360d;

            if (pointsPer1Deg > 0)
            {
                fovDeg = pointsPerFovDeg / pointsPer1Deg;
            }

            var viewPortWidth = CalculateViewPortWidth(state.DisplayType, state.DisplayDistance, state.ViewPortObserveDeg);
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

        private static double CalculateViewPortWidth(DisplayType displayType, double displayDistance, double viewPortObserveDeg)
        {
            var viewPortRad = viewPortObserveDeg * Math.PI / 180;

            if (displayType == DisplayType.Flat)
            {
                return Math.Tan(viewPortRad / 2) * 2 * displayDistance;
            }

            if (displayType == DisplayType.Curved)
            {
                return viewPortRad * displayDistance;
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