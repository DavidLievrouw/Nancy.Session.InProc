namespace Nancy.Session.InProc.InProcSessionsManagement {
  using Xunit;

  public class SessionIdentificationDataFixture {
    private readonly string _hmacString;
    private readonly SessionIdentificationData _sessionIdentificationData;

    public SessionIdentificationDataFixture() {
      _sessionIdentificationData = new SessionIdentificationData {SessionId = "TheSessionId", Hmac = new byte[] {211, 81, 204, 0, 47, 124}};
      _hmacString = "01HMAC98";
    }

    [Fact]
    public void ToString_returns_expected_string_representation() {
      var actual = _sessionIdentificationData.ToString();
      var expected = string.Format("{0}{1}", _hmacString, _sessionIdentificationData.SessionId);
      Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToString_returns_value_without_hmac_if_there_is_no_hmac() {
      _sessionIdentificationData.Hmac = null;
      var actual = _sessionIdentificationData.ToString();
      Assert.Equal(_sessionIdentificationData.SessionId, actual);
    }
  }
}