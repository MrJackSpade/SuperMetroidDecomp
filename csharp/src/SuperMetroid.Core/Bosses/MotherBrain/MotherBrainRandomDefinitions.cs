namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The random choices of Mother Brain's fight, her ascent and the baby metroid's scenes: holds, dust, the hand beam's
/// scatter and the rolls her late phases make of the random sample.
/// </summary>
internal static class MotherBrainRandomDefinitions
{
    /// <summary>The baby metroid's ambient cry roll on its way to Samus: the sample's bits 0-11; 4000 or more cries.</summary>
    internal static readonly RandomSampleField BabyMetroidCryRoll = new(0, 12);

    /// <summary>The Shitroid's hover roll: the sample's bits 0-11; 4064 or more restarts its hover.</summary>
    internal static readonly RandomSampleField ShitroidHoverRoll = new(0, 12);

    /// <summary>Whether a dust cloud of Mother Brain's ascent is the short big cloud: the sample's bit eight, from its upper random byte.</summary>
    internal static readonly RandomSampleField AscentDustBig = new(8, 1);

    /// <summary>The phase-three neutral hold roll: the sample's bits 0-11; below 3776 holds the neutral pose again.</summary>
    internal static readonly RandomSampleField NeutralPhase3HoldRoll = new(0, 12);

    /// <summary>The dying drool roll: the sample's bits 0-11; 4064 or more bursts into more drool.</summary>
    internal static readonly RandomSampleField DroolBurstRoll = new(0, 12);

    /// <summary>
    /// The finish-off attack roll while Samus is drained: the sample's bits 0-11; below 4000 does nothing, and below 4080
    /// chooses onion rings over a bomb.
    /// </summary>
    internal static readonly RandomSampleField FinishOffAttackRoll = new(0, 12);

    /// <summary>A rotting corpse's dust offset, 0 through 31 pixels added to the brain's X less sixteen: the sample's bits 0-4.</summary>
    internal static readonly RandomSampleField CorpseDustOffset = new(0, 5);

    /// <summary>Whether phase three's forward inch waits two updates longer: the sample's bit one.</summary>
    internal static readonly RandomSampleField ForwardInchLongerDelay = new(1, 1);

    /// <summary>A fired hand beam's scatter speed, 0 through 7 whole pixels: the sample's bits 8-10, from its upper random byte.</summary>
    internal static readonly RandomSampleField HandBeamScatterSpeed = new(8, 3);

    /// <summary>The roll her attack choice compares against its thresholds: the sample's low byte.</summary>
    internal static readonly RandomSampleField AttackRoll = new(0, 8);

    /// <summary>A fired hand beam's scatter turn, in byte-angle units: the sample's low byte.</summary>
    internal static readonly RandomSampleField HandBeamScatterTurn = new(0, 8);

    /// <summary>A turret's wait before it chooses another direction, raised to its minimum: the sample's low byte.</summary>
    internal static readonly RandomSampleField TurretRotationWait = new(0, 8);

    /// <summary>A turret's pause between bullets, raised to its minimum: the sample's low byte.</summary>
    internal static readonly RandomSampleField TurretFiringWait = new(0, 8);
}
