namespace Nancy.Session.InProc.InProcSessionsManagement {
  internal interface IInProcSessionFactory {
    InProcSession Create(SessionId sessionId, ISession wrappedSession);
  }
}