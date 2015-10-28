# In-process sessions

This package was developed in order to be able to work with in-process memory sessions. The current NancyFX version only works with cookie-based sessions.

A couple of reasons for doing this:
- Try keeping a non-serializable object in the session. This doesn't work using the standard NancyFx library. So I would like to keep it in memory, and only send a lightweight correlation ID around.
- When working with larger objects, the cookie data gets quite large. And that's going over the wire with every request. 
- I would like to be able to use cookieless sessions, over HTTPS.

### Super-Duper-Happy-Path

```cs
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            InProcSessions.Enable(pipelines);
        }
    }
```
This command enables the in-process sessions, using the default configuration options:
- A cookie is created, containing **only** the unique identifier of the session.
- This cookie is encrypted using CryptographyConfiguration.Default.
- The cookie name is "_nsid".
- There is no Domain or Path setting for the cookie.
- The session timeout is 20 minutes.
- Expired sessions are cleaned automatically every 30 minutes.

### Configuration options
```cs
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            
            var sessionConfig = new InProcSessionsConfiguration
            {
                SessionTimeout = TimeSpan.FromMinutes(3),
                CacheTrimInterval = TimeSpan.FromMinutes(10),
                SessionIdentificationMethod = new BySessionIdCookieIdentificationMethod(CryptographyConfiguration.NoEncryption)
                {
                    CookieName = "Mycookie",
                    Domain = ".nascar.com",
                    Path = "/schedule/"
                }
            };
            InProcSessions.Enable(pipelines, sessionConfig);
        }
    }
```

You can specify several configuration options when enabling this feature:
- SessionTimeout: The timeout after which sessions expire.
- CacheTrimInterval: Time interval between automatic cleanup actions of expired sessions (async).
- SessionIdentificationMethod:
  - Defines the method by which the session is identified. Two implementations are provided:
    - BySessionIdCookie (default):
      - CookieName, CookiePath and CookieDomain.
      - CryptographyConfiguration.
    - ByQueryStringParam (for cookieless sessions, more on that later)
      - ParameterName: The name of the querystring parameter in which the session identifier is stored. 
      - CryptographyConfiguration.

### Cookieless sessions
The default session identification method is BySessionIdCookie. This is very similar with how the current CookieBasedSessions feature works: It creates a cookie that is sent with each request, in order to be able to load the session.
The big difference: The session data is not in the cookie value itself. Only a correlation ID.

When I worked with ASP.NET, there was a feature called cookieless sessions. There are a lot of pros and cons to consider, when implementing this. But I found it useful in the past. So I thought it would be a nice feature to add to NancyFX.

```cs
public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            var sessionConfig = new InProcSessionsConfiguration
            {
                SessionIdentificationMethod = new ByQueryStringParamIdentificationMethod(CryptographyConfiguration.Default)
            };
            InProcSessions.Enable(pipelines, sessionConfig);

            base.ApplicationStartup(container, pipelines);
        }
    }
```

When navigating to a page without the session ID querystring parameter, a 302-FOUND response with a location header is sent back to the client. This location header redirects to the same location, but includes the session ID querystring parameter.

### Roll your own
Currently, two session identification methods are implemented:
- BySessionIdCookie
- ByQueryStringParam

You can create your own method. Implement the interface and wire it up.

```cs
    /// <summary>
    /// Identification method for in-process memory based sessions.
    /// </summary>
    public class MySessionIdentificationMethod : IInProcSessionIdentificationMethod
    {
        /// <summary>
        /// Load the session identifier from the specified context.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <returns>The identifier of the session for the current request.</returns>
        public SessionId GetCurrentSessionId(NancyContext context)
        {
            // Load the session identifier from the context.
            // Or create a new one, if no session data is sent with the request.
        }

        /// <summary>
        /// Save the session identifier in the specified context.
        /// </summary>
        /// <param name="sessionId">The identifier of the session.</param>
        /// <param name="context">The current context.</param>
        public void SaveSessionId(SessionId sessionId, NancyContext context)
        {
            // Make sure that the next request from the client
            // contains this session ID.
        }
    }
```

And modify your bootstrapper:
```cs
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            var sessionConfig = new InProcSessionsConfiguration
            {
                SessionIdentificationMethod = new MySessionIdentificationMethod()
            };
            InProcSessions.Enable(pipelines, sessionConfig);

            base.ApplicationStartup(container, pipelines);
        }
    }
```

### Contact
Feel free to contact me. I would like some feedback on this.