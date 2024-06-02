namespace FovCalibrationTool.CalibrationTool
{
    public class SyncDispatcher
    {
        private CancellationTokenSource _taskTokenSource;
        private Task _task;

        private SemaphoreSlim _taskSemaphore;

        public SyncDispatcher()
        {
            _taskSemaphore = new(1);
        }

        public async Task ExecuteAsync(Func<CancellationToken, Task> taskFactory, bool replaceTask = false)
        {
            await _taskSemaphore.WaitAsync();

            try
            {
                if (replaceTask)
                {
                    if (_task != null)
                    {
                        try
                        {
                            _taskTokenSource.Cancel();

                            await _task.WaitAsync(
                                Timeout.InfiniteTimeSpan
                            );
                        }
                        catch (OperationCanceledException)
                        {
                            // Running task has been cancelled
                        }
                        finally
                        {
                            _taskTokenSource.Dispose();
                            _taskTokenSource = null;

                            _task = null;
                        }
                    }
                }

                if (_task != null)
                {
                    if (_task.IsCompleted == false)
                    {
                        // Task is still running
                        return;
                    }

                    _taskTokenSource.Dispose();
                }

                _taskTokenSource = new CancellationTokenSource();
                _task = taskFactory(
                    _taskTokenSource.Token
                );
            }
            finally
            {
                _taskSemaphore.Release();
            }
        }
    }
}
