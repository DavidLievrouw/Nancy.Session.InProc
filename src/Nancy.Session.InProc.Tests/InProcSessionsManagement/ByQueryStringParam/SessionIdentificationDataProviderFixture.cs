namespace Nancy.Session.InProc.InProcSessionsManagement.ByQueryStringParam {
  using System;
  using System.Collections.Generic;
  using Cryptography;
  using FakeItEasy;
  using Xunit;

  public class SessionIdentificationDataProviderFixture {
    private readonly string _encryptedSessionIdString;
    private readonly SessionIdentificationData _expectedResult;
    private readonly IHmacProvider _hmacProvider;
    private readonly string _hmacString;
    private readonly string _parameterName;
    private readonly SessionIdentificationDataProvider _sessionIdentificationDataProvider;
    private readonly Request _validRequest;

    public SessionIdentificationDataProviderFixture() {
      _parameterName = "TheParamName";
      _hmacProvider = A.Fake<IHmacProvider>();
      _sessionIdentificationDataProvider = new SessionIdentificationDataProvider(_hmacProvider);

      _hmacString = "01HMAC98";
      _encryptedSessionIdString = "s%26%c2%a7%c2%a7ionId";
      _validRequest = new Request("GET", string.Format("http://www.google.be?{0}={1}{2}", _parameterName, _hmacString, _encryptedSessionIdString));

      _expectedResult = new SessionIdentificationData {SessionId = "s&§§ionId", Hmac = new byte[] {211, 81, 204, 0, 47, 124}};

      A.CallTo(() => _hmacProvider.HmacLength).Returns(6);
    }

    [Fact]
    public void Given_null_hmac_provider_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new SessionIdentificationDataProvider(null));
    }

    [Fact]
    public void Given_null_request_then_throws() {
      Assert.Throws<ArgumentNullException>(() => _sessionIdentificationDataProvider.ProvideDataFromQuery(null, _parameterName));
    }

    [Fact]
    public void Given_null_parameter_name_then_throws() {
      Assert.Throws<ArgumentNullException>(() => _sessionIdentificationDataProvider.ProvideDataFromQuery(_validRequest, null));
    }

    [Fact]
    public void Given_empty_parameter_name_then_throws() {
      Assert.Throws<ArgumentNullException>(() => _sessionIdentificationDataProvider.ProvideDataFromQuery(_validRequest, string.Empty));
    }

    [Fact]
    public void Given_whitespace_parameter_name_then_throws() {
      Assert.Throws<ArgumentNullException>(() => _sessionIdentificationDataProvider.ProvideDataFromQuery(_validRequest, " "));
    }

    [Fact]
    public void Given_request_without_session_parameter_then_returns_null() {
      var requestWithoutSessionParameter = new Request("GET", "http://test/api");
      var actual = _sessionIdentificationDataProvider.ProvideDataFromQuery(requestWithoutSessionParameter, _parameterName);
      Assert.Null(actual);
    }

    [Fact]
    public void Given_session_data_is_completele_nonsense_then_returns_null() {
      SetParameterValue("BS");
      var actual = _sessionIdentificationDataProvider.ProvideDataFromQuery(_validRequest, _parameterName);
      Assert.Null(actual);
    }

    [Fact]
    public void Given_session_hmac_is_invalid_base64_string_then_returns_null() {
      SetParameterValue("A" + _encryptedSessionIdString);
      var actual = _sessionIdentificationDataProvider.ProvideDataFromQuery(_validRequest, _parameterName);
      Assert.Null(actual);
    }

    [Fact]
    public void Given_valid_session_parameter_then_returns_expected_result() {
      var actual = _sessionIdentificationDataProvider.ProvideDataFromQuery(_validRequest, _parameterName);
      Assert.Equal(_expectedResult.SessionId, actual.SessionId);
      Assert.True(HmacComparer.Compare(actual.Hmac, _expectedResult.Hmac, _hmacProvider.HmacLength));
    }

    private void SetParameterValue(string newValue) {
      if (newValue == null) {
        ((IDictionary<string, object>) _validRequest.Query).Clear();
      } else {
        ((IDictionary<string, object>) _validRequest.Query)[_parameterName] = newValue;
      }
    }
  }
}