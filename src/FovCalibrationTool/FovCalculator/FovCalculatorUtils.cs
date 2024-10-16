namespace FovCalibrationTool.FovCalculator
{
    public static class FovCalculatorUtils
    {
        private const double DISTANCE_PER_INCH = 25.4;

        public static double CalculateMoveDistance(int movePoints, int moveDpi)
        {
            return movePoints * DISTANCE_PER_INCH / moveDpi;
        }

        public static int CalculateMovePoints(double moveDistance, int moveDpi)
        {
            var movePoints = Math.Abs(moveDistance / DISTANCE_PER_INCH * moveDpi);
            var movePointsInt = Math.Round(movePoints);

            return (int)movePointsInt;
        }

        public static PresetState CalculatePresetState(EnvironmentOptions environment, UserOptions user, PresetOptions preset)
        {
            var moveDistancePer360Deg = double.NaN;
            var moveDistancePerFovDeg = double.NaN;

            var viewPortDeg = preset.ViewPortDeg;

            if (viewPortDeg > 0)
            {
                var viewPortWidth = CalculateViewPortWidth(environment.DisplayType, environment.DisplayDistance, user.ViewPortObserveDeg);

                var fovDeg = CalculateTargetAngle(viewPortDeg, viewPortWidth, environment.DisplayWidth);

                moveDistancePer360Deg = 360 / viewPortDeg * preset.ViewPortMoveDistance;
                moveDistancePerFovDeg = fovDeg / viewPortDeg * preset.ViewPortMoveDistance;
            }

            return new PresetState(moveDistancePer360Deg, moveDistancePerFovDeg);
        }

        public static FovStatistics CalculateFovStatistics(EnvironmentOptions environment, UserOptions user, PresetState presetState)
        {
            var moveDistancePer360Deg = Math.Abs(presetState.MoveDistancePer360Deg);
            var moveDistancePerFovDeg = Math.Abs(presetState.MoveDistancePerFovDeg);

            var fovDeg = double.NaN;

            var moveDistancePer1Deg = moveDistancePer360Deg / 360d;

            if (moveDistancePer1Deg > 0)
            {
                fovDeg = moveDistancePerFovDeg / moveDistancePer1Deg;
            }

            var viewPortWidth = CalculateViewPortWidth(environment.DisplayType, environment.DisplayDistance, user.ViewPortObserveDeg);
            var viewPortDeg = CalculateTargetAngle(fovDeg, environment.DisplayWidth, viewPortWidth);

            var fovDegAngleBased = CalculateTargetAngle(user.ViewPortDeg, viewPortWidth, environment.DisplayWidth);

            var moveDistancePer1DegSensBased = double.NaN;

            if (viewPortDeg > 0)
            {
                moveDistancePer1DegSensBased = user.ViewPortMoveDistance / viewPortDeg;
            }

            var moveDistancePerViewPortDeg = moveDistancePer1Deg * viewPortDeg;
            var moveDistancePerFovDegAngleBased = moveDistancePer1Deg * fovDegAngleBased;

            var moveDistancePer360DegSensBased = moveDistancePer1DegSensBased * 360;
            var moveDistancePerFovDegSensBased = moveDistancePer1DegSensBased * fovDeg;

            return new FovStatistics
            {
                FovDeg = fovDeg,
                FovDegAngleBased = fovDegAngleBased,

                MoveDistancePer360Deg = moveDistancePer360Deg,
                MoveDistancePer1Deg = moveDistancePer1Deg,
                MoveDistancePerFovDeg = moveDistancePerFovDeg,
                MoveDistancePerViewPortDeg = moveDistancePerViewPortDeg,

                ViewPortWidth = viewPortWidth,
                ViewPortDeg = viewPortDeg,

                MoveDistancePer360DegSensBased = moveDistancePer360DegSensBased,
                MoveDistancePer1DegSensBased = moveDistancePer1DegSensBased,
                MoveDistancePerFovDegSensBased = moveDistancePerFovDegSensBased,
                MoveDistancePerFovDegAngleBased = moveDistancePerFovDegAngleBased,
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