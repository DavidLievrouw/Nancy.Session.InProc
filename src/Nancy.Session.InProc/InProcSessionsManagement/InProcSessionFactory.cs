namespace Nancy.Session.InProc.InProcSessionsManagement {
  using System;
  using Cache;

  internal class InProcSessionFactory : IInProcSessionFactory {
    private readonly InProcSessionsConfiguration _configuration;
    private readonly ISystemClock _systemClock;

    public InProcSessionFactory(InProcSessionsConfiguration configuration, ISystemClock systemClock) {
      if (configuration == null) throw new ArgumentNullException("configuration");
      if (systemClock == null) throw new ArgumentNullException("systemClock");
      _configuration = configuration;
      _systemClock = systemClock;
    }

    public InProcSession Create(SessionId sessionId, ISession wrappedSession) {
      if (sessionId == null) throw new ArgumentNullException("sessionId");
      if (sessionId.IsEmpty) throw new ArgumentException("The session id cannot be empty", "sessionId");
      if (wrappedSession == null) throw new ArgumentNullException("wrappedSession");

      return new InProcSession(sessionId, wrappedSession, _systemClock.NowUtc, _configuration.SessionTimeout);
    }
  }
}