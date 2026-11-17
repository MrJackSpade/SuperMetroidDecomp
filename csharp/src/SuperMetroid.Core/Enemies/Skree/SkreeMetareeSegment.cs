namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The Skree's and Metaree's idling, wind-up, diving and stop animations (#641): one segment per animation a spawn, branch
/// or wake enters, and the spin each dive loops after its entry cue (#2276), in authored order. Their steps are
/// <see cref="SkreeMetareeAnimations"/>.
/// </summary>
internal enum SkreeMetareeSegment
{
    /// <summary>A Metaree turning from facing the viewer to edge-on and back, repeating.</summary>
    MetareeIdling,
    /// <summary>A Metaree facing the viewer, then flaring its wings, ready to dive.</summary>
    MetareePreparingAttack,
    /// <summary>The metaree diving animation's entry: the cue that has it processed off screen.</summary>
    MetareeDiving,
    /// <summary>The Metaree's diving spin, looped.</summary>
    MetareeDivingSpin,
    /// <summary>A Skree turning from facing the viewer to edge-on and back, repeating.</summary>
    SkreeIdling,
    /// <summary>A Skree facing the viewer, then flaring its wings, ready to dive.</summary>
    SkreePreparingAttack,
    /// <summary>The Skree diving animation's entry: the cue that has it processed off screen.</summary>
    SkreeDiving,
    /// <summary>The Skree's diving spin, looped.</summary>
    SkreeDivingSpin,
}
