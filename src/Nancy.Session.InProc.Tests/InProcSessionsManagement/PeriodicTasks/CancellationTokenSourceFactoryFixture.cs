namespace Nancy.Session.InProc.InProcSessionsManagement.PeriodicTasks {
  using System.Threading;
  using Xunit;

  public class CancellationTokenSourceFactoryFixture {
    private readonly CancellationTokenSourceFactory _cancellationTokenSourceFactory;

    public CancellationTokenSourceFactoryFixture() {
      _cancellationTokenSourceFactory = new CancellationTokenSourceFactory();
    }

    [Fact]
    public void Create_returns_new_cancellation_token_source() {
      var actual = _cancellationTokenSourceFactory.Create();
      Assert.NotNull(actual);
      Assert.IsAssignableFrom<CancellationTokenSource>(actual);
    }
  }
}