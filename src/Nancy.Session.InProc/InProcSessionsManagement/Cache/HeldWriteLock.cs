namespace Nancy.Session.InProc.InProcSessionsManagement.Cache {
  using System.Threading;

  internal class HeldWriteLock : HeldLock {
    public HeldWriteLock(ReaderWriterLockSlim wrappedLock) : base(wrappedLock) {}

    protected override void Acquire() {
      WrappedLock.EnterWriteLock();
    }

    public override void Dispose() {
      if (WrappedLock.IsWriteLockHeld) {
        WrappedLock.ExitWriteLock();
      }
    }
  }
}