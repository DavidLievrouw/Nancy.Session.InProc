namespace Nancy.Session.InProc.InProcSessionsManagement.PeriodicTasks {
  using System;
  using Xunit;

  public class PeriodicTaskFactoryFixture {
    private readonly PeriodicTaskFactory _periodicTaskFactory;

    public PeriodicTaskFactoryFixture() {
      _periodicTaskFactory = new PeriodicTaskFactory();
    }

    [Fact]
    public void Given_null_action_then_throws() {
      Assert.Throws<ArgumentNullException>(() => _periodicTaskFactory.Create(null));
    }

    [Fact]
    public void Given_valid_action_then_returns_new_periodic_task() {
      Action action = () => Console.WriteLine("ok");
      var actual = _periodicTaskFactory.Create(action);

      Assert.NotNull(actual);
      Assert.IsAssignableFrom<IPeriodicTask>(actual);
    }
  }
}