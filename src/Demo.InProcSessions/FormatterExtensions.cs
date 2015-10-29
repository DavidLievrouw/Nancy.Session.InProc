namespace Demo.InProcSessions {
  using System;
  using Nancy;

  public static class FormatterExtensions {
    public static Response AsNonCachedText(this IResponseFormatter responseFormatter, string responseText) {
      if (responseFormatter == null) throw new ArgumentNullException("responseFormatter");

      return responseFormatter.AsText(responseText)
        .WithHeader("Cache-Control", "no-cache, no-store, must-revalidate")
        .WithHeader("Pragma", "no-cache")
        .WithHeader("Expires", "0");
    }
  }
}