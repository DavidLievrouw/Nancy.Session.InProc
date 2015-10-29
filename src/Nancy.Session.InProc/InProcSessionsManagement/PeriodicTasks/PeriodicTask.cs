namespace Nancy.Session.InProc.InProcSessionsManagement.PeriodicTasks {
  using System;
  using System.Threading;

  internal class PeriodicTask : IPeriodicTask {
    private readonly Action _action;
    private readonly ITimer _timer;
    private bool _isDisposed;

    public PeriodicTask(Action action, ITimer timer) {
      if (action == null) throw new ArgumentNullException("action");
      if (timer == null) throw new ArgumentNullException("timer");
      _action = action;
      _timer = timer;
      _isDisposed = false;
    }

    public void Start(TimeSpan interval, CancellationToken cancellationToken) {
      CheckDisposed();
      if (interval <= TimeSpan.Zero) throw new ArgumentException("The interval must be greater than zero.", "interval");

      _timer.StartTimer(
        () => {
          if (IsStopRequested(cancellationToken)) {
            _timer.StopTimer();
            return;
          }
          _action();
        },
        interval);
    }

    public void Dispose() {
      _isDisposed = true;
    }

    private void CheckDisposed() {
      if (_isDisposed) {
        throw new ObjectDisposedException(GetType().Name);
      }
    }

    private bool IsStopRequested(CancellationToken cancellationToken) {
      return cancellationToken.IsCancellationRequested || _isDisposed;
    }
  }
}