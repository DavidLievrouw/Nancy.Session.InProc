namespace Nancy.Session.InProc.InProcSessionsManagement.Cache {
  using System;
  using System.Collections.Generic;

  internal interface IInProcSessionCache : IEnumerable<InProcSession>, IDisposable {
    int Count { get; }

    InProcSession Get(SessionId id);

    void Set(InProcSession session);

    void Trim();
  }
}