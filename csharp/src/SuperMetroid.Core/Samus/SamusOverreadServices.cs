using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// The compatibility services that reproduce what Samus's glitch paths read past the data the original game meant them
/// to read (#1474), as the import pipeline decoded them from the player's cartridge (owner question 3). The session's
/// gameplay runtime receives them with the installed content and hands each to the owner that consults it when it builds
/// that owner. Lifetime: the installation's, shared by every runtime of the session.
/// </summary>
public sealed class SamusOverreadServices
{
    /// <summary>Keeps every decoded service.</summary>
    /// <param name="animationContinuation">What a Samus animation frame past its pose's program reads.</param>
    /// <param name="trailStarts">Where a projectile trail starts when its read passes its placement list.</param>
    /// <param name="spaceTimeOverwrite">What SpaceTime's palette copy overwrites in saved progression.</param>
    /// <param name="chainsawFuseShift">Where the Chainsaw beam's animation lands when its fuse speeds up.</param>
    /// <param name="chargeFlareDelays">The delays a charge-flare frame reads below the flare's programs.</param>
    /// <param name="postureAdmission">Whether the posture-transition admission admits weapons for a pose outside its table.</param>
    /// <param name="speedBoosterColours">The suit colours Gravity Speed Booster copies from past its cycle.</param>
    internal SamusOverreadServices(
        LaidOutProgramContinuation<SamusPoseId> animationContinuation,
        TrailStartPastPlacements<SamusProjectileProgram, ProjectileTrailPlacementList, ProjectileTrailPlacement> trailStarts,
        SpaceTimeProgressionOverwrite<AnimationProgramCursor<SamusProjectileProgram>> spaceTimeOverwrite,
        ChainsawFuseAnimationShift<AnimationProgramCursor<SamusProjectileProgram>> chainsawFuseShift,
        ChargeFlareDelaysBelowPrograms chargeFlareDelays,
        PostureAdmissionBeyondTransitions<SamusPoseId> postureAdmission,
        SpeedBoosterOverrunColours<SpeedBoosterPaletteOverrun, RgbColor> speedBoosterColours)
    {
        AnimationContinuation = Ensure.NotNull(animationContinuation);
        TrailStarts = Ensure.NotNull(trailStarts);
        SpaceTimeOverwrite = Ensure.NotNull(spaceTimeOverwrite);
        ChainsawFuseShift = Ensure.NotNull(chainsawFuseShift);
        ChargeFlareDelays = Ensure.NotNull(chargeFlareDelays);
        PostureAdmission = Ensure.NotNull(postureAdmission);
        SpeedBoosterColours = Ensure.NotNull(speedBoosterColours);
    }

    /// <summary>What a Samus animation frame past its pose's program reads, by a pose that animates by each program.</summary>
    internal LaidOutProgramContinuation<SamusPoseId> AnimationContinuation { get; }

    /// <summary>Where a projectile trail starts when its read passes its placement list.</summary>
    internal TrailStartPastPlacements<SamusProjectileProgram, ProjectileTrailPlacementList, ProjectileTrailPlacement> TrailStarts { get; }

    /// <summary>What SpaceTime's palette copy overwrites in saved progression.</summary>
    internal SpaceTimeProgressionOverwrite<AnimationProgramCursor<SamusProjectileProgram>> SpaceTimeOverwrite { get; }

    /// <summary>Where the Chainsaw beam's animation lands when its borrowed Power Bomb fuse speeds up.</summary>
    internal ChainsawFuseAnimationShift<AnimationProgramCursor<SamusProjectileProgram>> ChainsawFuseShift { get; }

    /// <summary>The delays a charge-flare frame reads when it wraps below the flare's programs.</summary>
    internal ChargeFlareDelaysBelowPrograms ChargeFlareDelays { get; }

    /// <summary>Whether the posture-transition weapon admission admits weapons for a pose outside its table.</summary>
    internal PostureAdmissionBeyondTransitions<SamusPoseId> PostureAdmission { get; }

    /// <summary>The suit colours Gravity Speed Booster copies from past its cycle.</summary>
    internal SpeedBoosterOverrunColours<SpeedBoosterPaletteOverrun, RgbColor> SpeedBoosterColours { get; }
}
