using FovCalibrationTool.Mvvm;

namespace FovCalibrationTool.FovCalculator
{
    public class FovCalculatorViewModel : ViewModel<CalculatorState>
    {
        public FovCalculatorViewModel(EnvironmentOptions environment, UserOptions user, GameOptions game)
        {
            var state = new CalculatorState(
                TrackingState.Default,
                environment,
                user,
                game
            );

            RestoreState(state);
        }

        public void UsePresetSens()
        {
            var statePrevious = State;

            var stateStatistics = FovCalculatorUtils.CalculateStatistics(
                statePrevious.Environment,
                statePrevious.User,
                statePrevious.Game
            );

            var game = new GameOptions(
                stateStatistics.PointsPer360DegSensBased,
                stateStatistics.PointsPerFovDegSensBased
            );

            var state = new CalculatorState(
                statePrevious.Tracking,
                statePrevious.Environment,
                statePrevious.User,
                game
            );

            UpdateState(state);
        }

        public void UsePresetFov()
        {
            var statePrevious = State;

            var stateStatistics = FovCalculatorUtils.CalculateStatistics(
                statePrevious.Environment,
                statePrevious.User,
                statePrevious.Game
            );

            var game = new GameOptions(
                stateStatistics.PointsPer360Deg,
                stateStatistics.PointsPerFovDegAngleBased
            );

            var state = new CalculatorState(
                statePrevious.Tracking,
                statePrevious.Environment,
                statePrevious.User,
                game
            );

            UpdateState(state);
        }

        public void ChangeMode(TrackingMode mode)
        {
            var statePrevious = State;

            var trackingPrevious = statePrevious.Tracking;

            if (trackingPrevious.Mode == mode)
            {
                return;
            }

            var tracking = new TrackingState(
                trackingPrevious.Active,
                mode
            );

            var state = new CalculatorState(
                tracking,
                statePrevious.Environment,
                statePrevious.User,
                statePrevious.Game
            );

            UpdateState(state);
        }

        public void Track(bool active, bool tune = true)
        {
            var statePrevious = State;

            var trackingPrevious = statePrevious.Tracking;

            if (trackingPrevious.Active == active ||
                trackingPrevious.Mode == TrackingMode.Disabled)
            {
                return;
            }

            var tracking = new TrackingState(
                active,
                trackingPrevious.Mode
            );

            var gamePrevious = statePrevious.Game;

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

                if (tracking.Mode == TrackingMode.Capture360)
                {
                    var game = new GameOptions(
                        GetInitialPoints(
                            gamePrevious.PointsPer360Deg
                        ),
                        gamePrevious.PointsPerFovDeg
                    );

                    var state = new CalculatorState(
                        tracking,
                        statePrevious.Environment,
                        statePrevious.User,
                        game
                    );

                    UpdateState(state);
                }
                if (tracking.Mode == TrackingMode.CaptureFov)
                {
                    var game = new GameOptions(
                        gamePrevious.PointsPer360Deg,
                        GetInitialPoints(
                            gamePrevious.PointsPerFovDeg
                        )
                    );

                    var state = new CalculatorState(
                        tracking,
                        statePrevious.Environment,
                        statePrevious.User,
                        game
                    );

                    UpdateState(state);
                }
            }
            else
            {
                var state = new CalculatorState(
                    tracking,
                    statePrevious.Environment,
                    statePrevious.User,
                    gamePrevious
                );

                UpdateState(state);
            }
        }

        public void MoveMouse(int pointsDelta)
        {
            var statePrevious = State;

            var trackingPrevious = statePrevious.Tracking;

            if (trackingPrevious.Active == false ||
                trackingPrevious.Mode == TrackingMode.Disabled)
            {
                return;
            }

            var gamePrevious = statePrevious.Game;

            if (trackingPrevious.Mode == TrackingMode.Capture360)
            {
                var game = new GameOptions(
                    gamePrevious.PointsPer360Deg + pointsDelta,
                    gamePrevious.PointsPerFovDeg
                );

                var state = new CalculatorState(
                    trackingPrevious,
                    statePrevious.Environment,
                    statePrevious.User,
                    game
                );

                UpdateState(state);
            }

            if (trackingPrevious.Mode == TrackingMode.CaptureFov)
            {
                var game = new GameOptions(
                    gamePrevious.PointsPer360Deg,
                    gamePrevious.PointsPerFovDeg + pointsDelta
                );

                var state = new CalculatorState(
                    trackingPrevious,
                    statePrevious.Environment,
                    statePrevious.User,
                    game
                );

                UpdateState(state);
            }
        }
    }
}
