namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What a cue of Golden Torizo's super missile and eye beam does that only the behaviour keeping their Torizo runs: aim or throw the missile, or end the floor impact's wait.</summary>
internal enum GoldenTorizoWeaponCueKind
{
    /// <summary>Aims a thrown super missile at Samus.</summary>
    AimSuperMissileTowardSamus,
    /// <summary>Aims a thrown super missile away from Samus.</summary>
    AimSuperMissileAwayFromSamus,
    /// <summary>Throws the held super missile.</summary>
    ThrowSuperMissile,
    /// <summary>The eye beam's floor impact has waited out its pause (#2411): once Golden Torizo's eye-beam explosions are enabled, it starts its flames; until then it waits again.</summary>
    FloorImpactWaited,
}
