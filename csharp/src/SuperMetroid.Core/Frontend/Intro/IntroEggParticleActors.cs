namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The intro egg shell pieces' actor definitions (<see cref="IntroEggEffectDefinitions"/>), by piece.
/// </summary>
internal static partial class IntroEggParticleActors
{
    /// <summary>
    /// The actor of one of the six shell pieces: every piece flies as a shell piece does and shows its own picture, in the
    /// egg's colours in front of the scene, for as long as it flies.
    /// </summary>
    public static CinematicActorDefinition<IntroEggShellFragment> Particle(IntroEggShellFragment fragment) => new(fragment);
}
