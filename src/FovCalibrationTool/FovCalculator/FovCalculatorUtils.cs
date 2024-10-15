namespace FovCalibrationTool.FovCalculator
{
    public static class FovCalculatorUtils
    {
        public static GameOptions CalculateOptions(EnvironmentOptions environment, UserOptions user, GamePresetOptions game)
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

            return new GameOptions(pointsPer360Deg, pointsPerFovDeg);
        }

        public static FovStatistics CalculateStatistics(EnvironmentOptions environment, UserOptions user, GameOptions game)
        {
            var pointsPer360Deg = Math.Abs(game.PointsPer360Deg);
            var pointsPerFovDeg = Math.Abs(game.PointsPerFovDeg);

            var fovDeg = double.NaN;

            var pointsPer1Deg = pointsPer360Deg / 360d;

            if (pointsPer1Deg > 0)
            {
                fovDeg = pointsPerFovDeg / pointsPer1Deg;
            }

            var viewPortWidth = CalculateViewPortWidth(environment.DisplayType, environment.DisplayDistance, user.ViewPortObserveDeg);
            var viewPortDeg = CalculateTargetAngle(fovDeg, environment.DisplayWidth, viewPortWidth);

            var fovDegAngleBased = CalculateTargetAngle(user.ViewPortDeg, viewPortWidth, environment.DisplayWidth);

            var pointsPer1DegSensBased = double.NaN;

            if (viewPortDeg > 0)
            {
                pointsPer1DegSensBased = user.ViewPortPoints / viewPortDeg;
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