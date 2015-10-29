namespace Nancy.Session.InProc.InProcSessionsManagement.Cache {
  using System;
  using System.Threading;

  internal abstract class HeldLock : IHeldLock {
    protected readonly ReaderWriterLockSlim WrappedLock;

    protected HeldLock(ReaderWriterLockSlim wrappedLock) {
      if (wrappedLock == null) throw new ArgumentNullException("wrappedLock");
      WrappedLock = wrappedLock;
      Acquire();
    }

    public abstract void Dispose();

    protected abstract void Acquire();
  }
}