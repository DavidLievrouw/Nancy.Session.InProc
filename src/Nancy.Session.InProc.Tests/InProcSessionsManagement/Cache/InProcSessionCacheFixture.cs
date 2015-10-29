namespace Nancy.Session.InProc.InProcSessionsManagement.Cache {
  using System;
  using System.Collections.Generic;
  using System.Threading;
  using FakeItEasy;
  using Xunit;

  public class InProcSessionCacheFixture : IDisposable {
    private readonly InProcSession _activeSession;
    private readonly InProcSession _expiredSession;
    private readonly ISystemClock _fakeSystemClock;
    private readonly InProcSessionCache _inProcSessionCache;
    private readonly DateTime _nowUtc;
    private readonly int _numberOfSessions;

    public InProcSessionCacheFixture() {
      _nowUtc = new DateTime(2015, 10, 20, 21, 36, 14, DateTimeKind.Utc);
      _fakeSystemClock = A.Fake<ISystemClock>();
      ConfigureSystemClock_ToReturn(_nowUtc);

      _inProcSessionCache = new InProcSessionCache(_fakeSystemClock);
      _expiredSession = new InProcSession(new SessionId(Guid.NewGuid(), false), A.Dummy<ISession>(), _nowUtc.AddMinutes(-20), TimeSpan.FromMinutes(15));
      _activeSession = new InProcSession(new SessionId(Guid.NewGuid(), false), A.Dummy<ISession>(), _nowUtc.AddMinutes(-3), TimeSpan.FromMinutes(15));
      _inProcSessionCache.Set(_expiredSession);
      _inProcSessionCache.Set(_activeSession);
      _numberOfSessions = 2;
    }

    public void Dispose() {
      _inProcSessionCache.Dispose();
    }

    [Fact]
    public void Count_returns_number_of_elements() {
      var actual = _inProcSessionCache.Count;
      Assert.Equal(_numberOfSessions, actual);
    }

    [Fact]
    public void Trim_removes_expired_elements() {
      _inProcSessionCache.Trim();

      var expected = _numberOfSessions - 1;
      var actual = _inProcSessionCache.Count;
      Assert.Equal(expected, actual);
    }

    [Fact]
    public void Set_adds_new_element() {
      var extraSession = new InProcSession(new SessionId(Guid.NewGuid(), false), A.Dummy<ISession>(), _nowUtc, TimeSpan.FromMinutes(15));
      _inProcSessionCache.Set(extraSession);

      var expected = _numberOfSessions + 1;
      var actual = _inProcSessionCache.Count;
      Assert.Equal(expected, actual);
    }

    [Fact]
    public void Set_replaces_item_when_element_already_is_cached() {
      var newSession = new InProcSession(_activeSession.Id, A.Dummy<ISession>(), _nowUtc.AddSeconds(10), TimeSpan.FromMinutes(10));
      _inProcSessionCache.Set(newSession);

      var actual = _inProcSessionCache.Count;
      Assert.Equal(_numberOfSessions, actual);

      var sessionAfterSave = _inProcSessionCache.Get(_activeSession.Id);
      Assert.Equal(sessionAfterSave.LastSave, newSession.LastSave);
    }

    [Fact]
    public void Get_gets_element_if_id_is_found() {
      var idToFind = _activeSession.Id;
      var actual = _inProcSessionCache.Get(idToFind);
      Assert.Equal(_activeSession, actual);
    }

    [Fact]
    public void Get_returns_null_if_id_is_not_found() {
      var nonExistingId = new SessionId(Guid.NewGuid(), false);
      var actual = _inProcSessionCache.Get(nonExistingId);
      Assert.Null(actual);
    }

    [Fact]
    public void Get_throws_when_null_session_id_is_specified() {
      Assert.Throws<ArgumentNullException>(() => _inProcSessionCache.Get(null));
    }

    [Fact]
    public void Get_removes_value_if_it_is_expired_and_returns_null() {
      var idToFind = _expiredSession.Id;
      var actualSession = _inProcSessionCache.Get(idToFind);
      Assert.Null(actualSession);

      var expectedCount = _numberOfSessions - 1;
      var actualCount = _inProcSessionCache.Count;
      Assert.Equal(expectedCount, actualCount);
    }

    [Fact]
    public void When_disposed_then_cannot_access_Count() {
      _inProcSessionCache.Dispose();
      Assert.Throws<ObjectDisposedException>(() => _inProcSessionCache.Count);
    }

    [Fact]
    public void When_disposed_then_cannot_access_Get() {
      var idToFind = new SessionId(Guid.NewGuid(), false);
      _inProcSessionCache.Dispose();
      Assert.Throws<ObjectDisposedException>(() => _inProcSessionCache.Get(idToFind));
    }

    [Fact]
    public void When_disposed_then_cannot_access_Set() {
      var extraSession = new InProcSession(new SessionId(Guid.NewGuid(), false), A.Dummy<ISession>(), _nowUtc, TimeSpan.FromMinutes(15));
      _inProcSessionCache.Dispose();
      Assert.Throws<ObjectDisposedException>(() => _inProcSessionCache.Set(extraSession));
    }

    [Fact]
    public void Collection_is_thread_safe() {
      const int numberOfThreads = 500;
      var threadAction = new ThreadStart(
        () => {
          var extraSession1 = new InProcSession(new SessionId(Guid.NewGuid(), false), A.Dummy<ISession>(), _nowUtc.AddMinutes(-20), TimeSpan.FromMinutes(15));
          var extraSession2 = new InProcSession(new SessionId(Guid.NewGuid(), false), A.Dummy<ISession>(), _nowUtc, TimeSpan.FromMinutes(15));
          _inProcSessionCache.Set(extraSession1);
          Thread.Sleep(20);
          _inProcSessionCache.Get(extraSession1.Id);
          _inProcSessionCache.Set(extraSession2);
          _inProcSessionCache.Get(extraSession2.Id);
        });

      var threads = new List<Thread>();
      for (var i = 0; i < numberOfThreads; i++) {
        var newThread = new Thread(threadAction);
        newThread.Start();
        threads.Add(newThread);
      }
      threads.ForEach(thread => thread.Join());

      var expectedNumberOfSessions = numberOfThreads + _numberOfSessions;
      var actualNumberOfSessions = _inProcSessionCache.Count;

      Assert.Equal(expectedNumberOfSessions, actualNumberOfSessions);
    }

    private void ConfigureSystemClock_ToReturn(DateTime nowUtc) {
      A.CallTo(() => _fakeSystemClock.NowUtc).Returns(nowUtc);
    }
  }
}