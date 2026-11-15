namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// The phase of Crocomire's fight: what his fight does each time his body animation's fight cue fires. Its values are
/// arbitrary; nothing reads or orders them. A defeated room's corpse fights in no phase (#1939).
/// </summary>
public enum CrocomirePhase
{
    /// <summary>Lies dormant until Samus comes within 224 pixels, then waits for his first hit.</summary>
    Sleeping,
    /// <summary>Steps forward, stepping back on a hit that queued steps, and charges once he is near the spike wall.</summary>
    SteppingForward,
    /// <summary>Spits a volley of shots, one at each fight cue, then steps forward; a hit steps him back.</summary>
    ProjectileAttack,
    /// <summary>Charges toward the spike wall until a hit steps him back.</summary>
    NearSpikeWallCharge,
    /// <summary>Steps back once for each queued step, then steps forward again.</summary>
    SteppingBack,
    /// <summary>Backs off from the spike wall until he stands clear of it, then steps forward.</summary>
    BackingOffSpikeWall,
    /// <summary>Roars until his first hit steps him back into waiting for the second.</summary>
    WaitingForFirstDamage,
    /// <summary>Roars until his second hit steps him back into the fight.</summary>
    WaitingForSecondDamage,
    /// <summary>Charges left after a Power Bomb, one queued step at each fight cue, until fewer than two remain.</summary>
    PowerBombCharge,
}
