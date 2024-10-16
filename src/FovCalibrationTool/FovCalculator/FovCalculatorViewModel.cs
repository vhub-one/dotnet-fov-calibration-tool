using FovCalibrationTool.Mvvm;

namespace FovCalibrationTool.FovCalculator
{
    public class FovCalculatorViewModel : ViewModel<CalculatorState>
    {
        public FovCalculatorViewModel(EnvironmentOptions environment, UserOptions user, PresetOptions preset)
        {
            var presetState = PresetState.Default;

            if (preset != null)
            {
                presetState = FovCalculatorUtils.CalculatePresetState(
                    environment,
                    user,
                    preset
                );
            }

            var state = new CalculatorState(
                environment,
                user,
                TrackingState.Default,
                presetState
            );

            RestoreState(state);
        }

        public void UsePresetSens()
        {
            var statePrevious = State;

            var stateStatistics = FovCalculatorUtils.CalculateFovStatistics(
                statePrevious.Environment,
                statePrevious.User,
                statePrevious.PresetState
            );

            var presetState = new PresetState(
                stateStatistics.MoveDistancePer360DegSensBased,
                stateStatistics.MoveDistancePerFovDegSensBased
            );

            var state = new CalculatorState(
                statePrevious.Environment,
                statePrevious.User,
                statePrevious.TrackingState,
                presetState
            );

            UpdateState(state);
        }

        public void UsePresetFov()
        {
            var statePrevious = State;

            var stateStatistics = FovCalculatorUtils.CalculateFovStatistics(
                statePrevious.Environment,
                statePrevious.User,
                statePrevious.PresetState
            );

            var presetState = new PresetState(
                stateStatistics.MoveDistancePer360Deg,
                stateStatistics.MoveDistancePerFovDegAngleBased
            );

            var state = new CalculatorState(
                statePrevious.Environment,
                statePrevious.User,
                statePrevious.TrackingState,
                presetState
            );

            UpdateState(state);
        }

        public void ChangeMode(TrackingMode mode)
        {
            var statePrevious = State;

            var trackingStatePrevious = statePrevious.TrackingState;

            if (trackingStatePrevious.Mode == mode)
            {
                return;
            }

            var trackingState = new TrackingState(
                trackingStatePrevious.Active,
                mode
            );

            var state = new CalculatorState(
                statePrevious.Environment,
                statePrevious.User,
                trackingState,
                statePrevious.PresetState
            );

            UpdateState(state);
        }

        public void Track(bool active, bool tune = true)
        {
            var statePrevious = State;

            var trackingStatePrevious = statePrevious.TrackingState;

            if (trackingStatePrevious.Active == active ||
                trackingStatePrevious.Mode == TrackingMode.Disabled)
            {
                return;
            }

            var trackingState = new TrackingState(
                active,
                trackingStatePrevious.Mode
            );

            var presetStatePrevious = statePrevious.PresetState;

            if (active)
            {
                double GetInitialPoints(double pointsPrevious)
                {
                    if (double.IsFinite(pointsPrevious) && tune)
                    {
                        return pointsPrevious;
                    }

                    return 0;
                }

                if (trackingState.Mode == TrackingMode.Capture360)
                {
                    var presetState = new PresetState(
                        GetInitialPoints(
                            presetStatePrevious.MoveDistancePer360Deg
                        ),
                        presetStatePrevious.MoveDistancePerFovDeg
                    );

                    var state = new CalculatorState(
                        statePrevious.Environment,
                        statePrevious.User,
                        trackingState,
                        presetState
                    );

                    UpdateState(state);
                }
                if (trackingState.Mode == TrackingMode.CaptureFov)
                {
                    var presetState = new PresetState(
                        presetStatePrevious.MoveDistancePer360Deg,
                        GetInitialPoints(
                            presetStatePrevious.MoveDistancePerFovDeg
                        )
                    );

                    var state = new CalculatorState(
                        statePrevious.Environment,
                        statePrevious.User,
                        trackingState,
                        presetState
                    );

                    UpdateState(state);
                }
            }
            else
            {
                var state = new CalculatorState(
                    statePrevious.Environment,
                    statePrevious.User,
                    trackingState,
                    presetStatePrevious
                );

                UpdateState(state);
            }
        }

        public void MoveMouse(double moveDistance)
        {
            var statePrevious = State;

            var trackingStatePrevious = statePrevious.TrackingState;

            if (trackingStatePrevious.Active == false ||
                trackingStatePrevious.Mode == TrackingMode.Disabled)
            {
                return;
            }

            var presetStatePrevious = statePrevious.PresetState;

            if (trackingStatePrevious.Mode == TrackingMode.Capture360)
            {
                var presetState = new PresetState(
                    presetStatePrevious.MoveDistancePer360Deg + moveDistance,
                    presetStatePrevious.MoveDistancePerFovDeg
                );

                var state = new CalculatorState(
                    statePrevious.Environment,
                    statePrevious.User,
                    trackingStatePrevious,
                    presetState
                );

                UpdateState(state);
            }

            if (trackingStatePrevious.Mode == TrackingMode.CaptureFov)
            {
                var presetState = new PresetState(
                    presetStatePrevious.MoveDistancePer360Deg,
                    presetStatePrevious.MoveDistancePerFovDeg + moveDistance
                );

                var state = new CalculatorState(
                    statePrevious.Environment,
                    statePrevious.User,
                    trackingStatePrevious,
                    presetState
                );

                UpdateState(state);
            }
        }
    }
}
