namespace Nancy.Session.InProc.InProcSessionsManagement.Cache {
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading;

  internal class InProcSessionCache : IInProcSessionCache {
    private readonly ReaderWriterLockSlim _rwlock;
    private readonly List<InProcSession> _sessions;
    private readonly ISystemClock _systemClock;
    private bool _isDisposed;

    public InProcSessionCache(ISystemClock systemClock) {
      if (systemClock == null) throw new ArgumentNullException("systemClock");
      _systemClock = systemClock;
      _sessions = new List<InProcSession>();
      _rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
    }

    public int Count {
      get {
        CheckDisposed();
        using (new HeldReadLock(_rwlock)) {
          return _sessions.Count;
        }
      }
    }

    public void Dispose() {
      _sessions.Clear();
      _rwlock.Dispose();
      _isDisposed = true;
    }

    public IEnumerator<InProcSession> GetEnumerator() {
      CheckDisposed();
      return _sessions.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      CheckDisposed();
      return GetEnumerator();
    }

    public void Set(InProcSession session) {
      if (session == null) throw new ArgumentNullException("session");
      CheckDisposed();

      using (new HeldWriteLock(_rwlock)) {
        var index = _sessions.IndexOf(session);

        if (index < 0) {
          _sessions.Add(session);
        } else {
          _sessions[index] = session;
        }
      }
    }

    public void Trim() {
      CheckDisposed();

      using (new HeldWriteLock(_rwlock)) {
        _sessions.RemoveAll(session => session.IsExpired(_systemClock.NowUtc));
      }
    }

    public InProcSession Get(SessionId id) {
      CheckDisposed();
      if (id == null) throw new ArgumentNullException("id");

      using (new HeldUpgradeableReadLock(_rwlock)) {
        var foundSession = _sessions.SingleOrDefault(session => session.Id == id);

        // CQS violation, for convenience
        if (foundSession != null && foundSession.IsExpired(_systemClock.NowUtc)) {
          using (new HeldWriteLock(_rwlock)) {
            _sessions.Remove(foundSession);
            foundSession = null;
          }
        }

        return foundSession;
      }
    }

    private void CheckDisposed() {
      if (_isDisposed) {
        throw new ObjectDisposedException(GetType().Name);
      }
    }
  }
}