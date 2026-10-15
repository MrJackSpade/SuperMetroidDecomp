namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The latches Bomb and Golden Torizo keep in enemy parameter 2. Bank $AA sets, clears and tests each bit on its own,
/// so they compose.
/// </summary>
[Flags]
internal enum TorizoBehaviorFlags : ushort
{
    /// <summary>No latch.</summary>
    None = 0,
    /// <summary>Bit 12: Golden Torizo has caught a Super Missile; later shots bypass the catch.</summary>
    CaughtSuperMissile = 0x1000,
    /// <summary>
    /// Bit 13: Golden Torizo is stunned. A Super Missile hit or another family's shot arms it, the stun-health branch
    /// consumes it, and the stun list's unmark command clears it.
    /// </summary>
    Stunned = 0x2000,
    /// <summary>Bit 14: Bomb Torizo's face is blown up; its faceless lists walk on from here.</summary>
    FaceBlownUp = 0x4000,
    /// <summary>Bit 15: Bomb Torizo's gut is blown up, which its first low-health interruption sets.</summary>
    GutBlownUp = 0x8000,
}
