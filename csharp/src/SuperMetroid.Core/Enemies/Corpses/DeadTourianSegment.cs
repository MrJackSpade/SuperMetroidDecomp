namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The dead Sidehopper's hop, idle and corpse animations and the dead Zoomer, Ripper and Skree corpse poses as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
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
    /// <summary>The Zoomer corpse 0 animation.</summary>
    ZoomerCorpse0,
    /// <summary>The Zoomer corpse 1 animation.</summary>
    ZoomerCorpse1,
    /// <summary>The Zoomer corpse 2 animation.</summary>
    ZoomerCorpse2,
    /// <summary>The Ripper corpse 0 animation.</summary>
    RipperCorpse0,
    /// <summary>The Ripper corpse 1 animation.</summary>
    RipperCorpse1,
    /// <summary>The Skree corpse 0 animation.</summary>
    SkreeCorpse0,
    /// <summary>The Skree corpse 1 animation.</summary>
    SkreeCorpse1,
    /// <summary>The Skree corpse 2 animation.</summary>
    SkreeCorpse2,
}
