namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The animation segments of Golden Torizo's caught super missile and its eye beam (#641, #1788, #2235, #2554): each animation begins at the
/// segment named for what it shows; the runs it repeats, loops on or moves on to are segments of their own
/// (<see cref="GoldenTorizoWeaponAnimations"/>).
/// </summary>
internal enum GoldenTorizoWeaponSegment
{
    /// <summary>A caught super missile thrown right: held, then aimed at Samus and thrown, spinning for good.</summary>
    SuperMissileRight,
    /// <summary>A thrown super missile spinning, thrown right: its eight poses, looping.</summary>
    SuperMissileSpinningRight,
    /// <summary>A caught super missile thrown left: held, then aimed away from Samus and thrown, spinning for good.</summary>
    SuperMissileLeft,
    /// <summary>A thrown super missile spinning, thrown left: its eight poses, looping.</summary>
    SuperMissileSpinningLeft,
    /// <summary>A thrown super missile exploding: it widens, stops, turns harmful, plays its sound and six poses, then is deleted.</summary>
    SuperMissileImpact,
    /// <summary>The eye beam bursting on a wall: it stops, plays its five poses, then is deleted.</summary>
    EyeBeamWallImpact,
    /// <summary>
    /// The eye beam reaching the floor: it stops and waits unseen in <see cref="EyeBeamFloorImpactWaiting"/> until Golden
    /// Torizo's eye-beam explosions are enabled, then bursts into <see cref="EyeBeamFloorFlames"/>.
    /// </summary>
    EyeBeamFloorImpact,
    /// <summary>The eye beam's floor impact waits unseen for its beat, then marks its wait over, again while Golden Torizo's eye-beam explosions are disabled.</summary>
    EyeBeamFloorImpactWaiting,
    /// <summary>The eye beam's floor impact bursts into flames: its impact sound and six flame poses, harmful from its third, then deletion.</summary>
    EyeBeamFloorFlames,
    /// <summary>The eye beam in flight: its five poses, for good.</summary>
    EyeBeam,
}
