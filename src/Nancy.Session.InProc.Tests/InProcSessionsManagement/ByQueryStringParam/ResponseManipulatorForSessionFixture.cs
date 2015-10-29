namespace Nancy.Session.InProc.InProcSessionsManagement.ByQueryStringParam {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Xunit;

  public class ResponseManipulatorForSessionFixture {
    private readonly NancyContext _context;
    private readonly string _parameterName;
    private readonly ResponseManipulatorForSession _responseManipulatorForSession;
    private readonly SessionIdentificationData _sessionIdentificationData;

    public ResponseManipulatorForSessionFixture() {
      _responseManipulatorForSession = new ResponseManipulatorForSession();

      _context = new NancyContext {Response = new Response(), Request = new Request("GET", "http://www.google.be")};
      _sessionIdentificationData = new SessionIdentificationData {SessionId = "01SessionId", Hmac = new byte[] {211, 81, 204, 0, 47, 124}};
      _parameterName = "SID";
    }

    [Fact]
    public void Given_null_context_then_throws() {
      Assert.Throws<ArgumentNullException>(() => _responseManipulatorForSession.ModifyResponseToRedirectToSessionAwareUrl(null, _sessionIdentificationData, _parameterName));
    }

    [Fact]
    public void Given_null_session_identification_data_then_throws() {
      Assert.Throws<ArgumentNullException>(() => _responseManipulatorForSession.ModifyResponseToRedirectToSessionAwareUrl(_context, null, _parameterName));
    }

    [Fact]
    public void Given_null_parameter_name_then_throws() {
      Assert.Throws<ArgumentNullException>(() => _responseManipulatorForSession.ModifyResponseToRedirectToSessionAwareUrl(_context, _sessionIdentificationData, null));
    }

    [Fact]
    public void Given_empty_parameter_name_then_throws() {
      Assert.Throws<ArgumentNullException>(() => _responseManipulatorForSession.ModifyResponseToRedirectToSessionAwareUrl(_context, _sessionIdentificationData, string.Empty));
    }

    [Fact]
    public void Given_whitespace_parameter_name_then_throws() {
      Assert.Throws<ArgumentNullException>(() => _responseManipulatorForSession.ModifyResponseToRedirectToSessionAwareUrl(_context, _sessionIdentificationData, " "));
    }

    [Fact]
    public void Given_context_without_request_then_throws() {
      var contextWithoutRequest = new NancyContext {Response = new Response()};
      Assert.Throws<ArgumentException>(() => _responseManipulatorForSession.ModifyResponseToRedirectToSessionAwareUrl(contextWithoutRequest, _sessionIdentificationData, _parameterName));
    }

    [Fact]
    public void Given_context_without_response_then_throws() {
      var contextWithoutResponse = new NancyContext {Request = new Request("GET", "http://www.google.be")};
      Assert.Throws<ArgumentException>(() => _responseManipulatorForSession.ModifyResponseToRedirectToSessionAwareUrl(contextWithoutResponse, _sessionIdentificationData, _parameterName));
    }

    [Fact]
    public void Changes_http_status_code_of_reponse_to_302() {
      _responseManipulatorForSession.ModifyResponseToRedirectToSessionAwareUrl(_context, _sessionIdentificationData, _parameterName);
      Assert.Equal(_context.Response.StatusCode, HttpStatusCode.Found);
    }

    [Fact]
    public void Given_request_without_query_then_creates_query_for_location_header() {
      var expectedLocationHeaderValue = _context.Request.Url + "?" + _parameterName + "=" + _sessionIdentificationData;

      _responseManipulatorForSession.ModifyResponseToRedirectToSessionAwareUrl(_context, _sessionIdentificationData, _parameterName);

      var locationHeader = _context.Response.Headers.FirstOrDefault(header => header.Key.Equals("Location", StringComparison.OrdinalIgnoreCase));
      Assert.NotNull(locationHeader);
      Assert.True(new Uri(expectedLocationHeaderValue).Equals(new Uri(locationHeader.Value)));
    }

    [Fact]
    public void Given_request_with_query_then_creates_query_for_location_header() {
      _context.Request = new Request("GET", "http://www.google.be?value=test&process=3");
      var expectedLocationHeaderValue = _context.Request.Url + "&" + _parameterName + "=" + _sessionIdentificationData;

      _responseManipulatorForSession.ModifyResponseToRedirectToSessionAwareUrl(_context, _sessionIdentificationData, _parameterName);

      var locationHeader = _context.Response.Headers.FirstOrDefault(header => header.Key.Equals("Location", StringComparison.OrdinalIgnoreCase));
      Assert.NotNull(locationHeader);
      Assert.True(new Uri(expectedLocationHeaderValue).Equals(new Uri(locationHeader.Value)));
    }

    [Fact]
    public void Given_request_already_has_a_session_id_then_replaces_that_session_id() {
      _context.Request = new Request("GET", "http://www.google.be:624?value=test&process=3&" + _parameterName + "=ABC123&page=14");
      var expectedLocationHeaderValue = "http://www.google.be:624?value=test&process=3" + "&" + _parameterName + "=" + _sessionIdentificationData + "&page=14";

      _responseManipulatorForSession.ModifyResponseToRedirectToSessionAwareUrl(_context, _sessionIdentificationData, _parameterName);

      var locationHeader = _context.Response.Headers.FirstOrDefault(header => header.Key.Equals("Location", StringComparison.OrdinalIgnoreCase));
      Assert.NotNull(locationHeader);
      Assert.True(new Uri(expectedLocationHeaderValue).Equals(new Uri(locationHeader.Value)));
    }

    [Fact]
    public void Given_request_already_has_a_session_id_with_encoded_characters_then_replaces_that_session_id() {
      _context.Request = new Request("GET", "http://www.google.be:624?value=test&process=3&" + _parameterName + "=ABC%C2%2F23&page=14");
      var expectedLocationHeaderValue = "http://www.google.be:624?value=test&process=3" + "&" + _parameterName + "=" + _sessionIdentificationData + "&page=14";

      _responseManipulatorForSession.ModifyResponseToRedirectToSessionAwareUrl(_context, _sessionIdentificationData, _parameterName);

      var locationHeader = _context.Response.Headers.FirstOrDefault(header => header.Key.Equals("Location", StringComparison.OrdinalIgnoreCase));
      Assert.NotNull(locationHeader);
      Assert.True(new Uri(expectedLocationHeaderValue).Equals(new Uri(locationHeader.Value)));
    }

    [Fact]
    public void Given_session_identifier_is_lowercase_url_encoded() {
      _sessionIdentificationData.SessionId = "/bOu§¨";
      const string encodedSessionId = "%2fbOu%c2%a7%c2%a8";
      const string expectedParameterValue = "01HMAC98" + encodedSessionId;
      var expectedLocationHeaderValue = _context.Request.Url + "/" + "?" + _parameterName + "=" + expectedParameterValue;

      _responseManipulatorForSession.ModifyResponseToRedirectToSessionAwareUrl(_context, _sessionIdentificationData, _parameterName);

      var locationHeader = _context.Response.Headers.FirstOrDefault(header => header.Key.Equals("Location", StringComparison.OrdinalIgnoreCase));
      Assert.NotNull(locationHeader);
      Assert.True(new Uri(expectedLocationHeaderValue).Equals(new Uri(locationHeader.Value)));
    }

    [Fact]
    public void Given_response_already_has_a_location_header_then_replaces_header_value() {
      var headers = new Dictionary<string, IEnumerable<string>>();
      headers.Add("location", new[] {"http://www.github.com/nancyfx"});
      _context.Request = new Request(_context.Request.Method, _context.Request.Url, null, headers);

      var expectedLocationHeaderValue = _context.Request.Url + "?" + _parameterName + "=" + _sessionIdentificationData;

      _responseManipulatorForSession.ModifyResponseToRedirectToSessionAwareUrl(_context, _sessionIdentificationData, _parameterName);

      var locationHeader = _context.Response.Headers.FirstOrDefault(header => header.Key.Equals("Location", StringComparison.OrdinalIgnoreCase));
      Assert.NotNull(locationHeader);
      Assert.True(new Uri(expectedLocationHeaderValue).Equals(new Uri(locationHeader.Value)));
    }
  }
}