using FovCalibrationTool.Mvvm;

namespace FovCalibrationTool.FovCalculator
{
    public class FovCalculatorViewModel : ViewModel<FovCalculatorState>
    {
        public FovCalculatorViewModel()
        {
            var state = new FovCalculatorState(
                false,
                FovCalculatorMode.Disabled,
                0,
                0
            );

            RestoreState(state);
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
                mode,
                statePrevious.PointsPer360Deg,
                statePrevious.PointsPerCustomDeg
            );

            UpdateState(state);
        }

        public void Track(bool tracking)
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
                        tracking,
                        statePrevious.Mode,
                        0,
                        statePrevious.PointsPerCustomDeg
                    );
                }
                if (statePrevious.Mode == FovCalculatorMode.CaptureCustom)
                {
                    state = new FovCalculatorState(
                        tracking,
                        statePrevious.Mode,
                        statePrevious.PointsPer360Deg,
                        0
                    );
                }
            }
            else
            {
                state = new FovCalculatorState(
                    tracking,
                    statePrevious.Mode,
                    statePrevious.PointsPer360Deg,
                    statePrevious.PointsPerCustomDeg
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
                    statePrevious.PointsPer360Deg + pointsDelta,
                    statePrevious.PointsPerCustomDeg
                );
            }

            if (statePrevious.Mode == FovCalculatorMode.CaptureCustom)
            {
                state = new FovCalculatorState(
                    statePrevious.Tracking,
                    statePrevious.Mode,
                    statePrevious.PointsPer360Deg,
                    statePrevious.PointsPerCustomDeg + pointsDelta
                );
            }

            if (state != null)
            {
                UpdateState(state);
            }
        }
    }
}
