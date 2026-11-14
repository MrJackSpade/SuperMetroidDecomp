namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Fixed cinematic-object definitions for egg shell fragments and slime drops.</summary>
internal static class IntroEggEffectDefinitions
{

    /// <summary>Number of shell-fragment definitions.</summary>
    public const int ParticleCount = 6;

    /// <summary>Number of slime drops allocated by the confused-baby frame behaviour.</summary>
    public const int SlimeDropCount = 4;

    /// <summary>The six particles' animations, indexed by particle.</summary>
    internal static readonly CinematicSpriteSegment[] ParticleAnimations =
    [
        CinematicSpriteSegment.MetroidEggParticle1, CinematicSpriteSegment.MetroidEggParticle2, CinematicSpriteSegment.MetroidEggParticle3,
        CinematicSpriteSegment.MetroidEggParticle4, CinematicSpriteSegment.MetroidEggParticle5, CinematicSpriteSegment.MetroidEggParticle6,
    ];

    /// <summary>The shared egg-slime initialization, motion, and animation definition.</summary>
    /// <remarks>
    /// Issues #625 and #985: the moving animation and the four impact
    /// animations select five one-entry visual frames,
    /// one for each of f=0..4, matching the authored data.
    /// Each record draws that frame's own picture four pixels left of and above the slime, in the egg's
    /// colours in front of the scene.
    /// These are authored artwork choices; preserve the five bounded picture
    /// identities and do not infer a sixth frame from adjacent data.
    /// </remarks>
    public static CinematicActorDefinition SlimeDrop =>
        new(CinematicSpriteFrameBehavior.MetroidEggSlimeDrops, CinematicSpriteSegment.MetroidEggParticle7);
}
