namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The dead Sidehopper's hop, idle and corpse animations and the dead Zoomer, Ripper and Skree corpse poses as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="DeadTourianAnimations"/>.
/// </summary>
internal enum DeadTourianProgram
{
    /// <summary>The sidehopper hopping animation.</summary>
    SidehopperHopping,
    /// <summary>The sidehopper idle animation.</summary>
    SidehopperIdle,
    /// <summary>The sidehopper corpse animation.</summary>
    SidehopperCorpse,
    /// <summary>The sidehopper initially dead animation.</summary>
    SidehopperInitiallyDead,
    /// <summary>The zoomer corpse 0 animation.</summary>
    ZoomerCorpse0,
    /// <summary>The zoomer corpse 1 animation.</summary>
    ZoomerCorpse1,
    /// <summary>The zoomer corpse 2 animation.</summary>
    ZoomerCorpse2,
    /// <summary>The ripper corpse 0 animation.</summary>
    RipperCorpse0,
    /// <summary>The ripper corpse 1 animation.</summary>
    RipperCorpse1,
    /// <summary>The skree corpse 0 animation.</summary>
    SkreeCorpse0,
    /// <summary>The skree corpse 1 animation.</summary>
    SkreeCorpse1,
    /// <summary>The skree corpse 2 animation.</summary>
    SkreeCorpse2,
}
