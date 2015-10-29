namespace Nancy.Session.InProc.InProcSessionsManagement.BySessionIdCookie {
  using System;
  using Cryptography;

  /// <summary>
  ///   Identification method for in-process memory based sessions, using a cookie that contains the session identifier.
  /// </summary>
  public class BySessionIdCookieIdentificationMethod : IBySessionIdCookieIdentificationMethod {
    private const string DefaultCookieName = "_nsid";
    private readonly ICookieFactory _cookieFactory;
    private readonly IEncryptionProvider _encryptionProvider;
    private readonly IHmacProvider _hmacProvider;
    private readonly IHmacValidator _hmacValidator;
    private readonly ISessionIdentificationDataProvider _sessionIdentificationDataProvider;
    private readonly ISessionIdFactory _sessionIdFactory;

    /// <summary>
    ///   Initializes a new instance of the <see cref="BySessionIdCookieIdentificationMethod" /> class.
    /// </summary>
    public BySessionIdCookieIdentificationMethod(CryptographyConfiguration cryptoConfig) {
      if (cryptoConfig == null) throw new ArgumentNullException("cryptoConfig");
      _encryptionProvider = cryptoConfig.EncryptionProvider;
      _hmacProvider = cryptoConfig.HmacProvider;
      _sessionIdentificationDataProvider = new SessionIdentificationDataProvider(cryptoConfig.HmacProvider);
      _hmacValidator = new HmacValidator(cryptoConfig.HmacProvider);
      _sessionIdFactory = new SessionIdFactory();
      _cookieFactory = new CookieFactory();
      CookieName = DefaultCookieName;
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="BySessionIdCookieIdentificationMethod" /> class.
    /// </summary>
    internal BySessionIdCookieIdentificationMethod(
      IEncryptionProvider encryptionProvider,
      IHmacProvider hmacProvider,
      ISessionIdentificationDataProvider sessionIdentificationDataProvider,
      IHmacValidator hmacValidator,
      ISessionIdFactory sessionIdFactory,
      ICookieFactory cookieFactory) {
      if (encryptionProvider == null) throw new ArgumentNullException("encryptionProvider");
      if (hmacProvider == null) throw new ArgumentNullException("hmacProvider");
      if (sessionIdentificationDataProvider == null) throw new ArgumentNullException("sessionIdentificationDataProvider");
      if (hmacValidator == null) throw new ArgumentNullException("hmacValidator");
      if (sessionIdFactory == null) throw new ArgumentNullException("sessionIdFactory");
      if (cookieFactory == null) throw new ArgumentNullException("cookieFactory");
      _encryptionProvider = encryptionProvider;
      _hmacProvider = hmacProvider;
      _sessionIdentificationDataProvider = sessionIdentificationDataProvider;
      _hmacValidator = hmacValidator;
      _sessionIdFactory = sessionIdFactory;
      _cookieFactory = cookieFactory;
      CookieName = DefaultCookieName;
    }

    /// <summary>
    ///   Gets or sets the cookie name in which the session id is stored.
    /// </summary>
    public string CookieName { get; set; }

    /// <summary>
    ///   Gets or sets the domain of the session cookie.
    /// </summary>
    public string Domain { get; set; }

    /// <summary>
    ///   Gets or sets the path of the session cookie.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    ///   Load the session identifier from the specified context.
    /// </summary>
    /// <param name="context">The current context.</param>
    /// <returns>The identifier of the session for the current request.</returns>
    public SessionId GetCurrentSessionId(NancyContext context) {
      if (context == null) throw new ArgumentNullException("context");

      var cookieData = _sessionIdentificationDataProvider.ProvideDataFromCookie(context.Request, CookieName);
      if (cookieData == null) {
        return _sessionIdFactory.CreateNew();
      }
      var isHmacValid = _hmacValidator.IsValidHmac(cookieData);
      if (!isHmacValid) {
        return _sessionIdFactory.CreateNew();
      }

      var decryptedSessionId = _encryptionProvider.Decrypt(cookieData.SessionId);
      if (string.IsNullOrEmpty(decryptedSessionId)) {
        return _sessionIdFactory.CreateNew();
      }

      return _sessionIdFactory.CreateFrom(decryptedSessionId) ?? _sessionIdFactory.CreateNew();
    }

    /// <summary>
    ///   Save the session identifier in the specified context.
    /// </summary>
    /// <param name="sessionId">The identifier of the session.</param>
    /// <param name="context">The current context.</param>
    public void SaveSessionId(SessionId sessionId, NancyContext context) {
      if (sessionId == null) throw new ArgumentNullException("sessionId");
      if (context == null) throw new ArgumentNullException("context");
      if (context.Response == null) throw new ArgumentException("The specified context does not contain a response to modify", "context");
      if (sessionId.IsEmpty) throw new ArgumentException("The specified session id cannot be empty", "sessionId");

      var encryptedSessionId = _encryptionProvider.Encrypt(sessionId.Value.ToString());
      var hmacBytes = _hmacProvider.GenerateHmac(encryptedSessionId);

      var sessionIdentificationData = new SessionIdentificationData {SessionId = encryptedSessionId, Hmac = hmacBytes};

      var cookie = _cookieFactory.CreateCookie(CookieName, Domain, Path, sessionIdentificationData);
      context.Response.WithCookie(cookie);
    }
  }
}