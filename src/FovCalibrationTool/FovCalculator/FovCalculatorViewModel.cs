using FovCalibrationTool.Mvvm;

namespace FovCalibrationTool.FovCalculator
{
    public class FovCalculatorViewModel : ViewModel<FovCalculatorState>
    {
        public FovCalculatorViewModel()
        {
            var state = new FovCalculatorState(
                false,
                FovCalculatorMode.Capture360,
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
                statePrevious.Enabled,
                mode,
                statePrevious.PointsPer360Deg,
                statePrevious.PointsPerCustomDeg
            );

            UpdateState(state);
        }

        public void Enable(bool enabled)
        {
            var statePrevious = State;

            if (statePrevious.Enabled == enabled)
            {
                return;
            }

            var state = default(FovCalculatorState);

            if (enabled)
            {
                if (statePrevious.Mode == FovCalculatorMode.Capture360)
                {
                    state = new FovCalculatorState(
                        enabled,
                        statePrevious.Mode,
                        0,
                        statePrevious.PointsPerCustomDeg
                    );
                }
                if (statePrevious.Mode == FovCalculatorMode.CaptureCustom)
                {
                    state = new FovCalculatorState(
                        enabled,
                        statePrevious.Mode,
                        statePrevious.PointsPer360Deg,
                        0
                    );
                }
            }
            else
            {
                state = new FovCalculatorState(
                    enabled,
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

            if (statePrevious.Enabled == false)
            {
                return;
            }

            var state = default(FovCalculatorState);

            if (statePrevious.Mode == FovCalculatorMode.Capture360)
            {
                state = new FovCalculatorState(
                    statePrevious.Enabled,
                    statePrevious.Mode,
                    statePrevious.PointsPer360Deg + pointsDelta,
                    statePrevious.PointsPerCustomDeg
                );
            }

            if (statePrevious.Mode == FovCalculatorMode.CaptureCustom)
            {
                state = new FovCalculatorState(
                    statePrevious.Enabled,
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
