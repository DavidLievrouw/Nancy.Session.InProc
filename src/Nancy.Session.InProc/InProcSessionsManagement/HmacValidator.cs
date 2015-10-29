namespace Nancy.Session.InProc.InProcSessionsManagement {
  using System;
  using Cryptography;

  internal class HmacValidator : IHmacValidator {
    private readonly IHmacProvider _hmacProvider;

    public HmacValidator(IHmacProvider hmacProvider) {
      if (hmacProvider == null) throw new ArgumentNullException("hmacProvider");
      _hmacProvider = hmacProvider;
    }

    public bool IsValidHmac(SessionIdentificationData sessionIdentificationData) {
      if (sessionIdentificationData == null) throw new ArgumentNullException("sessionIdentificationData");
      if (sessionIdentificationData.Hmac == null) return false;

      var incomingBytes = sessionIdentificationData.Hmac;
      var expectedHmac = _hmacProvider.GenerateHmac(sessionIdentificationData.SessionId);
      return HmacComparer.Compare(expectedHmac, incomingBytes, _hmacProvider.HmacLength);
    }
  }
}