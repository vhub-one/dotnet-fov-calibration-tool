using FovCalibrationTool.Mvvm;

namespace FovCalibrationTool.FovCalculator
{
    public class FovCalculatorViewModel : ViewModel<FovCalculatorState>
    {
        public FovCalculatorViewModel(FovCalculatorState state)
        {
            RestoreState(state);
        }

        public void UseEstimate()
        {
            var statePrevious = State;

            var stateStatistics = FovCalculatorUtils.CalculateStatistics(statePrevious);

            var state = new FovCalculatorState(
                statePrevious.Tracking,
                statePrevious.Mode,
                statePrevious.FovDistance,
                statePrevious.ViewPortDistance,
                stateStatistics.PointsPer360DegEstimate,
                stateStatistics.PointsPerFovDegEstimate,
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
                statePrevious.FovDistance,
                statePrevious.ViewPortDistance,
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
                if (statePrevious.Mode == FovCalculatorMode.Capture360)
                {
                    state = new FovCalculatorState(
                        Tracking: tracking,
                        statePrevious.Mode,
                        statePrevious.FovDistance,
                        statePrevious.ViewPortDistance,
                        PointsPer360Deg: tune ? statePrevious.PointsPer360Deg : 0,
                        statePrevious.PointsPerFovDeg,
                        statePrevious.PointsPerViewPortDeg
                    );
                }
                if (statePrevious.Mode == FovCalculatorMode.CaptureFov)
                {
                    state = new FovCalculatorState(
                        Tracking: tracking,
                        statePrevious.Mode,
                        statePrevious.FovDistance,
                        statePrevious.ViewPortDistance,
                        statePrevious.PointsPer360Deg,
                        PointsPerFovDeg: tune ? statePrevious.PointsPerFovDeg : 0,
                        statePrevious.PointsPerViewPortDeg
                    );
                }
                if (statePrevious.Mode == FovCalculatorMode.CaptureViewPort)
                {
                    state = new FovCalculatorState(
                        Tracking: tracking,
                        statePrevious.Mode,
                        statePrevious.FovDistance,
                        statePrevious.ViewPortDistance,
                        statePrevious.PointsPer360Deg,
                        statePrevious.PointsPerFovDeg,
                        PointsPerViewPortDeg: tune ? statePrevious.PointsPerViewPortDeg : 0
                    );
                }
            }
            else
            {
                state = new FovCalculatorState(
                    Tracking: tracking,
                    statePrevious.Mode,
                    statePrevious.FovDistance,
                    statePrevious.ViewPortDistance,
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
                    statePrevious.FovDistance,
                    statePrevious.ViewPortDistance,
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
                    statePrevious.FovDistance,
                    statePrevious.ViewPortDistance,
                    statePrevious.PointsPer360Deg,
                    statePrevious.PointsPerFovDeg + pointsDelta,
                    statePrevious.PointsPerViewPortDeg
                );
            }

            if (statePrevious.Mode == FovCalculatorMode.CaptureViewPort)
            {
                state = new FovCalculatorState(
                    statePrevious.Tracking,
                    statePrevious.Mode,
                    statePrevious.FovDistance,
                    statePrevious.ViewPortDistance,
                    statePrevious.PointsPer360Deg,
                    statePrevious.PointsPerFovDeg,
                    statePrevious.PointsPerViewPortDeg + pointsDelta
                );
            }

            if (state != null)
            {
                UpdateState(state);
            }
        }
    }
}
