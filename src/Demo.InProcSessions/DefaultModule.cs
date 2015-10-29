namespace Demo.InProcSessions {
  using Nancy;

  public class DefaultModule : NancyModule {
    public DefaultModule() {
      Get["/"] = parameters => {
        var currentSession = Context.Request.Session;
        var currentValue = (int?) currentSession["TestValue"];

        var responseText = string.Format(
          "Current session test value: {0}",
          currentValue.HasValue
            ? currentValue.Value.ToString()
            : "[null]");

        return Response.AsNonCachedText(responseText);
      };

      Get["/increment"] = parameters => {
        var currentSession = Context.Request.Session;
        var currentValue = (int?) currentSession["TestValue"];

        currentSession["TestValue"] = currentValue + 1 ?? 0;
        var responseText = string.Format("Current session test value after increment: {0}", currentSession["TestValue"]);

        return Response.AsNonCachedText(responseText);
      };
    }
  }
}