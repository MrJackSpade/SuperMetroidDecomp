using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// The compatibility services one Samus owns. Whoever builds a Samus (the room loader for a saved, a Ceres or a post-Ceres
/// Samus, the attract demo, the opening cinematic's flashbacks) creates a fresh set with her, and the set lives exactly as
/// long as she does: a new Samus starts with no recorded movement, no spark acceleration, no crash orbit travel, no shared
/// counts and no effect waits.
/// </summary>
internal sealed class SamusCompatibility
{
    /// <summary>The motion her shots inherit when they launch.</summary>
    public required ProjectileLaunchInheritance LaunchInheritance { get; init; }

    /// <summary>Her shinespark's upward acceleration, which the suit pickup's light beam overwrites.</summary>
    public required SuitPickupSparkCarryover SparkCarryover { get; init; }

    /// <summary>How far her shinespark crash's orbit has turned, which the first echo a crash releases overwrites.</summary>
    public required CrashOrbitTravelCarryover CrashOrbitTravel { get; init; }

    /// <summary>The count Crystal Flash's ammunition drain, Draygon's escape and the suit pickup's stage share.</summary>
    public required SharedEscapeAndDrainCount EscapeAndDrainCount { get; init; }

    /// <summary>The count her hurt flash runs on.</summary>
    public required SharedHurtFlashCount HurtFlash { get; init; }

    /// <summary>The waits her breath bubbles and lava spray keep for the effects that shared their places.</summary>
    public required BreathBubbleAndLavaSprayWaits EffectWaits { get; init; }

    /// <summary>The water speeds Draygon's hold and special movement move her by in air, where they have none of their own.</summary>
    public required WaterSpeedsInAir<SamusMovementType, SamusHorizontalSpeeds> WaterSpeedsInAir { get; init; }

    /// <summary>A fresh set for a new Samus.</summary>
    public static SamusCompatibility ForNewSamus() => new()
    {
        LaunchInheritance = new ProjectileLaunchInheritance(),
        SparkCarryover = new SuitPickupSparkCarryover(),
        CrashOrbitTravel = new CrashOrbitTravelCarryover(),
        EscapeAndDrainCount = new SharedEscapeAndDrainCount(),
        HurtFlash = new SharedHurtFlashCount(),
        EffectWaits = new BreathBubbleAndLavaSprayWaits(),
        WaterSpeedsInAir = new WaterSpeedsInAir<SamusMovementType, SamusHorizontalSpeeds>(SamusHorizontalSpeedDefinitions.WaterSpeedsInAir),
    };
}
