namespace Nancy.Session.InProc.InProcSessionsManagement {
  internal interface IInProcSessionManager {
    void Save(ISession session, NancyContext context);

    ISession Load(NancyContext context);
  }
}