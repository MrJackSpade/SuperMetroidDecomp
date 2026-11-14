using static SuperMetroid.Core.Frontend.Intro.IntroEggEffectDefinitions;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The intro egg shell pieces' actor definitions (<see cref="IntroEggEffectDefinitions"/>), by piece.
/// </summary>
internal static class IntroEggParticleActors
{
    /// <summary>
    /// The actor of one of the six shell pieces: every piece flies as a shell piece does and shows its own picture, in the
    /// egg's colours in front of the scene, for as long as it flies.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="fragment"/> is not a piece of the shell.</exception>
    public static CinematicActorDefinition Particle(IntroEggShellFragment fragment) =>
        new(CinematicSpriteFrameBehavior.MetroidEggParticle,
            ShellFragmentAnimations.TryGetValue(fragment, out CinematicSpriteSegment animation)
                ? animation : throw new ArgumentOutOfRangeException(nameof(fragment)));
}
