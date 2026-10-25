using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Compiled bank-$86 mechanics for the two enemy projectiles used by the initial
/// Ceres elevator arrival. Spritemap payloads remain presentation data in bank $8D.
/// </summary>
internal static class CeresElevatorArrivalDefinitions
{

    /// <summary>$86:A387, moving pad spawned first into enemy-projectile slot $22.</summary>
    internal static readonly CeresElevatorProjectileDefinition MovingPad = new(
        Program: CeresElevatorArrivalProgram.Pad);

    /// <summary>$86:A395, stationary level-data concealer spawned into slot $20.</summary>
    internal static readonly CeresElevatorProjectileDefinition StationaryPlatform = new(
        Program: CeresElevatorArrivalProgram.Platform);

    /// <summary>The graphics index both arrival projectiles start with: zero, as <c>$86:A301</c> clears it.</summary>
    internal const ushort StartingGraphicsIndex = 0;

    /// <summary>$86:A316 seeds the moving pad's pre-instruction delay to sixty frames.</summary>
    internal const ushort MovingPadWaitFrames = 60;

    /// <summary>$86:A2FE places the moving pad twenty-eight pixels below Samus.</summary>
    internal const ushort MovingPadYOffset = 28;

    /// <summary>$86:A31B places the stationary platform at world Y=97.</summary>
    internal const ushort StationaryPlatformY = 97;

    /// <summary>$86:A34D clamps Samus to world Y=72 at the end of the descent.</summary>
    internal const ushort LandingSamusY = 72;

    /// <summary>$86:A345 compares the incremented Samus Y against 73 before clamping.</summary>
    internal const ushort LandingThresholdY = 73;

    /// <summary>The composition each arrival pose shows.</summary>
    internal static EnemyProjectileFrameId Frame(CeresElevatorArrivalPose pose) => pose switch
    {
        // Spritemap_CeresElevatorPad_0, shown by $86:A28D.
        CeresElevatorArrivalPose.PadFirst => EnemyProjectileFrameId.CeresElevatorPad0,
        // Spritemap_CeresElevatorPad_1, shown by $86:A291.
        CeresElevatorArrivalPose.PadSecond => EnemyProjectileFrameId.CeresElevatorPad1,
        // Spritemap_CeresElevatorPlatform, shown by $86:A299.
        CeresElevatorArrivalPose.Platform => EnemyProjectileFrameId.CeresElevatorPlatform,
        _ => throw new ArgumentOutOfRangeException(nameof(pose), pose, "Undefined Ceres elevator arrival pose."),
    };
}
