using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// The compatibility services that reproduce what Samus's glitch paths read past the data the original game meant them
/// to read (#1474), as the import pipeline decoded them from the player's game data (owner question 3). The session's
/// gameplay runtime receives them with the installed content and hands each to the owner that consults it when it builds
/// that owner: her animation, her palette, her beam's colours, and the projectile system, which takes its own group whole.
/// Lifetime: the installation's, shared by every runtime of the session.
/// </summary>
public sealed class SamusOverreadServices
{
    /// <summary>Keeps every decoded service.</summary>
    /// <param name="animationContinuation">What a Samus animation frame past its pose's program reads.</param>
    /// <param name="speedBoosterColours">The suit colours Gravity Speed Booster copies from past its cycle.</param>
    /// <param name="chainsawColours">The colours the Chainsaw beam's palette loader reads past its table.</param>
    /// <param name="projectiles">The services Samus's projectiles consult where they read past their data.</param>
    internal SamusOverreadServices(
        LaidOutProgramContinuation<SamusPoseId> animationContinuation,
        SpeedBoosterOverrunColours<SpeedBoosterPaletteOverrun, RgbColor> speedBoosterColours,
        ChainsawBeamColours<RgbColor> chainsawColours,
        SamusProjectileOverreads projectiles)
    {
        AnimationContinuation = Ensure.NotNull(animationContinuation);
        SpeedBoosterColours = Ensure.NotNull(speedBoosterColours);
        ChainsawColours = Ensure.NotNull(chainsawColours);
        Projectiles = Ensure.NotNull(projectiles);
    }

    /// <summary>What a Samus animation frame past its pose's program reads, by a pose that animates by each program.</summary>
    internal LaidOutProgramContinuation<SamusPoseId> AnimationContinuation { get; }

    /// <summary>The suit colours Gravity Speed Booster copies from past its cycle.</summary>
    internal SpeedBoosterOverrunColours<SpeedBoosterPaletteOverrun, RgbColor> SpeedBoosterColours { get; }

    /// <summary>The colours the Chainsaw beam's palette loader reads past its table.</summary>
    internal ChainsawBeamColours<RgbColor> ChainsawColours { get; }

    /// <summary>The services Samus's projectiles consult where their glitch paths read past their data.</summary>
    internal SamusProjectileOverreads Projectiles { get; }
}
