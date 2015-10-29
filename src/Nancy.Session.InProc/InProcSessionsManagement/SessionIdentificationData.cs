namespace Nancy.Session.InProc.InProcSessionsManagement {
  using System;

  internal class SessionIdentificationData {
    public string SessionId { get; set; }

    public byte[] Hmac { get; set; }

    public override string ToString() {
      var base64Hmac = string.Empty;
      if (Hmac != null) base64Hmac = Convert.ToBase64String(Hmac);
      return string.Format("{0}{1}", base64Hmac, SessionId);
    }
  }
}