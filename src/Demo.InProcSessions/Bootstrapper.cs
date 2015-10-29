namespace Demo.InProcSessions {
  using System;
  using Nancy;
  using Nancy.Bootstrapper;
  using Nancy.Cryptography;
  using Nancy.Session.InProc;
  using Nancy.Session.InProc.InProcSessionsManagement.ByQueryStringParam;
  using Nancy.TinyIoc;

  public class Bootstrapper : DefaultNancyBootstrapper {
    protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines) {
      StaticConfiguration.DisableErrorTraces = false;

      var sessionConfig = new InProcSessionsConfiguration {
        SessionTimeout = TimeSpan.FromMinutes(3),
        CacheTrimInterval = TimeSpan.FromMinutes(10),
        SessionIdentificationMethod = new ByQueryStringParamIdentificationMethod(CryptographyConfiguration.Default)
      };
      InProcSessions.Enable(pipelines, sessionConfig);

      base.ApplicationStartup(container, pipelines);
    }
  }
}