using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's scene actors: where each starts, which sprite palette it draws with and which cinematic program it runs
/// (#1416: moved from the deleted ending catalog; the cartridge's constructor and instruction-list locations are the
/// cinematic import's).
/// </summary>
internal static class EndingActorDefinitions
{
    /// <summary>OBJ palette two used by the operation-success text actors.</summary>
    public static readonly SnesObjAttributeWord TextPalette = SnesObjPalettes.Palette(2);
    /// <summary>OBJ palette one used by the clear-time label, separate from digit actors' zero palette override.</summary>
    public static readonly SnesObjAttributeWord TimeLabelPalette = SnesObjPalettes.Palette(1);
    /// <summary>OBJ palette five used by atmospheric clouds, explosion overlays, and suitless reward actors.</summary>
    public static readonly SnesObjAttributeWord ScenePalette = SnesObjPalettes.Palette(5);
    /// <summary>OBJ palette six used by explosion afterglow and armored Samus reward actors.</summary>
    public static readonly SnesObjAttributeWord AlternatePalette = SnesObjPalettes.Palette(6);
    /// <summary>OBJ palette seven used by the exploding planet, glow, and star actors.</summary>
    public static readonly SnesObjAttributeWord PlanetPalette = SnesObjPalettes.Palette(7);
    /// <summary>Yellow-cloud right actor: origin (320,192) pixels, OBJ palette five.</summary>
    internal static readonly EndingSpriteDefinition EscapeACloudRightTop =
        EndingSpriteDefinition.At(320, 192, ScenePalette, CinematicSpriteProgram.EndingCloud4);
    /// <summary>Yellow-cloud left actor: origin (-64,64) pixels, OBJ palette five.</summary>
    internal static readonly EndingSpriteDefinition EscapeACloudLeftTop =
        EndingSpriteDefinition.At(-64, 64, ScenePalette, CinematicSpriteProgram.EndingCloud5);
    /// <summary>Yellow-cloud right actor's second placement: origin (320,448) pixels, palette five.</summary>
    internal static readonly EndingSpriteDefinition EscapeACloudRightBottom =
        EndingSpriteDefinition.At(320, 448, ScenePalette, CinematicSpriteProgram.EndingCloud4);
    /// <summary>Yellow-cloud left actor's second placement: origin (-64,-192) pixels, palette five.</summary>
    internal static readonly EndingSpriteDefinition EscapeACloudLeftBottom =
        EndingSpriteDefinition.At(-64, -192, ScenePalette, CinematicSpriteProgram.EndingCloud5);
    /// <summary>First upper cloud, parameter zero, origin (128,-96).</summary>
    internal static readonly EndingSpriteDefinition EscapeBCloudTopA =
        EndingSpriteDefinition.At(128, -96, ScenePalette, CinematicSpriteProgram.EndingCloud0);
    /// <summary>Second upper cloud, parameter zero, origin (128,-32).</summary>
    internal static readonly EndingSpriteDefinition EscapeBCloudTopB =
        EndingSpriteDefinition.At(128, -32, ScenePalette, CinematicSpriteProgram.EndingCloud1);
    /// <summary>First lower cloud, parameter zero, origin (128,288).</summary>
    internal static readonly EndingSpriteDefinition EscapeBCloudBottomA =
        EndingSpriteDefinition.At(128, 288, ScenePalette, CinematicSpriteProgram.EndingCloud2);
    /// <summary>Second lower cloud, parameter zero, origin (128,352).</summary>
    internal static readonly EndingSpriteDefinition EscapeBCloudBottomB =
        EndingSpriteDefinition.At(128, 352, ScenePalette, CinematicSpriteProgram.EndingCloud3);
    /// <summary>Planet actor at (128,128) pixels, palette seven.</summary>
    internal static readonly EndingSpriteDefinition ExplodingZebes =
        EndingSpriteDefinition.At(128, 128, PlanetPalette, CinematicSpriteProgram.ZebesExplosionPlanet);
    /// <summary>Explosion palette-five actor: lava overlay at (128,128) pixels.</summary>
    internal static readonly EndingSpriteDefinition ExplosionLava =
        EndingSpriteDefinition.At(128, 128, ScenePalette, CinematicSpriteProgram.ZebesExplosionLava);
    /// <summary>Purple-glow actor: origin (128,128) pixels, palette seven.</summary>
    internal static readonly EndingSpriteDefinition ExplosionGlow =
        EndingSpriteDefinition.At(128, 128, PlanetPalette, CinematicSpriteProgram.ZebesExplosionGlow);
    /// <summary>Explosion-stars actor: origin (128,128) pixels, palette seven.</summary>
    internal static readonly EndingSpriteDefinition ExplosionStars =
        EndingSpriteDefinition.At(128, 128, PlanetPalette, CinematicSpriteProgram.ZebesExplosionStars);
    /// <summary>Palette-five explosion silhouette: origin (128,128) pixels.</summary>
    internal static readonly EndingSpriteDefinition ExplosionSilhouette =
        EndingSpriteDefinition.At(128, 128, ScenePalette, CinematicSpriteProgram.ZebesExplosionSilhouette);
    /// <summary>Right-hand explosion-stars actor: origin (128,128) pixels, palette seven.</summary>
    internal static readonly EndingSpriteDefinition ExplosionStarsRight =
        EndingSpriteDefinition.At(128, 128, PlanetPalette, CinematicSpriteProgram.ZebesExplosionRightStars);
    /// <summary>Left-hand explosion-stars actor: origin (-128,128) pixels, palette seven.</summary>
    internal static readonly EndingSpriteDefinition ExplosionStarsLeft =
        EndingSpriteDefinition.At(-128, 128, PlanetPalette, CinematicSpriteProgram.ZebesExplosionLeftStars);
    /// <summary>Zebes afterglow: origin (128,128) pixels, palette six.</summary>
    internal static readonly EndingSpriteDefinition ExplosionAfterglow =
        EndingSpriteDefinition.At(128, 128, AlternatePalette, CinematicSpriteProgram.ZebesExplosionAfterglow);
    /// <summary>THE OPERATION WAS actor: origin (128,96) pixels, palette two.</summary>
    internal static readonly EndingSpriteDefinition OperationWasText =
        EndingSpriteDefinition.At(128, 96, TextPalette, CinematicSpriteProgram.OperationWasText);
    /// <summary>COMPLETED SUCCESSFULLY actor: origin (128,96) pixels, palette two.</summary>
    internal static readonly EndingSpriteDefinition CompletedSuccessfullyText =
        EndingSpriteDefinition.At(128, 96, TextPalette, CinematicSpriteProgram.CompletedSuccessfullyText);
    /// <summary>The clear-time label actor at (128,160) pixels, palette one.</summary>
    internal static readonly EndingSpriteDefinition ClearTimeText =
        EndingSpriteDefinition.At(128, 160, TimeLabelPalette, CinematicSpriteProgram.ClearTimeText);
    /// <summary>Clear-time colon actor at (172,160) pixels with no palette override, separate from the numeric digits.</summary>
    internal static readonly EndingSpriteDefinition ClearTimeColon =
        EndingSpriteDefinition.At(172, 160, new SnesObjAttributeWord(0), CinematicSpriteProgram.ClearTimeColon);
    /// <summary>Shared clear-time digit origin Y=160 screen pixels, aligned with the label and colon.</summary>
    public const ushort ClearTimeDigitY = 160;
    /// <summary>Suitless idle-body actor: origin (120,136) pixels, palette five.</summary>
    internal static readonly EndingSpriteDefinition SuitlessRewardBody =
        EndingSpriteDefinition.At(120, 136, ScenePalette, CinematicSpriteProgram.SuitlessIdle);
    /// <summary>Suitless idle-legs actor at the shared origin (120,136) pixels, palette five.</summary>
    internal static readonly EndingSpriteDefinition SuitlessRewardLegs =
        EndingSpriteDefinition.At(120, 136, ScenePalette, CinematicSpriteProgram.SuitlessLegsIdle);
    /// <summary>Suited idle-body actor: origin (120,152) pixels, palette six.</summary>
    internal static readonly EndingSpriteDefinition ArmoredRewardBody =
        EndingSpriteDefinition.At(120, 152, AlternatePalette, CinematicSpriteProgram.SuitedIdle);
    /// <summary>Helmetless suited-head actor: origin (121,107) pixels, palette five.</summary>
    internal static readonly EndingSpriteDefinition HelmetlessRewardHead =
        EndingSpriteDefinition.At(121, 107, ScenePalette, CinematicSpriteProgram.HeadIdle);
    /// <summary>Helmeted suited-head actor: origin (124,108) pixels, palette six.</summary>
    internal static readonly EndingSpriteDefinition ArmoredRewardHead =
        EndingSpriteDefinition.At(124, 108, AlternatePalette, CinematicSpriteProgram.HelmetIdle);
}
