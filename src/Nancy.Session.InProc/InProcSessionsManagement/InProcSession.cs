namespace Nancy.Session.InProc.InProcSessionsManagement {
  using System;
  using System.Collections;
  using System.Collections.Generic;

  internal class InProcSession : ISession {
    public InProcSession(SessionId id, ISession wrappedSession, DateTime lastSave, TimeSpan timeout) {
      if (id == null) throw new ArgumentNullException("id");
      if (wrappedSession == null) throw new ArgumentNullException("wrappedSession");
      if (id.IsEmpty) throw new ArgumentException("The specified session id cannot be empty", "id");
      WrappedSession = wrappedSession;
      Id = id;
      LastSave = lastSave;
      Timeout = timeout;
    }

    public SessionId Id { get; private set; }

    public DateTime LastSave { get; private set; }

    public TimeSpan Timeout { get; private set; }

    internal ISession WrappedSession { get; private set; }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
      return WrappedSession.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    public int Count {
      get { return WrappedSession.Count; }
    }

    public void DeleteAll() {
      WrappedSession.DeleteAll();
    }

    public void Delete(string key) {
      WrappedSession.Delete(key);
    }

    public object this[string key] {
      get { return WrappedSession[key]; }
      set { WrappedSession[key] = value; }
    }

    public bool HasChanged {
      get { return WrappedSession.HasChanged; }
    }

    public bool IsExpired(DateTime nowUtc) {
      return nowUtc > LastSave.Add(Timeout);
    }

    public override bool Equals(object obj) {
      var otherSession = obj as InProcSession;
      if (otherSession == null) {
        return false;
      }

      return Id == otherSession.Id;
    }

    public override int GetHashCode() {
      return Id.GetHashCode();
    }
  }
}