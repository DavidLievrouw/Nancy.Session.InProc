namespace Nancy.Session.InProc.InProcSessionsManagement.BySessionIdCookie {
  using System;
  using Cryptography;
  using FakeItEasy;
  using Xunit;

  public class SessionIdentificationDataProviderFixture {
    private readonly string _cookieName;
    private readonly string _encryptedSessionIdString;

    private readonly SessionIdentificationData _expectedResult;
    private readonly IHmacProvider _hmacProvider;
    private readonly string _hmacString;
    private readonly SessionIdentificationDataProvider _sessionIdentificationDataProvider;
    private readonly Request _validRequest;

    public SessionIdentificationDataProviderFixture() {
      _cookieName = "TheCookieName";
      _hmacProvider = A.Fake<IHmacProvider>();
      _sessionIdentificationDataProvider = new SessionIdentificationDataProvider(_hmacProvider);

      _validRequest = new Request("GET", "http://www.google.be");
      _hmacString = "01HMAC98";
      _encryptedSessionIdString = "%02Session+Id";
      _validRequest.Cookies.Add(_cookieName, _hmacString + _encryptedSessionIdString);

      _expectedResult = new SessionIdentificationData {SessionId = "%02Session+Id", Hmac = new byte[] {211, 81, 204, 0, 47, 124}};

      A.CallTo(() => _hmacProvider.HmacLength).Returns(6);
    }

    [Fact]
    public void Given_null_hmac_provider_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new SessionIdentificationDataProvider(null));
    }

    [Fact]
    public void Given_null_request_then_throws() {
      Assert.Throws<ArgumentNullException>(() => _sessionIdentificationDataProvider.ProvideDataFromCookie(null, _cookieName));
    }

    [Fact]
    public void Given_null_cookie_name_then_throws() {
      Assert.Throws<ArgumentNullException>(() => _sessionIdentificationDataProvider.ProvideDataFromCookie(_validRequest, null));
    }

    [Fact]
    public void Given_empty_cookie_name_then_throws() {
      Assert.Throws<ArgumentNullException>(() => _sessionIdentificationDataProvider.ProvideDataFromCookie(_validRequest, string.Empty));
    }

    [Fact]
    public void Given_whitespace_cookie_name_then_throws() {
      Assert.Throws<ArgumentNullException>(() => _sessionIdentificationDataProvider.ProvideDataFromCookie(_validRequest, " "));
    }

    [Fact]
    public void Given_request_without_cookie_then_returns_null() {
      _validRequest.Cookies.Clear();
      var actual = _sessionIdentificationDataProvider.ProvideDataFromCookie(_validRequest, _cookieName);
      Assert.Null(actual);
    }

    [Fact]
    public void Given_cookie_data_is_completele_nonsense_then_returns_null() {
      SetCookieValue("BS");
      var actual = _sessionIdentificationDataProvider.ProvideDataFromCookie(_validRequest, _cookieName);
      Assert.Null(actual);
    }

    [Fact]
    public void Given_cookie_hmac_is_invalid_base64_string_then_returns_null() {
      SetCookieValue("A" + _encryptedSessionIdString);
      var actual = _sessionIdentificationDataProvider.ProvideDataFromCookie(_validRequest, _cookieName);
      Assert.Null(actual);
    }

    [Fact]
    public void Given_valid_cookie_then_returns_expected_result() {
      var actual = _sessionIdentificationDataProvider.ProvideDataFromCookie(_validRequest, _cookieName);
      Assert.Equal(_expectedResult.SessionId, actual.SessionId);
      Assert.True(HmacComparer.Compare(actual.Hmac, _expectedResult.Hmac, _hmacProvider.HmacLength));
    }

    private void SetCookieValue(string newValue) {
      _validRequest.Cookies.Clear();
      _validRequest.Cookies.Add(_cookieName, newValue);
    }
  }
}