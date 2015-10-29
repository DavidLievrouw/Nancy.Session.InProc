namespace Nancy.Session.InProc.InProcSessionsManagement.Cache {
  using System;
  using System.Threading;
  using Xunit;

  public class HeldWriteLockFixture : IDisposable {
    private readonly ReaderWriterLockSlim _wrappedLock;

    public HeldWriteLockFixture() {
      _wrappedLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
    }

    public void Dispose() {
      _wrappedLock.Dispose();
    }

    [Fact]
    public void Given_null_readerwriterlock_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new HeldWriteLock(null));
    }

    [Fact]
    public void When_creating_then_acquires_lock() {
      using (new HeldWriteLock(_wrappedLock)) {
        var actual = _wrappedLock.IsWriteLockHeld;
        Assert.True(actual);
      }
    }

    [Fact]
    public void When_disposing_then_releases_lock() {
      using (new HeldWriteLock(_wrappedLock)) {}

      var actual = _wrappedLock.IsWriteLockHeld;
      Assert.False(actual);
    }
  }
}