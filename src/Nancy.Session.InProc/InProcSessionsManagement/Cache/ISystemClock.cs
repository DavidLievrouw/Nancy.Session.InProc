namespace Nancy.Session.InProc.InProcSessionsManagement.Cache {
  using System;

  internal interface ISystemClock {
    DateTime NowUtc { get; }
  }
}