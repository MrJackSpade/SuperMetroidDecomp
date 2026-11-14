namespace SuperMetroid.Core.Rooms;

/// <summary>The control locks room stations put on Samus and release (#1395). Samus's control implements it.</summary>
public interface IRoomSamusControl
{
    /// <summary>Whether Samus's input is locked.</summary>
    bool InputLocked { get; }

    /// <summary>Returns Samus to normal control.</summary>
    void Unlock();

    /// <summary>Locks Samus in place without animating her, as the n00b tube's break does.</summary>
    void LockStationary();

    /// <summary>
    /// Locks the player out, as a save station does: the player's input no longer moves Samus, and nothing moves her but
    /// an elevator she rides.
    /// </summary>
    void LockOutPlayer();

    /// <summary>
    /// Locks Samus at a refill or map station: the player is locked out and nothing moves or animates her until the
    /// pause menu closes.
    /// </summary>
    void LockAtStation();
}
