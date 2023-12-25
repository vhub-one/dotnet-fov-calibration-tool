
namespace FovCalibrationTool.Mvvm
{
    public record ViewModelStateEventArgs<TState>(TState State, TState StatePrevious);
}
