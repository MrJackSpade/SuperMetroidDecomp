using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// The decoded services <see cref="SamusOverreadServices"/> keeps, as the import pipeline reads them from the installed
/// overread document: one member per glitch path, named for what it reproduces.
/// </summary>
internal sealed record SamusOverreadServiceParts
{
    /// <summary>What a Samus animation frame past its pose's program reads.</summary>
    public required LaidOutProgramContinuation<SamusPoseId> AnimationContinuation { get; init; }

    /// <summary>Where a projectile trail starts when its read passes its placement list.</summary>
    public required TrailStartPastPlacements<SamusProjectileProgram, ProjectileTrailPlacementList, ProjectileTrailPlacement> TrailStarts { get; init; }

    /// <summary>What SpaceTime's palette copy overwrites in saved progression.</summary>
    public required SpaceTimeProgressionOverwrite<AnimationProgramCursor<SamusProjectileProgram>, ProgressionOverwrite> SpaceTimeOverwrite { get; init; }

    /// <summary>Where the Chainsaw beam's animation lands when its fuse speeds up.</summary>
    public required ChainsawFuseAnimationShift<AnimationProgramCursor<SamusProjectileProgram>> ChainsawFuseShift { get; init; }

    /// <summary>The delays a charge-flare frame reads below the flare's programs.</summary>
    public required ChargeFlareDelaysBelowPrograms ChargeFlareDelays { get; init; }

    /// <summary>Whether the posture-transition admission admits weapons for a pose outside its table.</summary>
    public required PostureAdmissionBeyondTransitions<SamusPoseId> PostureAdmission { get; init; }

    /// <summary>The suit colours Gravity Speed Booster copies from past its cycle.</summary>
    public required SpeedBoosterOverrunColours<SpeedBoosterPaletteOverrun, RgbColor> SpeedBoosterColours { get; init; }

    /// <summary>The speeds a Spazer and Plasma beam launches at.</summary>
    public required UnauthoredBeamLaunchSpeeds<SamusBeamCombination> UnauthoredBeamSpeeds { get; init; }

    /// <summary>The angles a Shinespark crash finishing in a drained pose releases its echoes at.</summary>
    public required DrainedCrashEchoDepartures<SamusPoseId, ShinesparkDepartureAngles> DrainedCrashDepartures { get; init; }

    /// <summary>The colours the Chainsaw beam's palette loader reads past its table.</summary>
    public required ChainsawBeamColours<RgbColor> ChainsawColours { get; init; }
}
