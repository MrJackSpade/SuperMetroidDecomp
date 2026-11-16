namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The Skree's and Metaree's idling, wind-up, diving and stop animations (#641): one segment per animation a spawn, branch
/// or wake enters, and the spin each dive loops after its entry cue (#2276), in authored order. Their steps are
/// <see cref="SkreeMetareeAnimations"/>.
/// </summary>
internal enum SkreeMetareeSegment
{
    /// <summary>The metaree idling animation.</summary>
    MetareeIdling,
    /// <summary>The metaree preparing attack animation.</summary>
    MetareePreparingAttack,
    /// <summary>The metaree diving animation's entry: the cue that has it processed off screen.</summary>
    MetareeDiving,
    /// <summary>The Metaree's diving spin, looped.</summary>
    MetareeDivingSpin,
    /// <summary>The Skree idling animation.</summary>
    SkreeIdling,
    /// <summary>The Skree preparing attack animation.</summary>
    SkreePreparingAttack,
    /// <summary>The Skree diving animation's entry: the cue that has it processed off screen.</summary>
    SkreeDiving,
    /// <summary>The Skree's diving spin, looped.</summary>
    SkreeDivingSpin,
}
