namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>One departing crash-echo projectile's drawing enable and position.</summary>
/// <param name="Active">Speed-echo drawing enable, independent of current projectile-slot ownership; replacing the projectile need not clear these drawing words.</param>
/// <param name="Position">Whole room-pixel echo position with sixteen-bit wrapping; slot-three storage also aliases the crash angular-delta (X) and angular-travel (Y) words.</param>
public readonly record struct ShinesparkReleasedEcho(
    bool Active,
    Position16<RoomPixels> Position);
