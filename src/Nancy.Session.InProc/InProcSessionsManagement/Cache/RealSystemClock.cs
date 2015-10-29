namespace Nancy.Session.InProc.InProcSessionsManagement.Cache {
  using System;

  internal class RealSystemClock : ISystemClock {
    public DateTime NowUtc {
      get { return DateTime.UtcNow; }
    }
  }
}