using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The compatibility services Samus's projectiles consult where their glitch paths read past the data the original game
/// meant them to read (#1474, #1500): what the projectile system hands its trails, combos, firing, impacts, Chainsaw
/// fuse and crash echoes as it builds them. Lifetime: the installation's, shared by every projectile system.
/// </summary>
public sealed class SamusProjectileOverreads
{
    /// <summary>Keeps every decoded service the projectiles consult.</summary>
    /// <param name="trailStarts">Where a projectile trail starts when its read passes its placement list.</param>
    /// <param name="spaceTimeOverwrite">What Spacetime's palette copy overwrites in saved progression.</param>
    /// <param name="chainsawFuseShift">Where the Chainsaw beam's animation lands when its fuse speeds up.</param>
    /// <param name="chargeFlareDelays">The delays a charge-flare frame reads below the flare's programs.</param>
    /// <param name="postureAdmission">Whether the posture-transition admission admits weapons for a pose outside its table.</param>
    /// <param name="unauthoredBeamFiring">The speeds a Spazer and Plasma beam launches at and the cooldowns it waits.</param>
    /// <param name="drainedCrashDepartures">The angles a Shinespark crash finishing in a drained pose releases its echoes at.</param>
    internal SamusProjectileOverreads(
        TrailStartPastPlacements<SamusProjectileProgram, ProjectileTrailPlacementList, ProjectileTrailPlacement> trailStarts,
        SpaceTimeProgressionOverwrite<AnimationProgramCursor<SamusProjectileProgram>, ProgressionOverwrite> spaceTimeOverwrite,
        ChainsawFuseAnimationShift<AnimationProgramCursor<SamusProjectileProgram>> chainsawFuseShift,
        ChargeFlareDelaysBelowPrograms chargeFlareDelays,
        PostureAdmissionBeyondTransitions<SamusPoseId> postureAdmission,
        UnauthoredBeamFiring<SamusBeamCombination> unauthoredBeamFiring,
        DrainedCrashEchoDepartures<SamusPoseId, ShinesparkDepartureAngles> drainedCrashDepartures)
    {
        TrailStarts = Ensure.NotNull(trailStarts);
        SpaceTimeOverwrite = Ensure.NotNull(spaceTimeOverwrite);
        ChainsawFuseShift = Ensure.NotNull(chainsawFuseShift);
        ChargeFlareDelays = Ensure.NotNull(chargeFlareDelays);
        PostureAdmission = Ensure.NotNull(postureAdmission);
        UnauthoredBeamFiring = Ensure.NotNull(unauthoredBeamFiring);
        DrainedCrashDepartures = Ensure.NotNull(drainedCrashDepartures);
    }

    /// <summary>Where a projectile trail starts when its read passes its placement list.</summary>
    internal TrailStartPastPlacements<SamusProjectileProgram, ProjectileTrailPlacementList, ProjectileTrailPlacement> TrailStarts { get; }

    /// <summary>What Spacetime's palette copy overwrites in saved progression.</summary>
    internal SpaceTimeProgressionOverwrite<AnimationProgramCursor<SamusProjectileProgram>, ProgressionOverwrite> SpaceTimeOverwrite { get; }

    /// <summary>Where the Chainsaw beam's animation lands when its borrowed Power Bomb fuse speeds up.</summary>
    internal ChainsawFuseAnimationShift<AnimationProgramCursor<SamusProjectileProgram>> ChainsawFuseShift { get; }

    /// <summary>The delays a charge-flare frame reads when it wraps below the flare's programs.</summary>
    internal ChargeFlareDelaysBelowPrograms ChargeFlareDelays { get; }

    /// <summary>Whether the posture-transition weapon admission admits weapons for a pose outside its table.</summary>
    internal PostureAdmissionBeyondTransitions<SamusPoseId> PostureAdmission { get; }

    /// <summary>The speeds a Spazer and Plasma beam launches at and the cooldowns it waits, read past the firing tables.</summary>
    internal UnauthoredBeamFiring<SamusBeamCombination> UnauthoredBeamFiring { get; }

    /// <summary>The angles a Shinespark crash finishing in a drained pose releases its echoes at, read past its table.</summary>
    internal DrainedCrashEchoDepartures<SamusPoseId, ShinesparkDepartureAngles> DrainedCrashDepartures { get; }
}
