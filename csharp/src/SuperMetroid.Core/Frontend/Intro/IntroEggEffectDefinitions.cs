namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Fixed cinematic-object definitions for egg shell fragments and slime drops.</summary>
internal static class IntroEggEffectDefinitions
{

    /// <summary>Number of shell-fragment definitions at <c>$8B:CECD-$8B:CEEF</c>.</summary>
    public const int ParticleCount = 6;

    /// <summary>Number of slime drops allocated by the confused-baby pre-instruction.</summary>
    public const int SlimeDropCount = 4;

    /// <summary>The six particles' programs, indexed by particle.</summary>
    internal static readonly CinematicSpriteProgram[] ParticlePrograms =
    [
        CinematicSpriteProgram.MetroidEggParticle1, CinematicSpriteProgram.MetroidEggParticle2, CinematicSpriteProgram.MetroidEggParticle3,
        CinematicSpriteProgram.MetroidEggParticle4, CinematicSpriteProgram.MetroidEggParticle5, CinematicSpriteProgram.MetroidEggParticle6,
    ];

    /// <summary><c>$8B:CEF1</c>, the shared egg-slime initialization, motion, and list definition.</summary>
    /// <remarks>
    /// Issues #625 and #985: the moving list at $8B:CD69 and the four impact
    /// records at $8B:CD71 select five one-entry visual spritemaps at
    /// $8C:8FA8+7*f for f=0..4. All 35 bytes and five list references
    /// match pinned NTSC J/U v1.0 ROM and bank_8B.asm/bank_8C.asm.
    /// Each record has count one, encoded X=$01FC, Y=$FC, and attributes
    /// $3E00 | tile. Tiles are $10D+f for f=0..2, then
    /// $10D+$10*(f-2) for f=3..4: $10D/$10E/$10F/$11D/$12D.
    /// These are authored artwork choices; preserve the five bounded tile
    /// identities and do not infer a sixth frame from adjacent ROM data.
    /// </remarks>
    public static CinematicActorDefinition SlimeDrop =>
        new(CinematicSpritePreInstruction.MetroidEggSlimeDrops, CinematicSpriteProgram.MetroidEggParticle7);
}
