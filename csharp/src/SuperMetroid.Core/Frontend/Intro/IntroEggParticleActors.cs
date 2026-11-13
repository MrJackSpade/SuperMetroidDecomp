using static SuperMetroid.Core.Frontend.Intro.IntroEggEffectDefinitions;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Builds the intro egg particles' actor definitions (<see cref="IntroEggEffectDefinitions"/>) by particle.
/// </summary>
internal static class IntroEggParticleActors
{
    /// <summary>
    /// Returns one of the six shell-fragment definitions. All share
    /// an initializer and the fragment motion; each owns a distinct animation.
    /// </summary>
    /// <remarks>
    /// Issues #625 and #981: for fragments i=0..5,
    /// the six definitions contain the shared initializer, the fragment motion,
    /// and the fragment's own animation, in that order, matching the authored data.
    /// Each referenced animation starts with duration one and loops
    /// to itself. The egg spawn step creates exactly indices 0..5; the
    /// method rejects negative and one-past-end indices. The shared callback
    /// pair and two strides are an exact smaller representation of these
    /// six definitions. Actor verification completes all six fragments with
    /// physical definition reads forbidden.
    ///
    /// Issues #625 and #982: the six selected animations are a separate logical
    /// table. For i=0..5, animation i contains duration
    /// one, the fragment's frame, a jump, and its
    /// own start, matching the authored data,
    /// which also confirms one sprite entry in each
    /// selected frame. <see cref="IntroDiscoverySprite"/> reads the frame, then follows
    /// the self-loop, so the bounded selector preserves every selected animation.
    ///
    /// Issues #625 and #983: of the six referenced visual frames,
    /// i=0..5, each holds one sprite part, matching the authored data:
    /// X=508, Y=252, drawn in the egg's colours in front of the scene, and a tile,
    /// where tile=261+16*(i/2)+(i%2). Thus the six tile
    /// identities are 261/262, 277/278, and 293/294 in three pairs.
    /// The bounded selector reaches only these six records.
    /// </remarks>
    public static CinematicActorDefinition Particle(int index) =>
        new(CinematicSpriteFrameBehavior.MetroidEggParticle, Ensure.Element(ParticleAnimations, index));
}
