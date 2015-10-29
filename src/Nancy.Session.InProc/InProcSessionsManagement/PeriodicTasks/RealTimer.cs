namespace Nancy.Session.InProc.InProcSessionsManagement.PeriodicTasks {
  using System;
  using System.Threading;

  internal class RealTimer : ITimer {
    private Timer _innerTimer;
    private Action _timerAction;

    public bool IsStarted() {
      return _innerTimer != null;
    }

    public void StartTimer(Action action, TimeSpan interval) {
      if (action == null) throw new ArgumentNullException("action");
      _timerAction = action;
      _innerTimer = new Timer(TimerCallback, null, interval, interval);
    }

    public void StopTimer() {
      _innerTimer.Change(Timeout.Infinite, Timeout.Infinite);
      _innerTimer = null;
    }

    private void TimerCallback(object state) {
      _timerAction();
    }
  }
}