namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The dead Sidehopper's hop, idle and corpse animations and the dead Zoomer, Ripper and Skree corpse poses as typed animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="DeadTourianAnimations"/>.
/// </summary>
internal enum DeadTourianSegment
{
    /// <summary>The sidehopper hopping animation.</summary>
    SidehopperHopping,
    /// <summary>The sidehopper idle animation.</summary>
    SidehopperIdle,
    /// <summary>The sidehopper corpse animation.</summary>
    SidehopperCorpse,
    /// <summary>The sidehopper initially dead animation.</summary>
    SidehopperInitiallyDead,
    /// <summary>The Zoomer corpse lying on its back.</summary>
    ZoomerCorpseOnItsBack,
    /// <summary>The Zoomer corpse lying the right way up.</summary>
    ZoomerCorpseUpright,
    /// <summary>The Ripper corpse lying level.</summary>
    RipperCorpseLevel,
    /// <summary>The Ripper corpse lying tilted on a diagonal.</summary>
    RipperCorpseTilted,
    /// <summary>The Skree corpse.</summary>
    SkreeCorpse,
}
