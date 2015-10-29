namespace Nancy.Session.InProc {
  using System;
  using Xunit;

  public class InProcSessionsConfigurationFixture {
    private readonly InProcSessionsConfiguration _config;

    public InProcSessionsConfigurationFixture() {
      _config = new InProcSessionsConfiguration();
    }

    [Fact]
    public void Should_be_valid_with_all_properties_set() {
      var result = new InProcSessionsConfiguration().IsValid;
      Assert.True(result);
    }

    [Fact]
    public void Should_not_be_valid_with_null_session_identificationmethod() {
      _config.SessionIdentificationMethod = null;

      var result = _config.IsValid;

      Assert.False(result);
    }

    [Fact]
    public void Should_not_be_valid_with_empty_session_timeout() {
      _config.SessionTimeout = TimeSpan.Zero;

      var result = _config.IsValid;

      Assert.False(result);
    }

    [Fact]
    public void Should_not_be_valid_with_negative_session_timeout() {
      _config.SessionTimeout = TimeSpan.FromSeconds(-1);

      var result = _config.IsValid;

      Assert.False(result);
    }

    [Fact]
    public void Should_be_valid_with_empty_cache_trim_interval() {
      _config.CacheTrimInterval = TimeSpan.Zero;

      var result = _config.IsValid;

      Assert.True(result);
    }

    [Fact]
    public void Should_not_be_valid_with_negative_cache_trim_interval() {
      _config.CacheTrimInterval = TimeSpan.FromSeconds(-1);

      var result = _config.IsValid;

      Assert.False(result);
    }
  }
}