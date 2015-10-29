namespace Nancy.Session.InProc.InProcSessionsManagement.PeriodicTasks {
  using System;
  using System.Threading;

  internal interface IPeriodicTask : IDisposable {
    void Start(TimeSpan interval, CancellationToken cancellationToken);
  }
}