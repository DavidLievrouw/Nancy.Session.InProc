namespace Nancy.Session.InProc.InProcSessionsManagement {
  using System;
  using Cache;
  using FakeItEasy;
  using Xunit;

  public class InProcSessionManagerFixture {
    private readonly IPeriodicCacheCleaner _fakePeriodicCacheCleaner;
    private readonly IInProcSessionCache _fakeSessionCache;
    private readonly IInProcSessionFactory _fakeSessionFactory;
    private readonly IInProcSessionIdentificationMethod _fakeSessionIdentificationMethod;
    private readonly NancyContext _nancyContext;
    private readonly InProcSessionManager _sessionManager;
    private readonly InProcSessionsConfiguration _validConfiguration;

    public InProcSessionManagerFixture() {
      _nancyContext = new NancyContext();
      _fakeSessionIdentificationMethod = A.Fake<IInProcSessionIdentificationMethod>();
      _validConfiguration = new InProcSessionsConfiguration {SessionIdentificationMethod = _fakeSessionIdentificationMethod, SessionTimeout = TimeSpan.FromMinutes(30), CacheTrimInterval = TimeSpan.FromMinutes(40)};
      _fakeSessionCache = A.Fake<IInProcSessionCache>();
      _fakeSessionFactory = A.Fake<IInProcSessionFactory>();
      _fakePeriodicCacheCleaner = A.Fake<IPeriodicCacheCleaner>();
      _sessionManager = new InProcSessionManager(_validConfiguration, _fakeSessionCache, _fakeSessionFactory, _fakePeriodicCacheCleaner);
    }

    [Fact]
    public void Given_null_configuration_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new InProcSessionManager(null, _fakeSessionCache, _fakeSessionFactory, _fakePeriodicCacheCleaner));
    }

    [Fact]
    public void Given_null_cache_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new InProcSessionManager(_validConfiguration, null, _fakeSessionFactory, _fakePeriodicCacheCleaner));
    }

    [Fact]
    public void Given_null_factory_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new InProcSessionManager(_validConfiguration, _fakeSessionCache, null, _fakePeriodicCacheCleaner));
    }

    [Fact]
    public void Given_null_periodic_cleaner_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new InProcSessionManager(_validConfiguration, _fakeSessionCache, _fakeSessionFactory, null));
    }

    [Fact]
    public void When_constructing_then_starts_periodic_clean_task() {
      A.CallTo(() => _fakePeriodicCacheCleaner.Start()).MustHaveHappened();
    }

    public class Load : InProcSessionManagerFixture {
      [Fact]
      public void Given_null_context_then_throws() {
        Assert.Throws<ArgumentNullException>(() => _sessionManager.Load(null));
      }

      [Fact]
      public void Loads_session_with_id_from_identification_method() {
        var sessionId = new SessionId(Guid.NewGuid(), false);
        var expectedSession = new InProcSession(sessionId, A.Fake<ISession>(), DateTime.Now, TimeSpan.FromMinutes(10));

        A.CallTo(() => _fakeSessionIdentificationMethod.GetCurrentSessionId(_nancyContext)).Returns(sessionId);
        A.CallTo(() => _fakeSessionCache.Get(sessionId)).Returns(expectedSession);

        var actual = _sessionManager.Load(_nancyContext);

        Assert.Equal(expectedSession, actual);
        A.CallTo(() => _fakeSessionIdentificationMethod.GetCurrentSessionId(_nancyContext)).MustHaveHappened();
        A.CallTo(() => _fakeSessionCache.Get(sessionId)).MustHaveHappened();
      }

      [Fact]
      public void When_session_is_not_found_then_returns_new_empty_session() {
        var sessionId = new SessionId(Guid.NewGuid(), false);

        A.CallTo(() => _fakeSessionIdentificationMethod.GetCurrentSessionId(_nancyContext)).Returns(sessionId);
        A.CallTo(() => _fakeSessionCache.Get(sessionId)).Returns(null);

        var actual = _sessionManager.Load(_nancyContext);

        Assert.NotNull(actual);
        Assert.IsNotType<InProcSession>(actual);
        Assert.Equal(0, actual.Count);
      }
    }

    public class Save : InProcSessionManagerFixture {
      private readonly ISession _fakeSession;

      public Save() {
        _fakeSession = A.Fake<ISession>();
        A.CallTo(() => _fakeSession.Count).Returns(2);
        A.CallTo(() => _fakeSession.HasChanged).Returns(true);
      }

      [Fact]
      public void Given_null_context_then_throws() {
        Assert.Throws<ArgumentNullException>(() => _sessionManager.Save(_fakeSession, null));
      }

      [Fact]
      public void Given_null_session_then_does_not_save() {
        _sessionManager.Save(null, _nancyContext);

        A.CallTo(() => _fakeSessionCache.Set(A<InProcSession>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeSessionIdentificationMethod.SaveSessionId(A<SessionId>._, A<NancyContext>._)).MustNotHaveHappened();
      }

      [Fact]
      public void Given_unchanged_session_then_does_not_save() {
        A.CallTo(() => _fakeSession.HasChanged).Returns(false);

        _sessionManager.Save(_fakeSession, _nancyContext);

        A.CallTo(() => _fakeSessionCache.Set(A<InProcSession>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeSessionIdentificationMethod.SaveSessionId(A<SessionId>._, A<NancyContext>._)).MustNotHaveHappened();
      }

      [Fact]
      public void When_no_session_is_present_in_context_then_does_not_save() {
        _sessionManager.Save(new NullSessionProvider(), _nancyContext);

        A.CallTo(() => _fakeSessionCache.Set(A<InProcSession>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeSessionIdentificationMethod.SaveSessionId(A<SessionId>._, A<NancyContext>._)).MustNotHaveHappened();
      }

      [Fact]
      public void When_no_data_is_present_in_session_then_does_not_save() {
        A.CallTo(() => _fakeSession.Count).Returns(0);

        _sessionManager.Save(_fakeSession, _nancyContext);

        A.CallTo(() => _fakeSessionCache.Set(A<InProcSession>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeSessionIdentificationMethod.SaveSessionId(A<SessionId>._, A<NancyContext>._)).MustNotHaveHappened();
      }

      [Fact]
      public void Given_valid_session_then_caches_that_session() {
        var sessionId = new SessionId(Guid.NewGuid(), true);
        var sessionToSave = new InProcSession(sessionId, _fakeSession, DateTime.Now, TimeSpan.FromMinutes(10));
        A.CallTo(() => _fakeSessionIdentificationMethod.GetCurrentSessionId(_nancyContext)).Returns(sessionId);
        A.CallTo(() => _fakeSessionFactory.Create(sessionId, _fakeSession)).Returns(sessionToSave);

        _sessionManager.Save(_fakeSession, _nancyContext);
        A.CallTo(() => _fakeSessionCache.Set(sessionToSave)).MustHaveHappened();
      }

      [Fact]
      public void Given_valid_session_then_saves_that_session_using_method_from_configuration() {
        var sessionId = new SessionId(Guid.NewGuid(), true);
        A.CallTo(() => _fakeSessionIdentificationMethod.GetCurrentSessionId(_nancyContext)).Returns(sessionId);

        _sessionManager.Save(_fakeSession, _nancyContext);
        A.CallTo(() => _fakeSessionIdentificationMethod.SaveSessionId(sessionId, _nancyContext)).MustHaveHappened();
      }
    }
  }
}