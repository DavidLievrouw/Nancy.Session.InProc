namespace Nancy.Session.InProc.InProcSessionsManagement {
  using System;
  using Cache;

  internal class InProcSessionManager : IInProcSessionManager {
    private readonly InProcSessionsConfiguration _configuration;
    private readonly IPeriodicCacheCleaner _periodicCacheCleaner;
    private readonly IInProcSessionCache _sessionCache;
    private readonly IInProcSessionFactory _sessionFactory;

    public InProcSessionManager(InProcSessionsConfiguration configuration, IInProcSessionCache sessionCache, IInProcSessionFactory sessionFactory, IPeriodicCacheCleaner periodicCacheCleaner) {
      if (configuration == null) throw new ArgumentNullException("configuration");
      if (sessionCache == null) throw new ArgumentNullException("sessionCache");
      if (sessionFactory == null) throw new ArgumentNullException("sessionFactory");
      if (periodicCacheCleaner == null) throw new ArgumentNullException("periodicCacheCleaner");
      _configuration = configuration;
      _sessionCache = sessionCache;
      _sessionFactory = sessionFactory;
      _periodicCacheCleaner = periodicCacheCleaner;

      // Start periodic cleaning
      _periodicCacheCleaner.Start();
    }

    public void Save(ISession session, NancyContext context) {
      if (context == null) throw new ArgumentNullException("context");

      if (session == null || !session.HasChanged) return;
      if (session is NullSessionProvider || session.Count <= 0) return;

      var identificationMethod = _configuration.SessionIdentificationMethod;

      var sessionId = identificationMethod.GetCurrentSessionId(context);
      var inProcSession = _sessionFactory.Create(sessionId, session);
      _sessionCache.Set(inProcSession);

      identificationMethod.SaveSessionId(sessionId, context);
    }

    public ISession Load(NancyContext context) {
      if (context == null) throw new ArgumentNullException("context");

      var identificationMethod = _configuration.SessionIdentificationMethod;

      var sessionId = identificationMethod.GetCurrentSessionId(context);
      return (ISession) _sessionCache.Get(sessionId) ?? new Session();
    }
  }
}