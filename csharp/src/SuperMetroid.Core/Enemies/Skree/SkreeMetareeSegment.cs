namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The Skree's and Metaree's idling, wind-up, diving and stop animations as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="SkreeMetareeAnimations"/>.
/// </summary>
internal enum SkreeMetareeSegment
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
    /// <summary>The Skree idling animation.</summary>
    SkreeIdling,
    /// <summary>The Skree preparing attack animation.</summary>
    SkreePreparingAttack,
    /// <summary>The Skree diving animation.</summary>
    SkreeDiving,
    /// <summary>The loop.</summary>
    SkreeDivingLoop,
    /// <summary>The Skree stop animating animation. No authored spawn or branch enters it.</summary>
    SkreeStopAnimating,
}
