namespace Nancy.Session.InProc.InProcSessionsManagement {
  using System;
  using System.Threading;
  using Cache;
  using PeriodicTasks;

  internal class PeriodicCacheCleaner : IPeriodicCacheCleaner {
    private readonly ICancellationTokenSourceFactory _cancellationTokenSourceFactory;
    private readonly InProcSessionsConfiguration _configuration;
    private readonly IPeriodicTask _periodicTask;
    private CancellationTokenSource _cancellationTokenSource;

    public PeriodicCacheCleaner(InProcSessionsConfiguration configuration, IInProcSessionCache sessionCache, IPeriodicTaskFactory periodicTaskFactory, ICancellationTokenSourceFactory cancellationTokenSourceFactory) {
      if (configuration == null) throw new ArgumentNullException("configuration");
      if (sessionCache == null) throw new ArgumentNullException("sessionCache");
      if (periodicTaskFactory == null) throw new ArgumentNullException("periodicTaskFactory");
      if (cancellationTokenSourceFactory == null) throw new ArgumentNullException("cancellationTokenSourceFactory");
      _configuration = configuration;
      _cancellationTokenSourceFactory = cancellationTokenSourceFactory;
      _periodicTask = periodicTaskFactory.Create(sessionCache.Trim);
    }

    public void Start() {
      _cancellationTokenSource = _cancellationTokenSourceFactory.Create();
      _periodicTask.Start(_configuration.CacheTrimInterval, _cancellationTokenSource.Token);
    }

    public void Stop() {
      if (_cancellationTokenSource != null) {
        _cancellationTokenSource.Cancel();
      }
    }
  }
}