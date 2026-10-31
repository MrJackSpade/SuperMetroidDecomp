namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The Skree's and Metaree's idling, wind-up, diving and stop animations as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="SkreeMetareeAnimations"/>.
/// </summary>
internal enum SkreeMetareeProgram
{
    /// <summary>The metaree idling animation.</summary>
    MetareeIdling,
    /// <summary>The metaree preparing attack animation.</summary>
    MetareePreparingAttack,
    /// <summary>The metaree diving animation.</summary>
    MetareeDiving,
    /// <summary>The loop.</summary>
    MetareeDivingLoop,
    /// <summary>The metaree stop animating animation. No authored spawn or branch enters it.</summary>
    MetareeStopAnimating,
    /// <summary>The skree idling animation.</summary>
    SkreeIdling,
    /// <summary>The skree preparing attack animation.</summary>
    SkreePreparingAttack,
    /// <summary>The skree diving animation.</summary>
    SkreeDiving,
    /// <summary>The loop.</summary>
    SkreeDivingLoop,
    /// <summary>The skree stop animating animation. No authored spawn or branch enters it.</summary>
    SkreeStopAnimating,
}
