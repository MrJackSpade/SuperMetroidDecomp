namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Fixed cinematic-object definitions for egg shell fragments and slime drops.</summary>
internal static class IntroEggEffectDefinitions
{

    /// <summary>Number of shell-fragment definitions.</summary>
    public const int ParticleCount = 6;

    /// <summary>Number of slime drops allocated by the confused-baby frame behaviour.</summary>
    public const int SlimeDropCount = 4;

    /// <summary>The six particles' programs, indexed by particle.</summary>
    internal static readonly CinematicSpriteSegment[] ParticleAnimations =
    [
        CinematicSpriteSegment.MetroidEggParticle1, CinematicSpriteSegment.MetroidEggParticle2, CinematicSpriteSegment.MetroidEggParticle3,
        CinematicSpriteSegment.MetroidEggParticle4, CinematicSpriteSegment.MetroidEggParticle5, CinematicSpriteSegment.MetroidEggParticle6,
    ];

    /// <summary>The shared egg-slime initialization, motion, and animation definition.</summary>
    /// <remarks>
    /// Issues #625 and #985: the moving list and the four impact
    /// records select five one-entry visual frames,
    /// one for each of f=0..4, matching the authored data.
    /// Each record has one part, at X=508, Y=252, drawn in the egg's colours in front of the
    /// scene, and a tile. Tiles are 269+f for f=0..2, then
    /// 269+16*(f-2) for f=3..4: 269/270/271/285/301.
    /// These are authored artwork choices; preserve the five bounded tile
    /// identities and do not infer a sixth frame from adjacent data.
    /// </remarks>
    public static CinematicActorDefinition SlimeDrop =>
        new(CinematicSpriteFrameBehavior.MetroidEggSlimeDrops, CinematicSpriteSegment.MetroidEggParticle7);
}
