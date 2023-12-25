using Common.Objects;

namespace FovCalibrationTool.Mvvm
{
    public class ViewModel<TState>
        where TState : class
    {
        private TState _state;

        public event EventHandler<ViewModelStateEventArgs<TState>> StateChanged;

        public TState State
        {
            get { return _state; }
        }

        protected void UpdateState(TState state)
        {
            var statePrevious = ObjectUtils.Swap(ref _state, state);

            if (statePrevious == state)
            {
                return;
            }

            var stateChangedCallback = StateChanged;

            if (stateChangedCallback != null)
            {
                stateChangedCallback(this, new ViewModelStateEventArgs<TState>(state, statePrevious));
            }
        }

        public virtual void RestoreState(TState state)
        {
            UpdateState(state);
        }
    }
}
