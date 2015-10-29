namespace Nancy.Session.InProc.InProcSessionsManagement.PeriodicTasks {
  using System;

  public class TimerForUnitTests : ITimer {
    private TimeSpan _elapsedTime;

    private bool _isStarted;
    private Action _timerAction;
    private TimeSpan _timerInterval;

    public void ElapseSeconds(double seconds) {
      var newElapsedTime = _elapsedTime + TimeSpan.FromSeconds(seconds);
      if (_isStarted) {
        var executionCountBefore = _timerInterval <= TimeSpan.Zero
          ? 0
          : _elapsedTime.Ticks/_timerInterval.Ticks;
        var executionCountAfter = _timerInterval <= TimeSpan.Zero
          ? 0
          : newElapsedTime.Ticks/_timerInterval.Ticks;

        for (var i = 0; i < executionCountAfter - executionCountBefore; i++) {
          _timerAction();
        }
      }
      _elapsedTime = newElapsedTime;
    }

    #region ITimer methods

    public void StartTimer(Action action, TimeSpan interval) {
      _isStarted = true;
      _timerAction = action;
      _timerInterval = interval;
      _elapsedTime = new TimeSpan();
    }

    public bool IsStarted() {
      return _isStarted;
    }

    public void StopTimer() {
      _isStarted = false;
    }

    #endregion
  }
}