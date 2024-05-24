using FovCalibrationTool.FovCalculator;
using FovCalibrationTool.Keyboard.HotKeys;
using FovCalibrationTool.Mouse.MovementManager;
using FovCalibrationTool.Mouse.MovementTracker;
using FovCalibrationTool.Mvvm;
using FovCalibrationTool.Render;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading.Channels;
using System.Windows.Forms;

namespace FovCalibrationTool.CalibrationTool
{
    public class CalibrationToolService : BackgroundService
    {
        private static readonly HotKey KeySetMode0 = new(Keys.NumPad0);
        private static readonly HotKey KeySetMode1 = new(Keys.NumPad1);
        private static readonly HotKey KeySetMode2 = new(Keys.NumPad2);
        private static readonly HotKey KeyMoveLeft = new(Keys.NumPad4);
        private static readonly HotKey KeyUseEstiamtedSens = new(Keys.NumPad5);
        private static readonly HotKey KeyMoveRight = new(Keys.NumPad6);
        private static readonly HotKey KeyMoveLeftBy1 = new(Keys.NumPad7);
        private static readonly HotKey KeyUseEstiamtedAngle = new(Keys.NumPad8);
        private static readonly HotKey KeyMoveRightBy1 = new(Keys.NumPad9);
        private static readonly HotKey KeyTrack = new(Keys.LControlKey);

        private readonly MouseMovementTracker _movementTracker;
        private readonly MouseMovementManager _movementManager;
        private readonly HotKeysTracker _hotKeysTracker;
        private readonly IOptions<CalibrationToolOptions> _optionsAccessor;

        private FovCalculatorViewModel _fovCalculator;

        public CalibrationToolService(MouseMovementTracker movementTracker, MouseMovementManager movementManager, HotKeysTracker hotKeysTracker, IOptions<CalibrationToolOptions> optionsAccessor)
        {
            _movementTracker = movementTracker;
            _movementManager = movementManager;
            _hotKeysTracker = hotKeysTracker;
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
                    DrawState(state);

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
            var hotKeys = new[]
            {
                KeySetMode0,
                KeySetMode1,
                KeySetMode2,
                KeyMoveLeft,
                KeyMoveLeftBy1,
                KeyMoveRight,
                KeyMoveRightBy1,
                KeyUseEstiamtedSens,
                KeyUseEstiamtedAngle,
                KeyTrack
            };

            var hotKeyStatusStream = _hotKeysTracker.TrackAsync(hotKeys, token);

            await foreach (var (hotKey, direction) in hotKeyStatusStream)
            {
                if (hotKey.HasHotKey(KeyTrack))
                {
                    if (direction == HotKeyDirection.Down)
                    {
                        _fovCalculator.Track(true, hotKey.KeysModifier.HasFlag(Keys.Shift));
                    }
                    if (direction == HotKeyDirection.Up)
                    {
                        _fovCalculator.Track(false);
                    }
                }

                if (direction == HotKeyDirection.Down)
                {
                    if (hotKey.HasHotKey(KeySetMode0))
                    {
                        _fovCalculator.ChangeMode(FovCalculatorMode.Disabled);
                    }
                    if (hotKey.HasHotKey(KeySetMode1))
                    {
                        _fovCalculator.ChangeMode(FovCalculatorMode.Capture360);
                    }
                    if (hotKey.HasHotKey(KeySetMode2))
                    {
                        _fovCalculator.ChangeMode(FovCalculatorMode.CaptureFov);
                    }
                    if (hotKey.HasHotKey(KeyUseEstiamtedSens))
                    {
                        _fovCalculator.UseEstiamtedSens();
                    }
                    if (hotKey.HasHotKey(KeyUseEstiamtedAngle))
                    {
                        _fovCalculator.UseEstiamtedAngle();
                    }
                    if (hotKey.HasHotKey(KeyMoveLeft) || hotKey.HasHotKey(KeyMoveRight))
                    {
                        var state = _fovCalculator.State;
                        var stateDeltaAbs = FovCalculatorUtils.GetPoints(state);

                        if (hotKey.HasHotKey(KeyMoveLeft))
                        {
                            _movementManager.MoveByOffset(-stateDeltaAbs, 0);
                        }
                        if (hotKey.HasHotKey(KeyMoveRight))
                        {
                            _movementManager.MoveByOffset(stateDeltaAbs, 0);
                        }
                    }
                    if (hotKey.HasHotKey(KeyMoveLeftBy1) || hotKey.HasHotKey(KeyMoveRightBy1))
                    {
                        if (hotKey.HasHotKey(KeyMoveLeftBy1))
                        {
                            _movementManager.MoveByOffset(-1, 0);
                        }
                        if (hotKey.HasHotKey(KeyMoveRightBy1))
                        {
                            _movementManager.MoveByOffset(1, 0);
                        }
                    }
                }
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

        private void DrawState(FovCalculatorState state)
        {
            var stateStats = FovCalculatorUtils.CalculateStatistics(state);

            DrawPane(60, 0, pane =>
            {
                pane.DrawLine("# HOT KEYS");
                pane.DrawLine("[numpad 0]", "disable");
                pane.DrawLine("[numpad 1]", "set 360 points");
                pane.DrawLine("[numpad 2]", "set FOV points");
                pane.DrawLine("[numpad 4]", "move left");
                pane.DrawLine("[numpad 5]", "use preset sens");
                pane.DrawLine("[numpad 6]", "move right");
                pane.DrawLine("[numpad 7]", "move left by 1");
                pane.DrawLine("[numpad 8]", "use preset fov");
                pane.DrawLine("[numpad 9]", "move right by 1");
                pane.DrawLine("[ctrl]", "set points");
                pane.DrawLine("[shift] + [ctrl]", "tune points");
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

            DrawPane(0, 14, pane =>
            {
                pane.DrawLine("# PRESET-BASED FOV");
                pane.DrawLine("preset view port angle", state.ViewPortDeg);
                pane.DrawLine("fov angle", stateStats.FovDegAngleBased);
                pane.DrawLine("fov points", stateStats.PointsPerFovDegAngleBased);
            });

            DrawPane(0, 19, pane =>
            {
                pane.DrawLine("# PRESET-BASED SENSITIVITY");
                pane.DrawLine("preset view port points", state.PointsPerViewPortDeg);
                pane.DrawLine("fov points", stateStats.PointsPerFovDegSensBased);
                pane.DrawLine("360 deg points", stateStats.PointsPer360DegSensBased);
                pane.DrawLine("1 deg points", stateStats.PointsPer1DegSensBased);
            });
        }

        private static void DrawPane(int x, int y, Action<ConsolePane> paneDrawAction)
        {
            using (var pane = new ConsolePane(x, y))
            {
                paneDrawAction(pane);
            }
        }
    }
}
