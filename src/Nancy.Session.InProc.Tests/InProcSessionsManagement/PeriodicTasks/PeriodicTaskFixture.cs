namespace Nancy.Session.InProc.InProcSessionsManagement.PeriodicTasks {
  using System;
  using System.Threading;
  using Xunit;

  public class PeriodicTaskFixture {
    private readonly PeriodicTask _periodicTask;
    private readonly TimerForUnitTests _timer;
    private int _numberOfExecutions;

    public PeriodicTaskFixture() {
      _numberOfExecutions = 0;
      _timer = new TimerForUnitTests();
      _periodicTask = new PeriodicTask(() => _numberOfExecutions++, _timer);
    }

    [Fact]
    public void Given_null_action_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new PeriodicTask(null, _timer));
    }

    [Fact]
    public void Given_null_timer_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new PeriodicTask(() => _numberOfExecutions++, null));
    }

    [Fact]
    public void Given_zero_interval_time_then_throws() {
      using (var tokenSource = new CancellationTokenSource()) {
        Assert.Throws<ArgumentException>(() => _periodicTask.Start(TimeSpan.Zero, tokenSource.Token));
      }
    }

    [Fact]
    public void Given_negative_interval_time_then_throws() {
      using (var tokenSource = new CancellationTokenSource()) {
        Assert.Throws<ArgumentException>(() => _periodicTask.Start(TimeSpan.FromSeconds(-1), tokenSource.Token));
      }
    }

    [Fact]
    public void When_cancelled_before_first_execution_then_does_not_execute() {
      using (var tokenSource = new CancellationTokenSource()) {
        _periodicTask.Start(TimeSpan.FromMilliseconds(1000), tokenSource.Token);
        _timer.ElapseSeconds(0.1);
        tokenSource.Cancel();
        Assert.Equal(0, _numberOfExecutions);
      }
    }

    [Fact]
    public void Adheres_interval() {
      using (var tokenSource = new CancellationTokenSource()) {
        _periodicTask.Start(TimeSpan.FromMilliseconds(1000), tokenSource.Token);
        _timer.ElapseSeconds(2.5);
        tokenSource.Cancel();
        Assert.Equal(2, _numberOfExecutions);
      }
    }

    [Fact]
    public void When_disposed_then_stops() {
      using (var tokenSource = new CancellationTokenSource()) {
        _periodicTask.Start(TimeSpan.FromMilliseconds(1000), tokenSource.Token);
        _timer.ElapseSeconds(4.5);
        _periodicTask.Dispose();
        _timer.ElapseSeconds(2);
        Assert.Equal(4, _numberOfExecutions);
      }
    }
  }
}