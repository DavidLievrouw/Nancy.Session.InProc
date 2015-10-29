namespace Nancy.Session.InProc.InProcSessionsManagement {
  using System;

  /// <summary>
  ///   Represents a unique identifier for an in-proc session.
  /// </summary>
  public class SessionId : IEquatable<Guid> {
    /// <summary>
    ///   Creates a new instance of the <see cref="SessionId" /> class.
    /// </summary>
    internal SessionId(Guid value, bool isNew) {
      Value = value;
      IsNew = isNew;
    }

    /// <summary>
    ///   Gets the actual unique identifier of the session.
    /// </summary>
    public Guid Value { get; private set; }

    /// <summary>
    ///   Gets a value indicating whether this session identifier is for a new session.
    /// </summary>
    public bool IsNew { get; private set; }

    /// <summary>
    ///   Gets a value indicating whether this is an empty session identifier.
    /// </summary>
    public bool IsEmpty {
      get { return Value == Guid.Empty; }
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
    public bool Equals(Guid other) {
      return Value == other;
    }

    /// <summary>
    /// Equality operator for the <see cref="SessionId" /> class.
    /// </summary>
    public static bool operator ==(SessionId x, SessionId y) {
      if (ReferenceEquals(x, y)) {
        return true;
      }
      if ((object) x == null || (object) y == null) {
        return false;
      }
      return (x.Value == y.Value);
    }

    /// <summary>
    /// Inequality operator for the <see cref="SessionId" /> class.
    /// </summary>
    public static bool operator !=(SessionId x, SessionId y) {
      return !(x == y);
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    public override string ToString() {
      return IsNew
        ? Value + " (new)"
        : Value.ToString();
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    public override bool Equals(object obj) {
      var otherSessionId = obj as SessionId;
      if (otherSessionId == null) {
        return false;
      }

      return Value == otherSessionId.Value;
    }

    /// <summary>
    /// Serves as the hash function.
    /// </summary>
    public override int GetHashCode() {
      return Value.GetHashCode();
    }
  }
}