using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// The compatibility services that reproduce what Samus's glitch paths read past the data the original game meant them
/// to read (#1474), as the import pipeline decoded them from the player's game data (owner question 3). The session's
/// gameplay runtime receives them with the installed content and hands each to the owner that consults it when it builds
/// that owner. Lifetime: the installation's, shared by every runtime of the session.
/// </summary>
public sealed class SamusOverreadServices
{
    /// <summary>Keeps every decoded service.</summary>
    /// <param name="parts">The services, as the import pipeline decoded them.</param>
    internal SamusOverreadServices(SamusOverreadServiceParts parts)
    {
        Ensure.NotNull(parts);
        AnimationContinuation = Ensure.NotNull(parts.AnimationContinuation);
        TrailStarts = Ensure.NotNull(parts.TrailStarts);
        SpaceTimeOverwrite = Ensure.NotNull(parts.SpaceTimeOverwrite);
        ChainsawFuseShift = Ensure.NotNull(parts.ChainsawFuseShift);
        ChargeFlareDelays = Ensure.NotNull(parts.ChargeFlareDelays);
        PostureAdmission = Ensure.NotNull(parts.PostureAdmission);
        SpeedBoosterColours = Ensure.NotNull(parts.SpeedBoosterColours);
        UnauthoredBeamSpeeds = Ensure.NotNull(parts.UnauthoredBeamSpeeds);
        DrainedCrashDepartures = Ensure.NotNull(parts.DrainedCrashDepartures);
        ChainsawColours = Ensure.NotNull(parts.ChainsawColours);
    }

    /// <summary>What a Samus animation frame past its pose's program reads, by a pose that animates by each program.</summary>
    internal LaidOutProgramContinuation<SamusPoseId> AnimationContinuation { get; }

    /// <summary>Where a projectile trail starts when its read passes its placement list.</summary>
    internal TrailStartPastPlacements<SamusProjectileProgram, ProjectileTrailPlacementList, ProjectileTrailPlacement> TrailStarts { get; }

    /// <summary>What SpaceTime's palette copy overwrites in saved progression.</summary>
    internal SpaceTimeProgressionOverwrite<AnimationProgramCursor<SamusProjectileProgram>, ProgressionOverwrite> SpaceTimeOverwrite { get; }

    /// <summary>Where the Chainsaw beam's animation lands when its borrowed Power Bomb fuse speeds up.</summary>
    internal ChainsawFuseAnimationShift<AnimationProgramCursor<SamusProjectileProgram>> ChainsawFuseShift { get; }

    /// <summary>The delays a charge-flare frame reads when it wraps below the flare's programs.</summary>
    internal ChargeFlareDelaysBelowPrograms ChargeFlareDelays { get; }

    /// <summary>Whether the posture-transition weapon admission admits weapons for a pose outside its table.</summary>
    internal PostureAdmissionBeyondTransitions<SamusPoseId> PostureAdmission { get; }

    /// <summary>The suit colours Gravity Speed Booster copies from past its cycle.</summary>
    internal SpeedBoosterOverrunColours<SpeedBoosterPaletteOverrun, RgbColor> SpeedBoosterColours { get; }

    /// <summary>The speeds a Spazer and Plasma beam launches at, read past the beam speed table.</summary>
    internal UnauthoredBeamLaunchSpeeds<SamusBeamCombination> UnauthoredBeamSpeeds { get; }

    /// <summary>The angles a Shinespark crash finishing in a drained pose releases its echoes at, read past its table.</summary>
    internal DrainedCrashEchoDepartures<SamusPoseId, ShinesparkDepartureAngles> DrainedCrashDepartures { get; }

    /// <summary>The colours the Chainsaw beam's palette loader reads past its table.</summary>
    internal ChainsawBeamColours<RgbColor> ChainsawColours { get; }
}
