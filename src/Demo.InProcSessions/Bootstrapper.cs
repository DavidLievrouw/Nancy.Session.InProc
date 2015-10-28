using System;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

namespace Nancy.Demo.InProcSessions {
  public class Bootstrapper : DefaultNancyBootstrapper {
    protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines) {
      StaticConfiguration.DisableErrorTraces = false;

      /*var sessionConfig = new InProcSessionsConfiguration {
        SessionTimeout = TimeSpan.FromMinutes(3),
        CacheTrimInterval = TimeSpan.FromMinutes(10)
      };
      InProcSessions.Enable(pipelines, sessionConfig);
      */

      base.ApplicationStartup(container, pipelines);
    }
  }
}