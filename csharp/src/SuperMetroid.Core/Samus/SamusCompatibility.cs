namespace SuperMetroid.Core.Samus;

/// <summary>
/// The compatibility services one Samus owns. Whoever builds a Samus (the room loader for a saved, a Ceres or a post-Ceres
/// Samus, the attract demo, the opening cinematic's flashbacks) creates a fresh set with her, and the set lives exactly as
/// long as she does: a new Samus starts with no recorded movement, no spark acceleration, no shared counts, no charge and no effect waits.
/// </summary>
internal sealed class SamusCompatibility
{
    /// <summary>The motion her shots inherit when they launch.</summary>
    public required ProjectileLaunchInheritance LaunchInheritance { get; init; }

    /// <summary>Her shinespark's upward acceleration, which the suit pickup's light beam overwrites.</summary>
    public required SuitPickupSparkCarryover SparkCarryover { get; init; }

    /// <summary>The count Crystal Flash's ammunition drain, Draygon's escape and the suit pickup's stage share.</summary>
    public required SharedEscapeAndDrainCount EscapeAndDrainCount { get; init; }

    /// <summary>The count her hurt flash runs on.</summary>
    public required SharedHurtFlashCount HurtFlash { get; init; }

    /// <summary>The beam charge as her movement, bombs and sounds read it.</summary>
    public required SharedBeamChargeCount BeamChargeCount { get; init; }

    /// <summary>The waits her breath bubbles and lava spray keep for the effects that shared their places.</summary>
    public required BreathBubbleAndLavaSprayWaits EffectWaits { get; init; }

    /// <summary>A fresh set for a new Samus.</summary>
    public static SamusCompatibility ForNewSamus() => new()
    {
        LaunchInheritance = new ProjectileLaunchInheritance(),
        SparkCarryover = new SuitPickupSparkCarryover(),
        EscapeAndDrainCount = new SharedEscapeAndDrainCount(),
        HurtFlash = new SharedHurtFlashCount(),
        BeamChargeCount = new SharedBeamChargeCount(),
        EffectWaits = new BreathBubbleAndLavaSprayWaits(),
    };
}
