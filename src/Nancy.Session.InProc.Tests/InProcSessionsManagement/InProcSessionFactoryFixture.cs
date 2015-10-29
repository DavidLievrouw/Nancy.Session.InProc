namespace Nancy.Session.InProc.InProcSessionsManagement {
  using System;
  using Cache;
  using FakeItEasy;
  using Xunit;

  public class InProcSessionFactoryFixture {
    private readonly ISystemClock _fakeSystemClock;
    private readonly InProcSessionFactory _sessionFactory;
    private readonly InProcSessionsConfiguration _validConfiguration;

    public InProcSessionFactoryFixture() {
      _fakeSystemClock = A.Fake<ISystemClock>();
      _validConfiguration = new InProcSessionsConfiguration {SessionIdentificationMethod = A.Dummy<IInProcSessionIdentificationMethod>(), SessionTimeout = TimeSpan.FromMinutes(30)};
      _sessionFactory = new InProcSessionFactory(_validConfiguration, _fakeSystemClock);
    }

    [Fact]
    public void Given_null_configuration_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new InProcSessionFactory(null, _fakeSystemClock));
    }

    [Fact]
    public void Given_null_system_clock_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new InProcSessionFactory(_validConfiguration, null));
    }

    [Fact]
    public void Given_null_session_id_then_throws() {
      Assert.Throws<ArgumentNullException>(() => _sessionFactory.Create(null, A.Dummy<ISession>()));
    }

    [Fact]
    public void Given_empty_session_id_then_throws() {
      var emptySessionId = new SessionId(Guid.Empty, false);
      Assert.Throws<ArgumentException>(() => _sessionFactory.Create(emptySessionId, A.Dummy<ISession>()));
    }

    [Fact]
    public void Given_null_inner_session_then_throws() {
      var sessionId = new SessionId(Guid.NewGuid(), false);
      Assert.Throws<ArgumentNullException>(() => _sessionFactory.Create(sessionId, null));
    }

    [Fact]
    public void Creates_session_with_expected_values() {
      var sessionId = new SessionId(Guid.NewGuid(), false);
      var innerSession = A.Dummy<ISession>();
      var nowUtc = new DateTime(2015, 10, 22, 16, 23, 14);
      ConfigureSystemClock_ToReturn(nowUtc);

      var actual = _sessionFactory.Create(sessionId, innerSession);

      Assert.NotNull(actual);
      Assert.IsAssignableFrom<InProcSession>(actual);
      Assert.Equal(sessionId, actual.Id);
      Assert.Equal(nowUtc, actual.LastSave);
      Assert.Equal(_validConfiguration.SessionTimeout, actual.Timeout);
      Assert.Equal(innerSession, actual.WrappedSession);
    }

    private void ConfigureSystemClock_ToReturn(DateTime nowUtc) {
      A.CallTo(() => _fakeSystemClock.NowUtc).Returns(nowUtc);
    }
  }
}