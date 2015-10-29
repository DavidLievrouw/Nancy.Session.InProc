namespace Nancy.Session.InProc.InProcSessionsManagement.Cache {
  using System.Threading;

  internal class HeldUpgradeableReadLock : HeldLock {
    public HeldUpgradeableReadLock(ReaderWriterLockSlim wrappedLock) : base(wrappedLock) {}

    protected override void Acquire() {
      WrappedLock.EnterUpgradeableReadLock();
    }

    public override void Dispose() {
      if (WrappedLock.IsUpgradeableReadLockHeld) {
        WrappedLock.ExitUpgradeableReadLock();
      }
    }
  }
}