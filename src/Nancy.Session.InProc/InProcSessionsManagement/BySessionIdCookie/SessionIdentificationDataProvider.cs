namespace Nancy.Session.InProc.InProcSessionsManagement.BySessionIdCookie {
  using System;
  using Cryptography;

  internal class SessionIdentificationDataProvider : ISessionIdentificationDataProvider {
    private readonly IHmacProvider _hmacProvider;

    public SessionIdentificationDataProvider(IHmacProvider hmacProvider) {
      if (hmacProvider == null) throw new ArgumentNullException("hmacProvider");
      _hmacProvider = hmacProvider;
    }

    public SessionIdentificationData ProvideDataFromCookie(Request request, string cookieName) {
      if (request == null) throw new ArgumentNullException("request");
      if (string.IsNullOrWhiteSpace(cookieName)) throw new ArgumentNullException("cookieName");

      string cookieValue = null;
      if (!request.Cookies.TryGetValue(cookieName, out cookieValue)) {
        return null;
      }
      
      var hmacLength = Base64Helpers.GetBase64Length(_hmacProvider.HmacLength);

      if (cookieValue.Length < hmacLength) {
        // Definitely invalid
        return null;
      }

      var hmacString = cookieValue.Substring(0, hmacLength);
      var encryptedSessionId = cookieValue.Substring(hmacLength);

      byte[] hmacBytes;
      try {
        hmacBytes = Convert.FromBase64String(hmacString);
      } catch (FormatException) {
        // Invalid HMAC
        return null;
      }

      return new SessionIdentificationData {SessionId = encryptedSessionId, Hmac = hmacBytes};
    }
  }
}