using FovCalibrationTool.FovCalculator;
using FovCalibrationTool.Keyboard.HotKeys;
using FovCalibrationTool.Mouse.MovementManager;
using FovCalibrationTool.Mouse.MovementTracker;
using FovCalibrationTool.Mvvm;
using Microsoft.Extensions.Hosting;
using System.Threading.Channels;
using System.Windows.Forms;

namespace FovCalibrationTool.CalibrationTool
{
    public class CalibrationToolService : BackgroundService
    {
        private static readonly HotKey KeySetMode1 = new(Keys.NumPad1);
        private static readonly HotKey KeySetMode2 = new(Keys.NumPad2);
        private static readonly HotKey KeyMoveLeft = new(Keys.NumPad4);
        private static readonly HotKey KeyMoveRight = new(Keys.NumPad6);
        private static readonly HotKey KeyEnable = new(Keys.LControlKey);
        private static readonly HotKey KeyDisable = new(Keys.LControlKey, Keys.Control);

        private readonly FovCalculatorViewModel _fovCalculator = new();

        private readonly MouseMovementTracker _movementTracker;
        private readonly MouseMovementManager _movementManager;
        private readonly HotKeysTracker _hotKeysTracker;

        public CalibrationToolService(MouseMovementTracker movementTracker, MouseMovementManager movementManager, HotKeysTracker hotKeysTracker)
        {
            _movementTracker = movementTracker;
            _movementManager = movementManager;
            _hotKeysTracker = hotKeysTracker;
        }

        protected override Task ExecuteAsync(CancellationToken token)
        {
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
                KeySetMode1,
                KeySetMode2,
                KeyMoveLeft,
                KeyMoveRight,
                KeyEnable,
                KeyDisable
            };

            var hotKeyStatusStream = _hotKeysTracker.TrackAsync(hotKeys, token);

            await foreach (var hotKeyStatus in hotKeyStatusStream)
            {
                if (hotKeyStatus.HotKey == KeyEnable ||
                    hotKeyStatus.HotKey == KeyDisable)
                {
                    if (hotKeyStatus.HotKeyDirection == HotKeyDirection.Down)
                    {
                        _fovCalculator.Enable(true);
                    }
                    if (hotKeyStatus.HotKeyDirection == HotKeyDirection.Up)
                    {
                        _fovCalculator.Enable(false);
                    }
                }

                if (hotKeyStatus.HotKeyDirection == HotKeyDirection.Down)
                {
                    if (hotKeyStatus.HotKey == KeySetMode1)
                    {
                        _fovCalculator.ChangeMode(FovCalculatorMode.Capture360);
                    }
                    if (hotKeyStatus.HotKey == KeySetMode2)
                    {
                        _fovCalculator.ChangeMode(FovCalculatorMode.CaptureCustom);
                    }
                    if (hotKeyStatus.HotKey == KeyMoveLeft || hotKeyStatus.HotKey == KeyMoveRight)
                    {
                        var state = _fovCalculator.State;
                        var stateDelta = 0;

                        if (state.Mode == FovCalculatorMode.Capture360)
                        {
                            stateDelta = Math.Abs(state.PointsPer360Deg);
                        }
                        if (state.Mode == FovCalculatorMode.CaptureCustom)
                        {
                            stateDelta = Math.Abs(state.PointsPerCustomDeg);
                        }

                        if (hotKeyStatus.HotKey == KeyMoveLeft)
                        {
                            stateDelta = -stateDelta;
                        }

                        _movementManager.MoveByOffset(stateDelta, 0);
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

            Console.SetCursorPosition(0, 0);
            Console.WriteLine("{0,30}: {1,20}   ", "Mode", state.Mode);

            if (state.Mode == FovCalculatorMode.Capture360 &&
                state.Enabled)
            {
                Console.BackgroundColor = ConsoleColor.Red;
            }

            Console.WriteLine("{0,30}: {1,20}   ", "Points per 360 angle", stateStats.PointsPer360Deg);
            Console.WriteLine("{0,30}: {1,23:F2}", "Points per 1 angle", stateStats.PointsPer1Deg);
            Console.BackgroundColor = ConsoleColor.Black;

            if (state.Mode == FovCalculatorMode.CaptureCustom &&
                state.Enabled)
            {
                Console.BackgroundColor = ConsoleColor.Red;
            }

            Console.WriteLine("{0,30}: {1,20}   ", "Points per custom angle", stateStats.PointsPerCustomDeg);
            Console.WriteLine("{0,30}: {1,23:F2}", "Custom angle", stateStats.CustomDeg);
            Console.BackgroundColor = ConsoleColor.Black;
        }
    }
}
