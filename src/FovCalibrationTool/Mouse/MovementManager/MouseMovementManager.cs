using GregsStack.InputSimulatorStandard;

namespace FovCalibrationTool.Mouse.MovementManager
{
    public class MouseMovementManager
    {
        private const int DELTA_MAX = 100;

        private readonly InputSimulator _inputSimulator = new();

        public async ValueTask MoveByOffsetAsync(int deltaX, int deltaY, CancellationToken token)
        {
            while (true)
            {
                var offsetX = GetMaxDelta(deltaX);
                var offsetY = GetMaxDelta(deltaY);

                if (offsetX == 0 &&
                    offsetY == 0)
                {
                    break;
                }

                deltaX -= offsetX;
                deltaY -= offsetY;

                _inputSimulator.Mouse.MoveMouseBy(offsetX, offsetY);

                await Task.Delay(50, token);
            }
        }

        private static int GetMaxDelta(int delta)
        {
            if (delta > DELTA_MAX)
            {
                return DELTA_MAX;
            }
            if (delta < -DELTA_MAX)
            {
                return -DELTA_MAX;
            }

            return delta;
        }
    }
}
