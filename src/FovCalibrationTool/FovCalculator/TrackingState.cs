
namespace FovCalibrationTool.FovCalculator
{
    public record TrackingState(bool Active, TrackingMode Mode)
    {
        public static readonly TrackingState Default = new(false, TrackingMode.Disabled);
    }
}
