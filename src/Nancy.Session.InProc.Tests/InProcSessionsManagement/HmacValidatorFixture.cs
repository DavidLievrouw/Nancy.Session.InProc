namespace Nancy.Session.InProc.InProcSessionsManagement {
  using System;
  using Cryptography;
  using FakeItEasy;
  using Xunit;

  public class HmacValidatorFixture {
    private readonly IHmacProvider _fakeHmacProvider;
    private readonly byte[] _hmac;
    private readonly HmacValidator _hmacValidator;
    private readonly SessionIdentificationData _sessionIdentificationData;

    public HmacValidatorFixture() {
      _fakeHmacProvider = A.Fake<IHmacProvider>();
      _hmacValidator = new HmacValidator(_fakeHmacProvider);
      _hmac = new byte[] {1, 2, 3};
      _sessionIdentificationData = new SessionIdentificationData {SessionId = "TheSessionId", Hmac = _hmac};
    }

    [Fact]
    public void Given_null_hmac_provider_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new HmacValidator(null));
    }

    [Fact]
    public void Given_null_cookie_data_then_throws() {
      Assert.Throws<ArgumentNullException>(() => _hmacValidator.IsValidHmac(null));
    }

    [Fact]
    public void Given_cookie_data_without_hmac_then_returns_false() {
      _sessionIdentificationData.Hmac = null;
      var actual = _hmacValidator.IsValidHmac(_sessionIdentificationData);
      Assert.False(actual);
    }

    [Fact]
    public void Given_hmac_is_invalid_then_returns_false() {
      A.CallTo(() => _fakeHmacProvider.GenerateHmac(_sessionIdentificationData.SessionId)).Returns(new byte[] {7, 8, 9, 10});
      A.CallTo(() => _fakeHmacProvider.HmacLength).Returns(4);
      var actual = _hmacValidator.IsValidHmac(_sessionIdentificationData);
      Assert.False(actual);
    }

    [Fact]
    public void Given_hmac_is_valid_then_returns_true() {
      A.CallTo(() => _fakeHmacProvider.GenerateHmac(_sessionIdentificationData.SessionId)).Returns(_hmac);
      A.CallTo(() => _fakeHmacProvider.HmacLength).Returns(_hmac.Length);
      var actual = _hmacValidator.IsValidHmac(_sessionIdentificationData);
      Assert.True(actual);
    }
  }
}