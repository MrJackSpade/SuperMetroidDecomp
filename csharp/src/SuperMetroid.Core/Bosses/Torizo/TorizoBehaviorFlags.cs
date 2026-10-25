namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The latches Bomb and Golden Torizo keep. Bank $AA sets, clears and tests each on its own, so they compose. The
/// cartridge keeps them in the top four bits of enemy parameter 2; where, is <see cref="TorizoParameterFlagCodec"/>'s
/// knowledge, and the values here are arbitrary bits (#1413).
/// </summary>
[Flags]
internal enum TorizoBehaviorFlags
{
    /// <summary>Golden Torizo has caught a Super Missile; later shots bypass the catch.</summary>
    CaughtSuperMissile = 1,
    /// <summary>
    /// Golden Torizo is stunned. A Super Missile hit or another family's shot arms it, the stun-health branch consumes
    /// it, and the stun list's unmark command clears it.
    /// </summary>
    Stunned = 2,
    /// <summary>Bomb Torizo's face is blown up; its faceless lists walk on from here.</summary>
    FaceBlownUp = 4,
    /// <summary>Bomb Torizo's gut is blown up, which its first low-health interruption sets.</summary>
    GutBlownUp = 8,
}
