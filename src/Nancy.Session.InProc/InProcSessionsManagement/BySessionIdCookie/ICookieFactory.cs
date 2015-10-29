namespace Nancy.Session.InProc.InProcSessionsManagement.BySessionIdCookie {
  using Cookies;

  internal interface ICookieFactory {
    INancyCookie CreateCookie(string cookieName, string cookieDomain, string cookiePath, SessionIdentificationData sessionIdentificationData);
  }
}