namespace Nancy.Session.InProc.InProcSessionsManagement.BySessionIdCookie {
  using System;
  using Xunit;

  public class CookieFactoryFixture {
    private readonly string _cookieDomain;
    private readonly CookieFactory _cookieFactory;
    private readonly string _cookieName;
    private readonly string _cookiePath;
    private readonly string _cookieValue;
    private readonly string _cookieValueEncoded;
    private readonly SessionIdentificationData _sessionIdentificationData;

    public CookieFactoryFixture() {
      _cookieFactory = new CookieFactory();

      _cookieName = "TheCookieName";
      _cookieValue = "01HMAC98%02SessionId";
      _cookieValueEncoded = "01HMAC98%2502SessionId";
      _sessionIdentificationData = new SessionIdentificationData {SessionId = "%02SessionId", Hmac = new byte[] {211, 81, 204, 0, 47, 124}};
      _cookieDomain = ".nascar.com";
      _cookiePath = "/schedule/";
    }

    [Fact]
    public void Given_null_cookie_name_then_throws() {
      Assert.Throws<ArgumentNullException>(() => _cookieFactory.CreateCookie(null, _cookieDomain, _cookiePath, _sessionIdentificationData));
    }

    [Fact]
    public void Given_empty_cookie_name_then_throws() {
      Assert.Throws<ArgumentNullException>(() => _cookieFactory.CreateCookie(string.Empty, _cookieDomain, _cookiePath, _sessionIdentificationData));
    }

    [Fact]
    public void Given_whitespace_cookie_name_then_throws() {
      Assert.Throws<ArgumentNullException>(() => _cookieFactory.CreateCookie(" ", _cookieDomain, _cookiePath, _sessionIdentificationData));
    }

    [Fact]
    public void Given_null_cookie_domain_then_does_not_throw() {
      Assert.DoesNotThrow(() => _cookieFactory.CreateCookie(_cookieName, null, _cookiePath, _sessionIdentificationData));
    }

    [Fact]
    public void Given_null_cookie_path_then_does_not_throw() {
      Assert.DoesNotThrow(() => _cookieFactory.CreateCookie(_cookieName, _cookieDomain, null, _sessionIdentificationData));
    }

    [Fact]
    public void Returns_http_only_cookie() {
      var actualCookie = _cookieFactory.CreateCookie(_cookieName, _cookieDomain, _cookiePath, _sessionIdentificationData);
      Assert.True(actualCookie.HttpOnly);
    }

    [Fact]
    public void Returns_cookie_without_path_and_domain_if_none_is_set() {
      var actualCookie = _cookieFactory.CreateCookie(_cookieName, null, null, _sessionIdentificationData);
      Assert.Null(actualCookie.Domain);
      Assert.Null(actualCookie.Path);
    }

    [Fact]
    public void Returns_cookie_with_path_and_domain_if_specified() {
      var actualCookie = _cookieFactory.CreateCookie(_cookieName, _cookieDomain, _cookiePath, _sessionIdentificationData);
      Assert.Equal(_cookieDomain, actualCookie.Domain);
      Assert.Equal(_cookiePath, actualCookie.Path);
    }

    [Fact]
    public void Returns_cookie_with_valid_name() {
      var actualCookie = _cookieFactory.CreateCookie(_cookieName, _cookieDomain, _cookiePath, _sessionIdentificationData);
      Assert.Equal(_cookieName, actualCookie.Name);
    }

    [Fact]
    public void Returns_cookie_with_specified_data() {
      var actualCookie = _cookieFactory.CreateCookie(_cookieName, _cookieDomain, _cookiePath, _sessionIdentificationData);
      Assert.Equal(_cookieValue, actualCookie.Value);
    }

    [Fact]
    public void Returns_cookie_with_specified_encoded_data() {
      var actualCookie = _cookieFactory.CreateCookie(_cookieName, _cookieDomain, _cookiePath, _sessionIdentificationData);
      Assert.Equal(_cookieValueEncoded, actualCookie.EncodedValue);
    }
  }
}