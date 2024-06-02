using FovCalibrationTool.Mvvm;

namespace FovCalibrationTool.FovCalculator
{
    public class FovCalculatorViewModel : ViewModel<FovCalculatorState>
    {
        public FovCalculatorViewModel(FovCalculatorState state)
        {
            RestoreState(state);
        }

        public void UsePresetSens()
        {
            var statePrevious = State;

            var stateStatistics = FovCalculatorUtils.CalculateStatistics(statePrevious);

            var state = new FovCalculatorState(
                statePrevious.Tracking,
                statePrevious.Mode,
                statePrevious.DisplayType,
                statePrevious.DisplayDistance,
                statePrevious.FovWidth,
                statePrevious.ViewPortDeg,
                statePrevious.ViewPortObserveDeg,
                stateStatistics.PointsPer360DegSensBased,
                stateStatistics.PointsPerFovDegSensBased,
                statePrevious.PointsPerViewPortDeg
            );

            UpdateState(state);
        }

        public void UsePresetFov()
        {
            var statePrevious = State;

            var stateStatistics = FovCalculatorUtils.CalculateStatistics(statePrevious);

            var state = new FovCalculatorState(
                statePrevious.Tracking,
                statePrevious.Mode,
                statePrevious.DisplayType,
                statePrevious.DisplayDistance,
                statePrevious.FovWidth,
                statePrevious.ViewPortDeg,
                statePrevious.ViewPortObserveDeg,
                statePrevious.PointsPer360Deg,
                stateStatistics.PointsPerFovDegAngleBased,
                statePrevious.PointsPerViewPortDeg
            );

            UpdateState(state);
        }

        public void ChangeMode(FovCalculatorMode mode)
        {
            var statePrevious = State;

            if (statePrevious.Mode == mode)
            {
                return;
            }

            var state = new FovCalculatorState(
                statePrevious.Tracking,
                Mode: mode,
                statePrevious.DisplayType,
                statePrevious.DisplayDistance,
                statePrevious.FovWidth,
                statePrevious.ViewPortDeg,
                statePrevious.ViewPortObserveDeg,
                statePrevious.PointsPer360Deg,
                statePrevious.PointsPerFovDeg,
                statePrevious.PointsPerViewPortDeg
            );

            UpdateState(state);
        }

        public void Track(bool tracking, bool tune = true)
        {
            var statePrevious = State;

            if (statePrevious.Tracking == tracking ||
                statePrevious.Mode == FovCalculatorMode.Disabled)
            {
                return;
            }

            var state = default(FovCalculatorState);

            if (tracking)
            {
                double GetInitialPoints(double pointsPrevious)
                {
                    if (double.IsFinite(pointsPrevious) && tune)
                    {
                        return pointsPrevious;
                    }

                    return 0;
                }

                if (statePrevious.Mode == FovCalculatorMode.Capture360)
                {
                    state = new FovCalculatorState(
                        Tracking: tracking,
                        statePrevious.Mode,
                        statePrevious.DisplayType,
                        statePrevious.DisplayDistance,
                        statePrevious.FovWidth,
                        statePrevious.ViewPortDeg,
                        statePrevious.ViewPortObserveDeg,
                        PointsPer360Deg: GetInitialPoints(statePrevious.PointsPer360Deg),
                        statePrevious.PointsPerFovDeg,
                        statePrevious.PointsPerViewPortDeg
                    );
                }
                if (statePrevious.Mode == FovCalculatorMode.CaptureFov)
                {
                    state = new FovCalculatorState(
                        Tracking: tracking,
                        statePrevious.Mode,
                        statePrevious.DisplayType,
                        statePrevious.DisplayDistance,
                        statePrevious.FovWidth,
                        statePrevious.ViewPortDeg,
                        statePrevious.ViewPortObserveDeg,
                        statePrevious.PointsPer360Deg,
                        PointsPerFovDeg: GetInitialPoints(statePrevious.PointsPerFovDeg),
                        statePrevious.PointsPerViewPortDeg
                    );
                }
            }
            else
            {
                state = new FovCalculatorState(
                    Tracking: tracking,
                    statePrevious.Mode,
                    statePrevious.DisplayType,
                    statePrevious.DisplayDistance,
                    statePrevious.FovWidth,
                    statePrevious.ViewPortDeg,
                    statePrevious.ViewPortObserveDeg,
                    statePrevious.PointsPer360Deg,
                    statePrevious.PointsPerFovDeg,
                    statePrevious.PointsPerViewPortDeg
                );
            }

            if (state != null)
            {
                UpdateState(state);
            }
        }

        public void MoveMouse(int pointsDelta)
        {
            var statePrevious = State;

            if (statePrevious.Tracking == false ||
                statePrevious.Mode == FovCalculatorMode.Disabled)
            {
                return;
            }

            var state = default(FovCalculatorState);

            if (statePrevious.Mode == FovCalculatorMode.Capture360)
            {
                state = new FovCalculatorState(
                    statePrevious.Tracking,
                    statePrevious.Mode,
                    statePrevious.DisplayType,
                    statePrevious.DisplayDistance,
                    statePrevious.FovWidth,
                    statePrevious.ViewPortDeg,
                    statePrevious.ViewPortObserveDeg,
                    statePrevious.PointsPer360Deg + pointsDelta,
                    statePrevious.PointsPerFovDeg,
                    statePrevious.PointsPerViewPortDeg
                );
            }

            if (statePrevious.Mode == FovCalculatorMode.CaptureFov)
            {
                state = new FovCalculatorState(
                    statePrevious.Tracking,
                    statePrevious.Mode,
                    statePrevious.DisplayType,
                    statePrevious.DisplayDistance,
                    statePrevious.FovWidth,
                    statePrevious.ViewPortDeg,
                    statePrevious.ViewPortObserveDeg,
                    statePrevious.PointsPer360Deg,
                    statePrevious.PointsPerFovDeg + pointsDelta,
                    statePrevious.PointsPerViewPortDeg
                );
            }

            if (state != null)
            {
                UpdateState(state);
            }
        }
    }
}
