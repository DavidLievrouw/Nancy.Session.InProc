namespace Nancy.Session.InProc.InProcSessionsManagement.Cache {
  using System.Threading;

  internal class HeldReadLock : HeldLock {
    public HeldReadLock(ReaderWriterLockSlim wrappedLock) : base(wrappedLock) {}

    protected override void Acquire() {
      WrappedLock.EnterReadLock();
    }

    public override void Dispose() {
      if (WrappedLock.IsReadLockHeld) {
        WrappedLock.ExitReadLock();
      }
    }
  }
}