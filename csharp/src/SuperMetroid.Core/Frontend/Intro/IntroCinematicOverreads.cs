using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The installed services reproducing what the opening cinematic reads past its data: Samus's glitch reads, which its
/// gameplay flashbacks' Samus and projectiles consult, and the steps the egg's shell fragments fall by past their
/// gravity curve (#1500).
/// </summary>
/// <param name="Samus">The services reproducing Samus's glitch reads.</param>
/// <param name="ShellFall">The steps the egg's shell fragments fall by past their gravity curve.</param>
/// <exception cref="InvalidDataException">A shell fragment is still in the air after every installed step.</exception>
public sealed record IntroCinematicOverreads(SamusOverreadServices Samus, ShellFragmentFallPastCurve ShellFall)
{
    /// <summary>The steps the egg's shell fragments fall by past their gravity curve, within which every fragment lands.</summary>
    public ShellFragmentFallPastCurve ShellFall { get; init; } = IntroEggParticle.ConfirmEveryFragmentLands(ShellFall);
}
