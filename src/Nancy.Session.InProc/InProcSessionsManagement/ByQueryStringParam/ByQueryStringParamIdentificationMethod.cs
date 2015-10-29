namespace Nancy.Session.InProc.InProcSessionsManagement.ByQueryStringParam {
  using System;
  using Cryptography;

  /// <summary>
  ///   Identification method for in-process memory based sessions, using a querystring parameter, that contains the session
  ///   identifier.
  /// </summary>
  public class ByQueryStringParamIdentificationMethod : IByQueryStringParamIdentificationMethod {
    private const string DefaultParameterName = "_nsid";
    private readonly IEncryptionProvider _encryptionProvider;
    private readonly IHmacProvider _hmacProvider;
    private readonly IHmacValidator _hmacValidator;
    private readonly IResponseManipulatorForSession _responseManipulatorForSession;
    private readonly ISessionIdentificationDataProvider _sessionIdentificationDataProvider;
    private readonly ISessionIdFactory _sessionIdFactory;

    /// <summary>
    ///   Initializes a new instance of the <see cref="ByQueryStringParamIdentificationMethod" /> class.
    /// </summary>
    public ByQueryStringParamIdentificationMethod(CryptographyConfiguration cryptoConfig) {
      if (cryptoConfig == null) throw new ArgumentNullException("cryptoConfig");
      _encryptionProvider = cryptoConfig.EncryptionProvider;
      _hmacProvider = cryptoConfig.HmacProvider;
      _sessionIdentificationDataProvider = new SessionIdentificationDataProvider(cryptoConfig.HmacProvider);
      _hmacValidator = new HmacValidator(cryptoConfig.HmacProvider);
      _sessionIdFactory = new SessionIdFactory();
      _responseManipulatorForSession = new ResponseManipulatorForSession();
      ParameterName = DefaultParameterName;
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="ByQueryStringParamIdentificationMethod" /> class.
    /// </summary>
    internal ByQueryStringParamIdentificationMethod(
      IEncryptionProvider encryptionProvider,
      IHmacProvider hmacProvider,
      ISessionIdentificationDataProvider sessionIdentificationDataProvider,
      IHmacValidator hmacValidator,
      ISessionIdFactory sessionIdFactory,
      IResponseManipulatorForSession responseManipulatorForSession) {
      if (encryptionProvider == null) throw new ArgumentNullException("encryptionProvider");
      if (hmacProvider == null) throw new ArgumentNullException("hmacProvider");
      if (sessionIdentificationDataProvider == null) throw new ArgumentNullException("configuration");
      if (hmacValidator == null) throw new ArgumentNullException("configuration");
      if (sessionIdFactory == null) throw new ArgumentNullException("configuration");
      if (responseManipulatorForSession == null) throw new ArgumentNullException("responseManipulatorForSession");
      _encryptionProvider = encryptionProvider;
      _hmacProvider = hmacProvider;
      _sessionIdentificationDataProvider = sessionIdentificationDataProvider;
      _hmacValidator = hmacValidator;
      _sessionIdFactory = sessionIdFactory;
      _responseManipulatorForSession = responseManipulatorForSession;
      ParameterName = DefaultParameterName;
    }

    /// <summary>
    ///   Load the session identifier from the specified context.
    /// </summary>
    /// <param name="context">The current context.</param>
    /// <returns>The identifier of the session for the current request.</returns>
    public SessionId GetCurrentSessionId(NancyContext context) {
      if (context == null) throw new ArgumentNullException("context");

      var queryStringData = _sessionIdentificationDataProvider.ProvideDataFromQuery(context.Request, ParameterName);
      if (queryStringData == null) {
        return _sessionIdFactory.CreateNew();
      }
      var isHmacValid = _hmacValidator.IsValidHmac(queryStringData);
      if (!isHmacValid) {
        return _sessionIdFactory.CreateNew();
      }

      var decryptedSessionId = _encryptionProvider.Decrypt(queryStringData.SessionId);
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
      if (context.Request == null) throw new ArgumentException("The specified context does not contain a request", "context");
      if (sessionId.IsEmpty) throw new ArgumentException("The specified session id cannot be empty", "sessionId");

      // Redirect the client to the same url, with the session Id as a query string parameter, if needed
      if (sessionId.IsNew) {
        var encryptedSessionId = _encryptionProvider.Encrypt(sessionId.Value.ToString());
        var hmacBytes = _hmacProvider.GenerateHmac(encryptedSessionId);

        var sessionIdentificationData = new SessionIdentificationData {SessionId = encryptedSessionId, Hmac = hmacBytes};

        _responseManipulatorForSession.ModifyResponseToRedirectToSessionAwareUrl(context, sessionIdentificationData, ParameterName);
      }
    }

    /// <summary>
    ///   Gets or sets the querystring parameter name in which the session id is stored.
    /// </summary>
    public string ParameterName { get; set; }
  }
}