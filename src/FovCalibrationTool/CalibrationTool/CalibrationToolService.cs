using FovCalibrationTool.FovCalculator;
using FovCalibrationTool.Mouse.MovementManager;
using FovCalibrationTool.Mouse.MovementTracker;
using FovCalibrationTool.Mvvm;
using FovCalibrationTool.Render;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading.Channels;

namespace FovCalibrationTool.CalibrationTool
{
    public class CalibrationToolService : BackgroundService
    {
        private readonly IOptions<CalibrationToolOptions> _optionsAccessor;

        private readonly MouseMovementTracker _movementTracker;
        private readonly MouseMovementManager _movementManager;
        private readonly CalculatorActionController _calculatorController;

        private FovCalculatorViewModel _fovCalculator;

        public CalibrationToolService(MouseMovementTracker movementTracker, MouseMovementManager movementManager, CalculatorActionController calculatorController, IOptions<CalibrationToolOptions> optionsAccessor)
        {
            _movementTracker = movementTracker;
            _movementManager = movementManager;
            _calculatorController = calculatorController;
            _optionsAccessor = optionsAccessor;
        }

        protected override Task ExecuteAsync(CancellationToken token)
        {
            var options = _optionsAccessor.Value;

            if (options == null)
            {
                throw new InvalidOperationException();
            }

            var environment = options.Environment;
            var user = options.User;
            var preset = options.Preset;

            if (environment == null || user == null)
            {
                throw new InvalidOperationException();
            }

            _fovCalculator = new FovCalculatorViewModel(
                environment,
                user,
                preset
            );

            return Task.WhenAll(
                TrackStateChangesAsync(token),
                TrackMouseMovementAsync(token),
                TrackHotKeysAsync(token)
            );
        }

        private async Task TrackStateChangesAsync(CancellationToken token)
        {
            var stateChannel = Channel.CreateUnbounded<CalculatorState>();

            void stateChangeHandler(object target, ViewModelStateEventArgs<CalculatorState> eventArgs)
            {
                stateChannel.Writer.TryWrite(eventArgs.State);
            }

            _fovCalculator.StateChanged += stateChangeHandler;

            try
            {
                // Get initial state
                var state = _fovCalculator.State;

                while (true)
                {
                    await DrawStateAsync(state, token);

                    // Get updated state
                    state = await stateChannel.Reader.ReadAsync(token);
                }
            }
            finally
            {
                _fovCalculator.StateChanged -= stateChangeHandler;
            }
        }

        private async Task TrackHotKeysAsync(CancellationToken token)
        {
            var actionsStreamDispatcher = new SyncDispatcher();
            var actionsStream = _calculatorController.TrackActionAsync(token);

            await foreach (var action in actionsStream)
            {
                await actionsStreamDispatcher.ExecuteAsync(
                    actionToken => HandleActionAsync(action, actionToken),
                    action == CalculatorAction.Stop
                );
            }
        }

        private Task TrackMouseMovementAsync(CancellationToken token)
        {
            void HandleMouseMovement()
            {
                var mouseMoveStream = _movementTracker.Track(token);

                foreach (var movement in mouseMoveStream)
                {
                    var state = _fovCalculator.State;

                    var moveDistance = FovCalculatorUtils.CalculateMoveDistance(
                        movement.DeltaX,
                        state.Environment.MoveDpi
                    );

                    _fovCalculator.MoveMouse(moveDistance);
                }
            }

            return Task.Run(HandleMouseMovement, token);
        }

        private async Task HandleActionAsync(CalculatorAction action, CancellationToken token)
        {
            #region TRACKING

            if (action == CalculatorAction.StartTracking)
            {
                _fovCalculator.Track(true, false);
            }
            if (action == CalculatorAction.StartTrackingPrecise)
            {
                _fovCalculator.Track(true, true);
            }
            if (action == CalculatorAction.Stop)
            {
                _fovCalculator.Track(false);
            }

            #endregion

            #region CHANGE MODE

            if (action == CalculatorAction.Disable)
            {
                _fovCalculator.ChangeMode(TrackingMode.Disabled);
            }
            if (action == CalculatorAction.Capture360)
            {
                _fovCalculator.ChangeMode(TrackingMode.Capture360);
            }
            if (action == CalculatorAction.CaptureFov)
            {
                _fovCalculator.ChangeMode(TrackingMode.CaptureFov);
            }

            #endregion

            #region USE PRESET

            if (action == CalculatorAction.UsePresetSens)
            {
                _fovCalculator.UsePresetSens();
            }
            if (action == CalculatorAction.UsePresetFov)
            {
                _fovCalculator.UsePresetFov();
            }

            #endregion

            #region MOVE LEFT

            if (action == CalculatorAction.MoveLeft)
            {
                var state = _fovCalculator.State;

                var trackingMovePoints = GetMovePoints(state);

                if (trackingMovePoints != 0)
                {
                    await _movementManager.MoveByOffsetAsync(-trackingMovePoints, 0, token);
                }
            }
            if (action == CalculatorAction.MoveRightBy1)
            {
                await _movementManager.MoveByOffsetAsync(1, 0, token);
            }

            #endregion

            #region MOVE RIGHT

            if (action == CalculatorAction.MoveRight)
            {
                var state = _fovCalculator.State;

                var trackingMovePoints = GetMovePoints(state);

                if (trackingMovePoints != 0)
                {
                    await _movementManager.MoveByOffsetAsync(trackingMovePoints, 0, token);
                }
            }
            if (action == CalculatorAction.MoveLeftBy1)
            {
                await _movementManager.MoveByOffsetAsync(-1, 0, token);
            }

            #endregion
        }

        private static int GetMovePoints(CalculatorState state)
        {
            var tracking = state.TrackingState;

            if (tracking.Active == false)
            {
                var moveDistance = 0d;

                if (tracking.Mode == TrackingMode.Capture360)
                {
                    moveDistance = state.PresetState.MoveDistancePer360Deg;
                }
                if (tracking.Mode == TrackingMode.CaptureFov)
                {
                    moveDistance = state.PresetState.MoveDistancePerFovDeg;
                }

                if (double.IsFinite(moveDistance))
                {
                    return FovCalculatorUtils.CalculateMovePoints(moveDistance, state.Environment.MoveDpi);
                }
            }

            return 0;
        }

        private static ValueTask DrawStateAsync(CalculatorState state, CancellationToken token)
        {
            var stateStats = FovCalculatorUtils.CalculateFovStatistics(
                state.Environment,
                state.User,
                state.PresetState
            );

            DrawPane(60, 0, pane =>
            {
                pane.DrawLine("# PRESET");
                pane.DrawLine("view port move distance", state.User.ViewPortMoveDistance);
                pane.DrawLine("view port angle", state.User.ViewPortDeg);
                pane.DrawLine("view port width", stateStats.ViewPortWidth);
            });

            DrawPane(60, 5, pane =>
            {
                pane.DrawLine("# PRESET-BASED FOV");
                pane.DrawLine("fov move distance", stateStats.MoveDistancePerFovDegAngleBased);
                pane.DrawLine("fov angle", stateStats.FovDegAngleBased);
            });

            DrawPane(60, 9, pane =>
            {
                pane.DrawLine("# PRESET-BASED SENSITIVITY");
                pane.DrawLine("360 deg move distance", stateStats.MoveDistancePer360DegSensBased);
                pane.DrawLine("fov move distance", stateStats.MoveDistancePerFovDegSensBased);
            });

            DrawPane(0, 0, pane =>
            {
                var trackingState = state.TrackingState;
                var trackingStateSelected = trackingState.Mode == TrackingMode.Capture360;

                if (trackingState.Active && trackingStateSelected)
                {
                    pane.HighlightNext();
                }

                pane.DrawHeader("# GAME 360 DEG", trackingStateSelected);
                pane.DrawLine("360 deg move distance", stateStats.MoveDistancePer360Deg);
                pane.DrawLine("1 deg move distance", stateStats.MoveDistancePer1Deg);
            });

            DrawPane(0, 4, pane =>
            {
                var trackingState = state.TrackingState;
                var trackingStateSelected = trackingState.Mode == TrackingMode.CaptureFov;

                if (trackingState.Active && trackingStateSelected)
                {
                    pane.HighlightNext();
                }

                pane.DrawHeader("# GAME FOV", trackingStateSelected);
                pane.DrawLine("fov move distance", stateStats.MoveDistancePerFovDeg);
                pane.DrawLine("fov angle", stateStats.FovDeg);
                pane.DrawLine("fov width", state.Environment.DisplayWidth);
            });

            DrawPane(0, 9, pane =>
            {
                pane.DrawLine("# GAME STATS");
                pane.DrawLine("view port move distance", stateStats.MoveDistancePerViewPortDeg);
                pane.DrawLine("view port angle", stateStats.ViewPortDeg);
            });

            return ValueTask.CompletedTask;
        }

        private static void DrawPane(int x, int y, Action<ConsolePane> paneDrawAction)
        {
            using var pane = new ConsolePane(x, y);

            if (paneDrawAction != null)
            {
                paneDrawAction(pane);
            }
        }
    }
}
