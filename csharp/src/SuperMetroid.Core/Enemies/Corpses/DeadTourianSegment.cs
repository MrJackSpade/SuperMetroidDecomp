namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The dead Sidehopper's hop, idle and corpse animations and the dead Zoomer, Ripper and Skree corpse poses as typed animations (#641).
/// Their steps are <see cref="DeadTourianAnimations"/>.
/// </summary>
internal enum DeadTourianSegment
{
    /// <summary>
    /// The dead Sidehopper bobbing on bent and crouched legs, with a long crouch between, before its hop ends.
    /// </summary>
    SidehopperHopping,
    /// <summary>The dead Sidehopper standing on straight legs, held.</summary>
    SidehopperIdle,
    /// <summary>The dead Sidehopper sprawled, held.</summary>
    SidehopperCorpse,
    /// <summary>The dead Sidehopper sprawled with its legs up, held, as one already dead lies.</summary>
    SidehopperInitiallyDead,
    /// <summary>The Zoomer corpse lying on its back.</summary>
    ZoomerCorpseOnItsBack,
    /// <summary>The Zoomer corpse lying the right way up.</summary>
    ZoomerCorpseUpright,
    /// <summary>The Ripper corpse lying level.</summary>
    RipperCorpseLevel,
    /// <summary>The Ripper corpse lying tilted on a diagonal.</summary>
    RipperCorpseTilted,
    /// <summary>The dead Skree's corpse, held.</summary>
    SkreeCorpse,
}
