using FovCalibrationTool.FovCalculator;
using FovCalibrationTool.Keyboard.HotKeys;
using FovCalibrationTool.Mouse.MovementManager;
using FovCalibrationTool.Mouse.MovementTracker;
using FovCalibrationTool.Mvvm;
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
        private static readonly HotKey KeySetMode3 = new(Keys.NumPad3);
        private static readonly HotKey KeyMoveLeft = new(Keys.NumPad4);
        private static readonly HotKey KeyUseEstiamte = new(Keys.NumPad5);
        private static readonly HotKey KeyMoveRight = new(Keys.NumPad6);
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
                options.FovDistance,
                options.ViewPortDistance,
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
                KeySetMode3,
                KeyMoveLeft,
                KeyMoveRight,
                KeyUseEstiamte,
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
                    if (hotKey.HasHotKey(KeySetMode3))
                    {
                        _fovCalculator.ChangeMode(FovCalculatorMode.CaptureViewPort);
                    }
                    if (hotKey.HasHotKey(KeyUseEstiamte))
                    {
                        _fovCalculator.UseEstimate();
                    }
                    if (hotKey.HasHotKey(KeyMoveLeft) || hotKey.HasHotKey(KeyMoveRight))
                    {
                        var state = _fovCalculator.State;
                        var stateDelta = FovCalculatorUtils.GetPoints(state);

                        if (hotKey.HasHotKey(KeyMoveLeft))
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

            DrawStateLine("Mode", state.Mode);

            Console.WriteLine();

            DrawStateCaption("#1 SET 360 DISTANCE", state, FovCalculatorMode.Capture360);

            DrawStateLine("points", stateStats.PointsPer360Deg);
            DrawStateLine("estimated points", stateStats.PointsPer360DegEstimate);

            DrawStateFooter();

            Console.WriteLine();

            DrawStateCaption("#2 SET FIELD OF VIEW DISTANCE", state, FovCalculatorMode.CaptureFov);

            DrawStateLine("angle", stateStats.FovDeg);
            DrawStateLine("distance", state.FovDistance);
            DrawStateLine("points", stateStats.PointsPerFovDeg);
            DrawStateLine("estimated points", stateStats.PointsPerFovDegEstimate);

            DrawStateFooter();

            Console.WriteLine();

            DrawStateCaption("#3 TUNE VIEW PORT DISTANCE", state, FovCalculatorMode.CaptureViewPort);

            DrawStateLine("angle", stateStats.ViewPortDeg);
            DrawStateLine("distance", state.ViewPortDistance);
            DrawStateLine("points", stateStats.PointsPerViewPortDeg);
            DrawStateLine("estimated points", stateStats.PointsPerViewPortDegEstimate);

            DrawStateFooter();

            Console.WriteLine();

            DrawStateCaption("# HOT KEYS");

            DrawStateLine("numpad [0]", "disable");
            DrawStateLine("numpad [1]", "set 360 distance");
            DrawStateLine("numpad [2]", "set FOV distance");
            DrawStateLine("numpad [3]", "tune VP distance");
            DrawStateLine("numpad [4]", "move left");
            DrawStateLine("numpad [5]", "use estimates");
            DrawStateLine("numpad [6]", "move right");
            DrawStateLine("[ctrl]", "set distance");
            DrawStateLine("[shift] + [ctrl]", "tune distance");
        }

        private static void DrawStateCaption(string caption, FovCalculatorState state, FovCalculatorMode stateMode)
        {
            if (state.Mode == stateMode)
            {
                if (state.Tracking)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.Green;
                }

                Console.ForegroundColor = ConsoleColor.Black;
            }

            DrawStateCaption(caption);

            if (state.Tracking == false)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        private static void DrawStateCaption(string caption)
        {
            Console.WriteLine(caption.PadRight(55));
        }

        private static void DrawStateFooter()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void DrawStateLine(string caption, double value)
        {
            Console.Write("{0,30}: ", caption);

            if (double.IsFinite(value))
            {
                Console.WriteLine("{0,23:F2}", value);
            }
            else
            {
                Console.WriteLine("{0,23}", $"-.--");
            }
        }

        private static void DrawStateLine<TValue>(string caption, TValue value)
        {
            Console.Write("{0,30}: ", caption);
            Console.WriteLine("{0,23}", value);
        }
    }
}
