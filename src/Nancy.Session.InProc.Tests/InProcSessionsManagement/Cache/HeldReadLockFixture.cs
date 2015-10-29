namespace Nancy.Session.InProc.InProcSessionsManagement.Cache {
  using System;
  using System.Threading;
  using Xunit;

  public class HeldReadLockFixture : IDisposable {
    private readonly ReaderWriterLockSlim _wrappedLock;

    public HeldReadLockFixture() {
      _wrappedLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
    }

    public void Dispose() {
      _wrappedLock.Dispose();
    }

    [Fact]
    public void Given_null_readerwriterlock_then_throws() {
      Assert.Throws<ArgumentNullException>(() => new HeldReadLock(null));
    }

    [Fact]
    public void When_creating_then_acquires_lock() {
      using (new HeldReadLock(_wrappedLock)) {
        var actual = _wrappedLock.IsReadLockHeld;
        Assert.True(actual);
      }
    }

    [Fact]
    public void When_disposing_then_releases_lock() {
      using (new HeldReadLock(_wrappedLock)) {}

      var actual = _wrappedLock.IsReadLockHeld;
      Assert.False(actual);
    }
  }
}