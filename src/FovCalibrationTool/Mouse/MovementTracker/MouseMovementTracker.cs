using Haukcode.HighResolutionTimer;
using Microsoft.Extensions.Options;
using Vortice.DirectInput;

namespace FovCalibrationTool.Mouse.MovementTracker
{
    public class MouseMovementTracker
    {
        private readonly IOptions<MouseMovementTrackerOptions> _optionsAccessor;

        public MouseMovementTracker(IOptions<MouseMovementTrackerOptions> optionsAccessor)
        {
            _optionsAccessor = optionsAccessor;
        }

        public IEnumerable<MouseMoveData> Track(CancellationToken token)
        {
            var options = _optionsAccessor.Value;

            if (options == null ||
                options.PollPeriod == default)
            {
                throw new InvalidOperationException("Configuration is missing from [MouseMovementTrackerOptions]");
            }

            using var directInput = DInput.DirectInput8Create();
            using var directInputDevice = CreateMouseDevice(directInput);

            using var timer = new HighResolutionTimer();

            timer.SetPeriod(options.PollPeriod);
            timer.Start();

            while (true)
            {
                timer.WaitForTrigger();

                if (token.IsCancellationRequested)
                {
                    yield break;
                }

                var dataList = directInputDevice.GetBufferedMouseData();

                var deltaSet = false;
                var deltaX = 0;
                var deltaY = 0;
                var deltaZ = 0;

                foreach (var dataItem in dataList)
                {
                    if (dataItem.Offset == MouseOffset.X)
                    {
                        deltaX += dataItem.Value;
                        deltaSet = true;
                    }
                    if (dataItem.Offset == MouseOffset.Y)
                    {
                        deltaY += dataItem.Value;
                        deltaSet = true;
                    }
                    if (dataItem.Offset == MouseOffset.Z)
                    {
                        deltaZ += dataItem.Value;
                        deltaSet = true;
                    }
                }

                if (deltaSet)
                {
                    yield return new MouseMoveData(deltaX, deltaY, deltaZ);
                }
            }
        }

        private IDirectInputDevice8 CreateMouseDevice(IDirectInput8 directInput)
        {
            if (directInput.IsDeviceAttached(DeviceGuid.SysMouse) == false)
            {
                return null;
            }

            var directInputDevice = directInput.CreateDevice(DeviceGuid.SysMouse);

            if (directInputDevice == null)
            {
                return null;
            }

            var directInputDeviceInitialized = false;

            try
            {
                directInputDevice.Properties.BufferSize = 16;

                var deviceFormatResult = directInputDevice.SetDataFormat<RawMouseState>();

                if (deviceFormatResult.Failure)
                {
                    return null;
                }

                var deviceAcquireResult = directInputDevice.Acquire();

                if (deviceAcquireResult.Failure)
                {
                    return null;
                }

                directInputDeviceInitialized = true;
            }
            finally
            {
                if (directInputDeviceInitialized == false)
                {
                    directInputDevice.Dispose();
                }
            }

            return directInputDevice;
        }
    }
}