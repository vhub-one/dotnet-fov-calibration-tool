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

            var fovCalculatorState = FovCalculatorState.CreateDefault(
                options.DisplayType,
                options.FovWidth,
                options.DisplayDistance,
                options.ViewPortObserveDeg,
                options.ViewPortDeg,
                options.ViewPortPoints
            );

            _fovCalculator = new FovCalculatorViewModel(fovCalculatorState);

            return Task.WhenAll(
                TrackStateChangesAsync(token),
                TrackMouseMovementAsync(token),
                TrackHotKeysAsync(token)
            );
        }

        private async Task TrackStateChangesAsync(CancellationToken token)
        {
            var stateChannel = Channel.CreateUnbounded<FovCalculatorState>();

            void stateChangeHandler(object target, ViewModelStateEventArgs<FovCalculatorState> eventArgs)
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

                foreach (var mouseMove in mouseMoveStream)
                {
                    _fovCalculator.MoveMouse(mouseMove.DeltaX);
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
                _fovCalculator.ChangeMode(FovCalculatorMode.Disabled);
            }
            if (action == CalculatorAction.Capture360)
            {
                _fovCalculator.ChangeMode(FovCalculatorMode.Capture360);
            }
            if (action == CalculatorAction.CaptureFov)
            {
                _fovCalculator.ChangeMode(FovCalculatorMode.CaptureFov);
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
                var stateDeltaAbs = FovCalculatorUtils.GetPoints(state);

                if (state.Tracking == false)
                {
                    await _movementManager.MoveByOffsetAsync(-stateDeltaAbs, 0, token);
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
                var stateDeltaAbs = FovCalculatorUtils.GetPoints(state);

                if (state.Tracking == false)
                {
                    await _movementManager.MoveByOffsetAsync(stateDeltaAbs, 0, token);
                }
            }
            if (action == CalculatorAction.MoveLeftBy1)
            {
                await _movementManager.MoveByOffsetAsync(-1, 0, token);
            }

            #endregion
        }

        private ValueTask DrawStateAsync(FovCalculatorState state, CancellationToken token)
        {
            var stateStats = FovCalculatorUtils.CalculateStatistics(state);

            DrawPane(60, 0, pane =>
            {
                pane.DrawLine("# PRESET-BASED FOV");
                pane.DrawLine("preset view port angle", state.ViewPortDeg);
                pane.DrawLine("fov angle", stateStats.FovDegAngleBased);
                pane.DrawLine("fov points", stateStats.PointsPerFovDegAngleBased);
            });

            DrawPane(60, 5, pane =>
            {
                pane.DrawLine("# PRESET-BASED SENSITIVITY");
                pane.DrawLine("preset view port points", state.PointsPerViewPortDeg);
                pane.DrawLine("fov points", stateStats.PointsPerFovDegSensBased);
                pane.DrawLine("360 deg points", stateStats.PointsPer360DegSensBased);
                pane.DrawLine("1 deg points", stateStats.PointsPer1DegSensBased);
            });

            DrawPane(0, 0, pane =>
            {
                var stateActive = state.Mode == FovCalculatorMode.Capture360;

                if (stateActive && state.Tracking)
                {
                    pane.HighlightPane();
                }

                pane.DrawHeader("#1 SET 360 DEG", stateActive);
                pane.DrawLine("360 deg points", stateStats.PointsPer360Deg);
                pane.DrawLine("1 deg points", stateStats.PointsPer1Deg);
            });

            DrawPane(0, 4, pane =>
            {
                var stateActive = state.Mode == FovCalculatorMode.CaptureFov;

                if (stateActive && state.Tracking)
                {
                    pane.HighlightPane();
                }

                pane.DrawHeader("#2 SET FOV", stateActive);
                pane.DrawLine("fov points", stateStats.PointsPerFovDeg);
                pane.DrawLine("fov angle", stateStats.FovDeg);
                pane.DrawLine("fov width", state.FovWidth);
            });

            DrawPane(0, 9, pane =>
            {
                pane.DrawLine("# PRESET");
                pane.DrawLine("view port points", stateStats.PointsPerViewPortDeg);
                pane.DrawLine("view port angle", stateStats.ViewPortDeg);
                pane.DrawLine("view port width", stateStats.ViewPortWidth);
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
