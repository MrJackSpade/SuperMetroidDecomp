namespace SuperMetroid.Core.Rooms;

/// <summary>The control locks room stations put on Samus and release (#1395). Samus's control implements it.</summary>
public interface IRoomSamusControl
{
    /// <summary>Whether Samus's movement handler is locked.</summary>
    bool AlphaLocked { get; }

    /// <summary>Returns Samus to normal control.</summary>
    void Unlock();

    /// <summary>Locks Samus in place without animating her, as the n00b tube's break does.</summary>
    void LockStationary();

    /// <summary>Locks Samus's handlers, as a save station does.</summary>
    void LockHandlers();

    /// <summary>Locks Samus into a refill station's handler.</summary>
    void LockIntoRefillStation();
}
