namespace Nancy.Session.InProc.InProcSessionsManagement.BySessionIdCookie {
  internal interface IBySessionIdCookieIdentificationMethod : IInProcSessionIdentificationMethod {
    string CookieName { get; set; }

    string Domain { get; set; }

    string Path { get; set; }
  }
}