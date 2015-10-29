namespace Nancy.Session.InProc.InProcSessionsManagement.ByQueryStringParam {
  using System;
  using Cryptography;
  using FakeItEasy;
  using Xunit;

  public class ByQueryStringParamIdentificationMethodFixture {
    private readonly ByQueryStringParamIdentificationMethod _byQueryStringParamIdentificationMethod;
    private readonly IEncryptionProvider _fakeEncryptionProvider;
    private readonly IHmacProvider _fakeHmacProvider;
    private readonly IHmacValidator _fakeHmacValidator;
    private readonly IResponseManipulatorForSession _fakeResponseManipulatorForSession;
    private readonly ISessionIdentificationDataProvider _fakeSessionIdentificationDataProvider;
    private readonly ISessionIdFactory _fakeSessionIdFactory;
    private readonly string _parameterName;
    private readonly InProcSessionsConfiguration _validConfiguration;

    public ByQueryStringParamIdentificationMethodFixture() {
      _fakeEncryptionProvider = A.Fake<IEncryptionProvider>();
      _fakeHmacProvider = A.Fake<IHmacProvider>();
      _validConfiguration = new InProcSessionsConfiguration();
      _fakeSessionIdentificationDataProvider = A.Fake<ISessionIdentificationDataProvider>();
      _fakeHmacValidator = A.Fake<IHmacValidator>();
      _fakeSessionIdFactory = A.Fake<ISessionIdFactory>();
      _fakeResponseManipulatorForSession = A.Fake<IResponseManipulatorForSession>();
      _byQueryStringParamIdentificationMethod = new ByQueryStringParamIdentificationMethod(
        _fakeEncryptionProvider,
        _fakeHmacProvider,
        _fakeSessionIdentificationDataProvider,
        _fakeHmacValidator,
        _fakeSessionIdFactory,
        _fakeResponseManipulatorForSession);
      _parameterName = "TheNameOfTheParameter";
      _byQueryStringParamIdentificationMethod.ParameterName = _parameterName;
    }

    [Fact]
    public void Given_null_crypto_configuration_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new ByQueryStringParamIdentificationMethod(null));
    }

    [Fact]
    public void Given_null_encryption_provider_then_throws() {
      Assert.Throws<ArgumentNullException>(
        () => new ByQueryStringParamIdentificationMethod(null, _fakeHmacProvider, _fakeSessionIdentificationDataProvider, _fakeHmacValidator, _fakeSessionIdFactory, _fakeResponseManipulatorForSession));
    }

    [Fact]
    public void Given_null_hmac_provider_then_throws() {
      Assert.Throws<ArgumentNullException>(
        () => new ByQueryStringParamIdentificationMethod(_fakeEncryptionProvider, null, _fakeSessionIdentificationDataProvider, _fakeHmacValidator, _fakeSessionIdFactory, _fakeResponseManipulatorForSession));
    }

    [Fact]
    public void Given_null_session_data_provider_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new ByQueryStringParamIdentificationMethod(_fakeEncryptionProvider, _fakeHmacProvider, null, _fakeHmacValidator, _fakeSessionIdFactory, _fakeResponseManipulatorForSession));
    }

    [Fact]
    public void Given_null_hmac_validator_then_throws() {
      Assert.Throws<ArgumentNullException>(
        () => new ByQueryStringParamIdentificationMethod(_fakeEncryptionProvider, _fakeHmacProvider, _fakeSessionIdentificationDataProvider, null, _fakeSessionIdFactory, _fakeResponseManipulatorForSession));
    }

    [Fact]
    public void Given_null_session_id_factory_then_throws() {
      Assert.Throws<ArgumentNullException>(
        () => new ByQueryStringParamIdentificationMethod(_fakeEncryptionProvider, _fakeHmacProvider, _fakeSessionIdentificationDataProvider, _fakeHmacValidator, null, _fakeResponseManipulatorForSession));
    }

    [Fact]
    public void Given_null_response_manipulator_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new ByQueryStringParamIdentificationMethod(_fakeEncryptionProvider, _fakeHmacProvider, _fakeSessionIdentificationDataProvider, _fakeHmacValidator, _fakeSessionIdFactory, null));
    }

    [Fact]
    public void On_creation_sets_default_parameter_name() {
      var newInstance = new ByQueryStringParamIdentificationMethod(_fakeEncryptionProvider, _fakeHmacProvider, _fakeSessionIdentificationDataProvider, _fakeHmacValidator, _fakeSessionIdFactory, _fakeResponseManipulatorForSession);
      Assert.Equal("_nsid", newInstance.ParameterName);
    }

    public class Load : ByQueryStringParamIdentificationMethodFixture {
      private readonly NancyContext _context;
      private readonly SessionId _newSessionId;

      public Load() {
        _context = new NancyContext {Request = new Request("GET", "http://www.google.be?_nsid=01HMAC02SessionId")};

        _newSessionId = new SessionId(Guid.NewGuid(), false);
        A.CallTo(() => _fakeSessionIdFactory.CreateNew()).Returns(_newSessionId);
      }

      [Fact]
      public void Given_null_context_then_throws() {
        Assert.Throws<ArgumentNullException>(() => _byQueryStringParamIdentificationMethod.GetCurrentSessionId(null));
      }

      [Fact]
      public void When_context_contains_no_session_identification_data_then_returns_new_session_id() {
        A.CallTo(() => _fakeSessionIdentificationDataProvider.ProvideDataFromQuery(_context.Request, _parameterName)).Returns(null);

        var actual = _byQueryStringParamIdentificationMethod.GetCurrentSessionId(_context);

        Assert.Equal(_newSessionId, actual);
        A.CallTo(() => _fakeSessionIdFactory.CreateNew()).MustHaveHappened();
        A.CallTo(() => _fakeSessionIdFactory.CreateFrom(A<string>._)).MustNotHaveHappened();
      }

      [Fact]
      public void When_querystring_does_not_have_a_valid_hmac_then_returns_new_session_id() {
        var sessionIdentificationData = new SessionIdentificationData {SessionId = "ABCSomeEncryptedSessionIdXYZ", Hmac = new byte[] {1, 2, 3}};

        A.CallTo(() => _fakeSessionIdentificationDataProvider.ProvideDataFromQuery(_context.Request, _parameterName)).Returns(sessionIdentificationData);
        A.CallTo(() => _fakeHmacValidator.IsValidHmac(sessionIdentificationData)).Returns(false);

        var actual = _byQueryStringParamIdentificationMethod.GetCurrentSessionId(_context);

        Assert.Equal(_newSessionId, actual);
        A.CallTo(() => _fakeSessionIdFactory.CreateNew()).MustHaveHappened();
        A.CallTo(() => _fakeSessionIdFactory.CreateFrom(A<string>._)).MustNotHaveHappened();
      }

      [Fact]
      public void When_decrypted_session_id_is_not_valid_then_returns_new_session_id() {
        var sessionIdentificationData = new SessionIdentificationData {SessionId = "ABCSomeEncryptedSessionIdXYZ", Hmac = new byte[] {1, 2, 3}};

        A.CallTo(() => _fakeSessionIdentificationDataProvider.ProvideDataFromQuery(_context.Request, _parameterName)).Returns(sessionIdentificationData);
        A.CallTo(() => _fakeHmacValidator.IsValidHmac(sessionIdentificationData)).Returns(true);
        A.CallTo(() => _fakeEncryptionProvider.Decrypt(sessionIdentificationData.SessionId)).Returns(string.Empty);

        var actual = _byQueryStringParamIdentificationMethod.GetCurrentSessionId(_context);

        Assert.Equal(_newSessionId, actual);
        A.CallTo(() => _fakeSessionIdFactory.CreateNew()).MustHaveHappened();
        A.CallTo(() => _fakeSessionIdFactory.CreateFrom(A<string>._)).MustNotHaveHappened();
      }

      [Fact]
      public void When_decrypted_session_id_is_not_a_valid_guid_then_returns_new_session_id() {
        const string invalidDecryptedSessionId = "This is not a valid guid!";
        var sessionIdentificationData = new SessionIdentificationData {SessionId = "ABCSomeEncryptedSessionIdXYZ", Hmac = new byte[] {1, 2, 3}};

        A.CallTo(() => _fakeSessionIdentificationDataProvider.ProvideDataFromQuery(_context.Request, _parameterName)).Returns(sessionIdentificationData);
        A.CallTo(() => _fakeHmacValidator.IsValidHmac(sessionIdentificationData)).Returns(true);
        A.CallTo(() => _fakeEncryptionProvider.Decrypt(sessionIdentificationData.SessionId)).Returns(invalidDecryptedSessionId);
        A.CallTo(() => _fakeSessionIdFactory.CreateFrom(invalidDecryptedSessionId)).Returns(null);

        var actual = _byQueryStringParamIdentificationMethod.GetCurrentSessionId(_context);

        Assert.Equal(_newSessionId, actual);
        A.CallTo(() => _fakeSessionIdFactory.CreateNew()).MustHaveHappened();
        A.CallTo(() => _fakeSessionIdFactory.CreateFrom(invalidDecryptedSessionId)).MustHaveHappened();
      }

      [Fact]
      public void When_decrypted_session_id_is_valid_then_returns_session_id_from_querystring() {
        var expectedSessionId = new SessionId(Guid.NewGuid(), false);
        var decryptedSessionId = expectedSessionId.Value.ToString();
        var sessionIdentificationData = new SessionIdentificationData {SessionId = "ABCSomeEncryptedSessionIdXYZ", Hmac = new byte[] {1, 2, 3}};

        A.CallTo(() => _fakeSessionIdentificationDataProvider.ProvideDataFromQuery(_context.Request, _parameterName)).Returns(sessionIdentificationData);
        A.CallTo(() => _fakeHmacValidator.IsValidHmac(sessionIdentificationData)).Returns(true);
        A.CallTo(() => _fakeEncryptionProvider.Decrypt(sessionIdentificationData.SessionId)).Returns(decryptedSessionId);
        A.CallTo(() => _fakeSessionIdFactory.CreateFrom(decryptedSessionId)).Returns(expectedSessionId);

        var actual = _byQueryStringParamIdentificationMethod.GetCurrentSessionId(_context);

        Assert.Equal(expectedSessionId, actual);
        A.CallTo(() => _fakeSessionIdFactory.CreateNew()).MustNotHaveHappened();
        A.CallTo(() => _fakeSessionIdFactory.CreateFrom(decryptedSessionId)).MustHaveHappened();
      }
    }

    public class Save : ByQueryStringParamIdentificationMethodFixture {
      private readonly NancyContext _context;
      private readonly SessionId _validSessionId;

      public Save() {
        _validSessionId = new SessionId(Guid.NewGuid(), true);
        _context = new NancyContext {Request = new Request("GET", "http://www.google.be")};
      }

      [Fact]
      public void Given_null_context_then_throws() {
        Assert.Throws<ArgumentNullException>(() => _byQueryStringParamIdentificationMethod.SaveSessionId(_validSessionId, null));
      }

      [Fact]
      public void Given_null_session_id_then_throws() {
        Assert.Throws<ArgumentNullException>(() => _byQueryStringParamIdentificationMethod.SaveSessionId(null, _context));
      }

      [Fact]
      public void Given_context_without_request_then_throws() {
        var contextWithoutRequest = new NancyContext();
        Assert.Throws<ArgumentException>(() => _byQueryStringParamIdentificationMethod.SaveSessionId(_validSessionId, contextWithoutRequest));
      }

      [Fact]
      public void Given_empty_session_id_then_throws() {
        var emptySessionId = new SessionId(Guid.Empty, false);
        Assert.Throws<ArgumentException>(() => _byQueryStringParamIdentificationMethod.SaveSessionId(emptySessionId, _context));
      }

      [Fact]
      public void Given_session_id_is_not_new_then_does_nothing() {
        var existingSessionId = new SessionId(Guid.NewGuid(), false);
        _byQueryStringParamIdentificationMethod.SaveSessionId(existingSessionId, _context);
        A.CallTo(() => _fakeResponseManipulatorForSession.ModifyResponseToRedirectToSessionAwareUrl(A<NancyContext>._, A<SessionIdentificationData>._, A<string>._)).MustNotHaveHappened();
      }

      [Fact]
      public void Given_session_id_is_new_then_manipulates_response() {
        const string encryptedSessionId = "ABC_sessionid_xyz";
        var hmacBytes = new byte[] {1, 2, 3};

        A.CallTo(() => _fakeEncryptionProvider.Encrypt(_validSessionId.Value.ToString())).Returns(encryptedSessionId);
        A.CallTo(() => _fakeHmacProvider.GenerateHmac(encryptedSessionId)).Returns(hmacBytes);
        A.CallTo(() => _fakeHmacProvider.HmacLength).Returns(hmacBytes.Length);

        _byQueryStringParamIdentificationMethod.SaveSessionId(_validSessionId, _context);

        A.CallTo(
          () =>
            _fakeResponseManipulatorForSession.ModifyResponseToRedirectToSessionAwareUrl(
              _context,
              A<SessionIdentificationData>.That.Matches(sid => sid.SessionId == encryptedSessionId && HmacComparer.Compare(sid.Hmac, hmacBytes, _fakeHmacProvider.HmacLength)),
              _parameterName)).MustHaveHappened();
      }
    }
  }
}