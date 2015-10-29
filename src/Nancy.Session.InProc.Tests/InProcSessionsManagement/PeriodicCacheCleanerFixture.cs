namespace Nancy.Session.InProc.InProcSessionsManagement {
  using System;
  using System.Threading;
  using Cache;
  using FakeItEasy;
  using PeriodicTasks;
  using Xunit;

  public class PeriodicCacheCleanerFixture {
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ICancellationTokenSourceFactory _fakeCancellationTokenSourceFactory;
    private readonly IPeriodicTask _fakePeriodicTask;
    private readonly IPeriodicTaskFactory _fakePeriodicTaskFactory;
    private readonly IInProcSessionCache _fakeSessionCache;
    private readonly PeriodicCacheCleaner _periodicCacheCleaner;
    private readonly InProcSessionsConfiguration _validConfiguration;

    public PeriodicCacheCleanerFixture() {
      _fakeSessionCache = A.Fake<IInProcSessionCache>();
      _fakePeriodicTaskFactory = A.Fake<IPeriodicTaskFactory>();
      _fakePeriodicTask = A.Fake<IPeriodicTask>();
      _fakeCancellationTokenSourceFactory = A.Fake<ICancellationTokenSourceFactory>();
      _validConfiguration = new InProcSessionsConfiguration();

      _cancellationTokenSource = new CancellationTokenSource();
      A.CallTo(() => _fakeCancellationTokenSourceFactory.Create()).Returns(_cancellationTokenSource);
      A.CallTo(() => _fakePeriodicTaskFactory.Create(A<Action>._)).Returns(_fakePeriodicTask);

      _periodicCacheCleaner = new PeriodicCacheCleaner(_validConfiguration, _fakeSessionCache, _fakePeriodicTaskFactory, _fakeCancellationTokenSourceFactory);
    }

    [Fact]
    public void Given_null_configuration_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new PeriodicCacheCleaner(null, _fakeSessionCache, _fakePeriodicTaskFactory, _fakeCancellationTokenSourceFactory));
    }

    [Fact]
    public void Given_null_session_cache_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new PeriodicCacheCleaner(_validConfiguration, null, _fakePeriodicTaskFactory, _fakeCancellationTokenSourceFactory));
    }

    [Fact]
    public void Given_null_periodic_task_factory_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new PeriodicCacheCleaner(_validConfiguration, _fakeSessionCache, null, _fakeCancellationTokenSourceFactory));
    }

    [Fact]
    public void Given_null_cancellation_token_source_factory_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new PeriodicCacheCleaner(_validConfiguration, _fakeSessionCache, _fakePeriodicTaskFactory, null));
    }

    [Fact]
    public void On_creation_creates_periodic_task() {
      A.CallTo(() => _fakePeriodicTaskFactory.Create(A<Action>._)).MustHaveHappened();
    }

    [Fact]
    public void On_start_starts_created_periodic_task_with_correct_arguments() {
      _periodicCacheCleaner.Start();

      A.CallTo(() => _fakePeriodicTask.Start(_validConfiguration.CacheTrimInterval, A<CancellationToken>._)).MustHaveHappened();
    }

    [Fact]
    public void On_stop_then_cancels_task() {
      _periodicCacheCleaner.Start();
      _periodicCacheCleaner.Stop();

      Assert.True(_cancellationTokenSource.IsCancellationRequested);
    }

    [Fact]
    public void On_stop_without_start_then_does_not_throw() {
      Assert.DoesNotThrow(() => _periodicCacheCleaner.Stop());
      A.CallTo(() => _fakeCancellationTokenSourceFactory.Create()).MustNotHaveHappened();
    }
  }
}