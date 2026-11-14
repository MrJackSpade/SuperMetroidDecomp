using static SuperMetroid.Core.Frontend.Intro.IntroEggEffectDefinitions;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Builds the intro egg shell fragments' actor definitions (<see cref="IntroEggEffectDefinitions"/>) by fragment.
/// </summary>
internal static class IntroEggParticleActors
{
    /// <summary>
    /// Returns the definition of one of the six shell fragments. All share
    /// an initializer and the fragment motion; each owns a distinct animation.
    /// </summary>
    /// <remarks>
    /// Issues #625 and #981: the six definitions contain the shared initializer, the fragment motion,
    /// and the fragment's own animation, in that order, matching the authored data.
    /// Each referenced animation starts with duration one and loops
    /// to itself. The egg spawn step creates each fragment once. Actor verification completes all six fragments with
    /// physical definition reads forbidden.
    ///
    /// Issues #625 and #982: the six selected animations are a separate logical
    /// table. Each contains duration one, the fragment's frame, a jump, and its
    /// own start, matching the authored data,
    /// which also confirms one sprite entry in each
    /// selected frame. <see cref="IntroDiscoverySprite"/> reads the frame, then follows
    /// the self-loop, so the bounded selector preserves every selected animation.
    ///
    /// Issues #625 and #983: each of the six referenced visual frames holds one sprite part, matching the authored data:
    /// X=508, Y=252, drawn in the egg's colours in front of the scene, with that
    /// frame's own picture. The six pictures come in three pairs.
    /// The bounded selector reaches only these six records.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="fragment"/> is not a piece of the shell.</exception>
    public static CinematicActorDefinition Particle(IntroEggShellFragment fragment) =>
        new(CinematicSpriteFrameBehavior.MetroidEggParticle,
            ShellFragmentAnimations.TryGetValue(fragment, out CinematicSpriteSegment animation)
                ? animation : throw new ArgumentOutOfRangeException(nameof(fragment)));
}
