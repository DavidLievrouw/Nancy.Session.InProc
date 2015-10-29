namespace Nancy.Session.InProc.InProcSessionsManagement.BySessionIdCookie {
  using System;
  using System.Linq;
  using Cookies;
  using Cryptography;
  using FakeItEasy;
  using Xunit;

  public class BySessionIdCookieIdentificationMethodFixture {
    private readonly BySessionIdCookieIdentificationMethod _bySessionIdCookieIdentificationMethod;
    private readonly string _cookieName;
    private readonly ICookieFactory _fakeCookieFactory;
    private readonly IEncryptionProvider _fakeEncryptionProvider;
    private readonly IHmacProvider _fakeHmacProvider;
    private readonly IHmacValidator _fakeHmacValidator;
    private readonly ISessionIdentificationDataProvider _fakeSessionIdentificationDataProvider;
    private readonly ISessionIdFactory _fakeSessionIdFactory;
    private readonly InProcSessionsConfiguration _validConfiguration;

    public BySessionIdCookieIdentificationMethodFixture() {
      _fakeEncryptionProvider = A.Fake<IEncryptionProvider>();
      _fakeHmacProvider = A.Fake<IHmacProvider>();
      _validConfiguration = new InProcSessionsConfiguration();
      _fakeSessionIdentificationDataProvider = A.Fake<ISessionIdentificationDataProvider>();
      _fakeHmacValidator = A.Fake<IHmacValidator>();
      _fakeSessionIdFactory = A.Fake<ISessionIdFactory>();
      _fakeCookieFactory = A.Fake<ICookieFactory>();
      _bySessionIdCookieIdentificationMethod = new BySessionIdCookieIdentificationMethod(
        _fakeEncryptionProvider,
        _fakeHmacProvider,
        _fakeSessionIdentificationDataProvider,
        _fakeHmacValidator,
        _fakeSessionIdFactory,
        _fakeCookieFactory);
      _cookieName = "TheNameOfTheCookie";
      _bySessionIdCookieIdentificationMethod.CookieName = _cookieName;
    }

    [Fact]
    public void Given_null_crypto_configuration_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new BySessionIdCookieIdentificationMethod(null));
    }

    [Fact]
    public void Given_null_encryption_provider_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new BySessionIdCookieIdentificationMethod(null, _fakeHmacProvider, _fakeSessionIdentificationDataProvider, _fakeHmacValidator, _fakeSessionIdFactory, _fakeCookieFactory));
    }

    [Fact]
    public void Given_null_hmac_provider_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new BySessionIdCookieIdentificationMethod(_fakeEncryptionProvider, null, _fakeSessionIdentificationDataProvider, _fakeHmacValidator, _fakeSessionIdFactory, _fakeCookieFactory));
    }

    [Fact]
    public void Given_null_session_data_provider_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new BySessionIdCookieIdentificationMethod(_fakeEncryptionProvider, _fakeHmacProvider, null, _fakeHmacValidator, _fakeSessionIdFactory, _fakeCookieFactory));
    }

    [Fact]
    public void Given_null_hmac_validator_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new BySessionIdCookieIdentificationMethod(_fakeEncryptionProvider, _fakeHmacProvider, _fakeSessionIdentificationDataProvider, null, _fakeSessionIdFactory, _fakeCookieFactory));
    }

    [Fact]
    public void Given_null_session_id_factory_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new BySessionIdCookieIdentificationMethod(_fakeEncryptionProvider, _fakeHmacProvider, _fakeSessionIdentificationDataProvider, _fakeHmacValidator, null, _fakeCookieFactory));
    }

    [Fact]
    public void Given_null_cookie_factory_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new BySessionIdCookieIdentificationMethod(_fakeEncryptionProvider, _fakeHmacProvider, _fakeSessionIdentificationDataProvider, _fakeHmacValidator, _fakeSessionIdFactory, null));
    }

    [Fact]
    public void On_creation_sets_default_cookie_name() {
      var newInstance = new BySessionIdCookieIdentificationMethod(_fakeEncryptionProvider, _fakeHmacProvider, _fakeSessionIdentificationDataProvider, _fakeHmacValidator, _fakeSessionIdFactory, _fakeCookieFactory);
      Assert.Equal("_nsid", newInstance.CookieName);
    }

    public class Load : BySessionIdCookieIdentificationMethodFixture {
      private readonly NancyContext _context;
      private readonly SessionId _newSessionId;

      public Load() {
        _context = new NancyContext {Request = new Request("GET", "http://www.google.be")};

        _newSessionId = new SessionId(Guid.NewGuid(), true);
        A.CallTo(() => _fakeSessionIdFactory.CreateNew()).Returns(_newSessionId);
      }

      [Fact]
      public void Given_null_context_then_throws() {
        Assert.Throws<ArgumentNullException>(() => _bySessionIdCookieIdentificationMethod.GetCurrentSessionId(null));
      }

      [Fact]
      public void When_context_contains_no_session_identification_data_then_returns_new_session_id() {
        A.CallTo(() => _fakeSessionIdentificationDataProvider.ProvideDataFromCookie(_context.Request, _cookieName)).Returns(null);

        var actual = _bySessionIdCookieIdentificationMethod.GetCurrentSessionId(_context);

        Assert.Equal(_newSessionId, actual);
        A.CallTo(() => _fakeSessionIdFactory.CreateNew()).MustHaveHappened();
        A.CallTo(() => _fakeSessionIdFactory.CreateFrom(A<string>._)).MustNotHaveHappened();
      }

      [Fact]
      public void When_cookie_does_not_have_a_valid_hmac_then_returns_new_session_id() {
        var cookieData = new SessionIdentificationData {SessionId = "ABCSomeEncryptedSessionIdXYZ", Hmac = new byte[] {1, 2, 3}};

        A.CallTo(() => _fakeSessionIdentificationDataProvider.ProvideDataFromCookie(_context.Request, _cookieName)).Returns(cookieData);
        A.CallTo(() => _fakeHmacValidator.IsValidHmac(cookieData)).Returns(false);

        var actual = _bySessionIdCookieIdentificationMethod.GetCurrentSessionId(_context);

        Assert.Equal(_newSessionId, actual);
        A.CallTo(() => _fakeSessionIdFactory.CreateNew()).MustHaveHappened();
        A.CallTo(() => _fakeSessionIdFactory.CreateFrom(A<string>._)).MustNotHaveHappened();
      }

      [Fact]
      public void When_decrypted_session_id_is_not_valid_then_returns_new_session_id() {
        var cookieData = new SessionIdentificationData {SessionId = "ABCSomeEncryptedSessionIdXYZ", Hmac = new byte[] {1, 2, 3}};

        A.CallTo(() => _fakeSessionIdentificationDataProvider.ProvideDataFromCookie(_context.Request, _cookieName)).Returns(cookieData);
        A.CallTo(() => _fakeHmacValidator.IsValidHmac(cookieData)).Returns(true);
        A.CallTo(() => _fakeEncryptionProvider.Decrypt(cookieData.SessionId)).Returns(string.Empty);

        var actual = _bySessionIdCookieIdentificationMethod.GetCurrentSessionId(_context);

        Assert.Equal(_newSessionId, actual);
        A.CallTo(() => _fakeSessionIdFactory.CreateNew()).MustHaveHappened();
        A.CallTo(() => _fakeSessionIdFactory.CreateFrom(A<string>._)).MustNotHaveHappened();
      }

      [Fact]
      public void When_decrypted_session_id_is_not_a_valid_guid_then_returns_new_session_id() {
        const string invalidDecryptedSessionId = "This is not a valid guid!";
        var cookieData = new SessionIdentificationData {SessionId = "ABCSomeEncryptedSessionIdXYZ", Hmac = new byte[] {1, 2, 3}};

        A.CallTo(() => _fakeSessionIdentificationDataProvider.ProvideDataFromCookie(_context.Request, _cookieName)).Returns(cookieData);
        A.CallTo(() => _fakeHmacValidator.IsValidHmac(cookieData)).Returns(true);
        A.CallTo(() => _fakeEncryptionProvider.Decrypt(cookieData.SessionId)).Returns(invalidDecryptedSessionId);
        A.CallTo(() => _fakeSessionIdFactory.CreateFrom(invalidDecryptedSessionId)).Returns(null);

        var actual = _bySessionIdCookieIdentificationMethod.GetCurrentSessionId(_context);

        Assert.Equal(_newSessionId, actual);
        A.CallTo(() => _fakeSessionIdFactory.CreateNew()).MustHaveHappened();
        A.CallTo(() => _fakeSessionIdFactory.CreateFrom(invalidDecryptedSessionId)).MustHaveHappened();
      }

      [Fact]
      public void When_decrypted_session_id_is_valid_then_returns_session_id_from_cookie() {
        var expectedSessionId = new SessionId(Guid.NewGuid(), false);
        var decryptedSessionId = expectedSessionId.Value.ToString();
        var cookieData = new SessionIdentificationData {SessionId = "ABCSomeEncryptedSessionIdXYZ", Hmac = new byte[] {1, 2, 3}};

        A.CallTo(() => _fakeSessionIdentificationDataProvider.ProvideDataFromCookie(_context.Request, _cookieName)).Returns(cookieData);
        A.CallTo(() => _fakeHmacValidator.IsValidHmac(cookieData)).Returns(true);
        A.CallTo(() => _fakeEncryptionProvider.Decrypt(cookieData.SessionId)).Returns(decryptedSessionId);
        A.CallTo(() => _fakeSessionIdFactory.CreateFrom(decryptedSessionId)).Returns(expectedSessionId);

        var actual = _bySessionIdCookieIdentificationMethod.GetCurrentSessionId(_context);

        Assert.Equal(expectedSessionId, actual);
        A.CallTo(() => _fakeSessionIdFactory.CreateNew()).MustNotHaveHappened();
        A.CallTo(() => _fakeSessionIdFactory.CreateFrom(decryptedSessionId)).MustHaveHappened();
      }
    }

    public class Save : BySessionIdCookieIdentificationMethodFixture {
      private readonly NancyContext _context;
      private readonly SessionId _validSessionId;

      public Save() {
        _validSessionId = new SessionId(Guid.NewGuid(), false);
        _context = new NancyContext {Response = new Response()};
      }

      [Fact]
      public void Given_null_context_then_throws() {
        Assert.Throws<ArgumentNullException>(() => _bySessionIdCookieIdentificationMethod.SaveSessionId(null, null));
      }

      [Fact]
      public void Given_null_session_id_then_throws() {
        Assert.Throws<ArgumentNullException>(() => _bySessionIdCookieIdentificationMethod.SaveSessionId(null, _context));
      }

      [Fact]
      public void Given_context_without_response_then_throws() {
        _context.Response = null;
        Assert.Throws<ArgumentException>(() => _bySessionIdCookieIdentificationMethod.SaveSessionId(_validSessionId, _context));
      }

      [Fact]
      public void Given_empty_session_id_then_throws() {
        var emptySessionId = new SessionId(Guid.Empty, false);
        Assert.Throws<ArgumentException>(() => _bySessionIdCookieIdentificationMethod.SaveSessionId(emptySessionId, _context));
      }

      [Fact]
      public void Adds_expected_cookie_to_response_containing_data_from_encryptionprovider_and_hmacprovider_and_returns_null() {
        const string encryptedSessionId = "ABC_sessionid_xyz";
        var hmacBytes = new byte[] {1, 2, 3};
        var hmacString = Convert.ToBase64String(hmacBytes);
        var expectedCookieData = string.Format("{0}{1}", encryptedSessionId, hmacString);

        A.CallTo(() => _fakeEncryptionProvider.Encrypt(_validSessionId.Value.ToString())).Returns(encryptedSessionId);
        A.CallTo(() => _fakeHmacProvider.GenerateHmac(encryptedSessionId)).Returns(hmacBytes);
        A.CallTo(() => _fakeHmacProvider.HmacLength).Returns(hmacBytes.Length);
        A.CallTo(
          () =>
            _fakeCookieFactory.CreateCookie(
              _cookieName,
              null,
              null,
              A<SessionIdentificationData>.That.Matches(cookieData => cookieData.SessionId == encryptedSessionId && HmacComparer.Compare(cookieData.Hmac, hmacBytes, _fakeHmacProvider.HmacLength))))
          .Returns(new NancyCookie("cookiefortest", expectedCookieData));

        _bySessionIdCookieIdentificationMethod.SaveSessionId(_validSessionId, _context);

        var addedCookie = _context.Response.Cookies.FirstOrDefault(cookie => cookie.Value == expectedCookieData);
        Assert.NotNull(addedCookie);
      }
    }
  }
}