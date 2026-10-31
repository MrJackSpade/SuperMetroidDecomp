using static SuperMetroid.Core.Frontend.Intro.IntroEggEffectDefinitions;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Builds the intro egg particles' actor definitions (<see cref="IntroEggEffectDefinitions"/>) by particle.
/// </summary>
internal static class IntroEggParticleActors
{
    /// <summary>
    /// Returns one of the six adjacent shell-fragment definitions. All share initializer
    /// an initializer and the fragment motion; each owns a distinct eight-byte animation.
    /// </summary>
    /// <remarks>
    /// Issues #625 and #981: for fragments i=0..5,
    /// the six definitions are six bytes apart and contain the shared initializer, the fragment motion,
    /// and an animation eight bytes after the previous one's, in that order. All 18 words
    /// match the original data.
    /// Each referenced eight-byte list starts with duration one and loops
    /// to itself. The egg spawn opcode creates exactly indices 0..5; the
    /// method rejects negative and one-past-end indices. The shared callback
    /// pair and two strides are an exact smaller representation of these
    /// six definitions. Actor verification completes all six fragments with
    /// physical definition reads forbidden.
    ///
    /// Issues #625 and #982: the six selected lists are a separate logical
    /// table. For i=0..5, animation i contains duration
    /// one, a spritemap seven bytes after the previous one's, a goto, and its
    /// own start. All 24 words match the original data,
    /// which also confirms one sprite entry at each
    /// selected pointer. IntroDiscoverySprite reads the frame, then follows
    /// the self-loop, so the bounded strides preserve every selected list.
    ///
    /// Issues #625 and #983: of the six referenced visual spritemaps,
    /// i=0..5, each encodes one five-byte sprite entry after
    /// a count word of one. All 42 bytes match the original data:
    /// encoded X=508, Y=252, and sprite attributes
    /// palette seven, priority three, and a tile, where tile=$105+$10*(i/2)+(i%2). Thus the six tile
    /// identities are $105/$106, $115/$116, and $125/$126 in three pairs.
    /// The generic sprite reader consumes the count, X, Y, and attribute word
    /// in that order; the bounded selector reaches only these six records.
    /// </remarks>
    public static CinematicActorDefinition Particle(int index) =>
        new(CinematicSpriteFrameBehavior.MetroidEggParticle, Ensure.Element(ParticlePrograms, index));
}
