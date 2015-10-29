﻿namespace Nancy.Session.InProc {
  using System;
  using Cryptography;
  using InProcSessionsManagement;
  using InProcSessionsManagement.BySessionIdCookie;

  /// <summary>
  ///   Configuration options for in-process memory based sessions
  /// </summary>
  public class InProcSessionsConfiguration {
    private const int DefaultSessionTimeoutMinutes = 20;
    private const int DefaultCacheTrimIntervalMinutes = 30;

    /// <summary>
    ///   An instance of the <see cref="InProcSessionsConfiguration" /> class, using its default values.
    /// </summary>
    public static readonly InProcSessionsConfiguration Default = new InProcSessionsConfiguration();

    /// <summary>
    ///   Initializes a new instance of the <see cref="InProcSessionsConfiguration" /> class.
    /// </summary>
    public InProcSessionsConfiguration() {
      SessionIdentificationMethod = new BySessionIdCookieIdentificationMethod(CryptographyConfiguration.Default);
      SessionTimeout = TimeSpan.FromMinutes(DefaultSessionTimeoutMinutes);
      CacheTrimInterval = TimeSpan.FromMinutes(DefaultCacheTrimIntervalMinutes);
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="InProcSessionsConfiguration" /> class.
    /// </summary>
    public InProcSessionsConfiguration(CryptographyConfiguration cryptographyConfiguration, IInProcSessionIdentificationMethod sessionIdentificationMethod) {
      SessionIdentificationMethod = sessionIdentificationMethod;
      SessionTimeout = TimeSpan.FromMinutes(DefaultSessionTimeoutMinutes);
      CacheTrimInterval = TimeSpan.FromMinutes(DefaultCacheTrimIntervalMinutes);
    }

    /// <summary>
    ///   Gets or sets the method that is used to identify the session from the context
    /// </summary>
    public IInProcSessionIdentificationMethod SessionIdentificationMethod { get; set; }

    /// <summary>
    ///   Gets or sets the time after which a memory session expires
    /// </summary>
    public TimeSpan SessionTimeout { get; set; }

    /// <summary>
    ///   Gets or sets the time after which a the expired sessions are cleaned up.
    /// </summary>
    public TimeSpan CacheTrimInterval { get; set; }

    /// <summary>
    ///   Gets a value indicating whether the configuration is valid or not.
    /// </summary>
    public bool IsValid {
      get {
        if (SessionIdentificationMethod == null) {
          return false;
        }

        if (SessionTimeout <= TimeSpan.Zero) {
          return false;
        }

        if (CacheTrimInterval < TimeSpan.Zero) {
          return false;
        }

        return true;
      }
    }
  }
}