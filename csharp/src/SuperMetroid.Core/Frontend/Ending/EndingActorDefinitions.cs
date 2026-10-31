using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's scene actors: where each starts, which of the screen's colours it draws in and which cinematic program it runs
/// (#1416: moved from the deleted ending catalog; the cartridge's constructor and instruction-list locations are the
/// cinematic import's).
/// </summary>
internal static class EndingActorDefinitions
{
    /// <summary>Yellow-cloud right actor: origin (320,192) pixels.</summary>
    internal static readonly EndingSpriteDefinition EscapeACloudRightTop =
        EndingSpriteDefinition.At(new(320, 192), ScreenColourGroup.EndingClouds, CinematicSpriteProgram.EndingCloud4);
    /// <summary>Yellow-cloud left actor: origin (-64,64) pixels.</summary>
    internal static readonly EndingSpriteDefinition EscapeACloudLeftTop =
        EndingSpriteDefinition.At(new(-64, 64), ScreenColourGroup.EndingClouds, CinematicSpriteProgram.EndingCloud5);
    /// <summary>Yellow-cloud right actor's second placement: origin (320,448) pixels.</summary>
    internal static readonly EndingSpriteDefinition EscapeACloudRightBottom =
        EndingSpriteDefinition.At(new(320, 448), ScreenColourGroup.EndingClouds, CinematicSpriteProgram.EndingCloud4);
    /// <summary>Yellow-cloud left actor's second placement: origin (-64,-192) pixels.</summary>
    internal static readonly EndingSpriteDefinition EscapeACloudLeftBottom =
        EndingSpriteDefinition.At(new(-64, -192), ScreenColourGroup.EndingClouds, CinematicSpriteProgram.EndingCloud5);
    /// <summary>First upper cloud, parameter zero, origin (128,-96).</summary>
    internal static readonly EndingSpriteDefinition EscapeBCloudTopA =
        EndingSpriteDefinition.At(new(128, -96), ScreenColourGroup.EndingClouds, CinematicSpriteProgram.EndingCloud0);
    /// <summary>Second upper cloud, parameter zero, origin (128,-32).</summary>
    internal static readonly EndingSpriteDefinition EscapeBCloudTopB =
        EndingSpriteDefinition.At(new(128, -32), ScreenColourGroup.EndingClouds, CinematicSpriteProgram.EndingCloud1);
    /// <summary>First lower cloud, parameter zero, origin (128,288).</summary>
    internal static readonly EndingSpriteDefinition EscapeBCloudBottomA =
        EndingSpriteDefinition.At(new(128, 288), ScreenColourGroup.EndingClouds, CinematicSpriteProgram.EndingCloud2);
    /// <summary>Second lower cloud, parameter zero, origin (128,352).</summary>
    internal static readonly EndingSpriteDefinition EscapeBCloudBottomB =
        EndingSpriteDefinition.At(new(128, 352), ScreenColourGroup.EndingClouds, CinematicSpriteProgram.EndingCloud3);
    /// <summary>Planet actor at (128,128) pixels.</summary>
    internal static readonly EndingSpriteDefinition ExplodingZebes =
        EndingSpriteDefinition.At(new(128, 128), ScreenColourGroup.EndingExplodingPlanet, CinematicSpriteProgram.ZebesExplosionPlanet);
    /// <summary>Explosion lava overlay at (128,128) pixels.</summary>
    internal static readonly EndingSpriteDefinition ExplosionLava =
        EndingSpriteDefinition.At(new(128, 128), ScreenColourGroup.EndingExplosionLava, CinematicSpriteProgram.ZebesExplosionLava);
    /// <summary>Purple-glow actor: origin (128,128) pixels.</summary>
    internal static readonly EndingSpriteDefinition ExplosionGlow =
        EndingSpriteDefinition.At(new(128, 128), ScreenColourGroup.EndingExplodingPlanet, CinematicSpriteProgram.ZebesExplosionGlow);
    /// <summary>Explosion-stars actor: origin (128,128) pixels.</summary>
    internal static readonly EndingSpriteDefinition ExplosionStars =
        EndingSpriteDefinition.At(new(128, 128), ScreenColourGroup.EndingExplodingPlanet, CinematicSpriteProgram.ZebesExplosionStars);
    /// <summary>Explosion silhouette: origin (128,128) pixels.</summary>
    internal static readonly EndingSpriteDefinition ExplosionSilhouette =
        EndingSpriteDefinition.At(new(128, 128), ScreenColourGroup.EndingExplosionLava, CinematicSpriteProgram.ZebesExplosionSilhouette);
    /// <summary>Right-hand explosion-stars actor: origin (128,128) pixels.</summary>
    internal static readonly EndingSpriteDefinition ExplosionStarsRight =
        EndingSpriteDefinition.At(new(128, 128), ScreenColourGroup.EndingExplodingPlanet, CinematicSpriteProgram.ZebesExplosionRightStars);
    /// <summary>Left-hand explosion-stars actor: origin (-128,128) pixels.</summary>
    internal static readonly EndingSpriteDefinition ExplosionStarsLeft =
        EndingSpriteDefinition.At(new(-128, 128), ScreenColourGroup.EndingExplodingPlanet, CinematicSpriteProgram.ZebesExplosionLeftStars);
    /// <summary>Zebes afterglow: origin (128,128) pixels.</summary>
    internal static readonly EndingSpriteDefinition ExplosionAfterglow =
        EndingSpriteDefinition.At(new(128, 128), ScreenColourGroup.EndingExplosionAfterglow, CinematicSpriteProgram.ZebesExplosionAfterglow);
    /// <summary>THE OPERATION WAS actor: origin (128,96) pixels.</summary>
    internal static readonly EndingSpriteDefinition OperationWasText =
        EndingSpriteDefinition.At(new(128, 96), ScreenColourGroup.EndingText, CinematicSpriteProgram.OperationWasText);
    /// <summary>COMPLETED SUCCESSFULLY actor: origin (128,96) pixels.</summary>
    internal static readonly EndingSpriteDefinition CompletedSuccessfullyText =
        EndingSpriteDefinition.At(new(128, 96), ScreenColourGroup.EndingText, CinematicSpriteProgram.CompletedSuccessfullyText);
    /// <summary>The clear-time label actor at (128,160) pixels.</summary>
    internal static readonly EndingSpriteDefinition ClearTimeText =
        EndingSpriteDefinition.At(new(128, 160), ScreenColourGroup.EndingTimeLabel, CinematicSpriteProgram.ClearTimeText);
    /// <summary>Clear-time colon actor at (172,160) pixels, in the clear-time digits' colours.</summary>
    internal static readonly EndingSpriteDefinition ClearTimeColon =
        EndingSpriteDefinition.At(new(172, 160), ScreenColourGroup.EndingClearTimeDigits, CinematicSpriteProgram.ClearTimeColon);
    /// <summary>Shared clear-time digit origin Y=160 screen pixels, aligned with the label and colon.</summary>
    public const int ClearTimeDigitY = 160;
    /// <summary>Suitless idle-body actor: origin (120,136) pixels.</summary>
    internal static readonly EndingSpriteDefinition SuitlessRewardBody =
        EndingSpriteDefinition.At(new(120, 136), ScreenColourGroup.EndingSuitlessSamus, CinematicSpriteProgram.SuitlessIdle);
    /// <summary>Suitless idle-legs actor at the shared origin (120,136) pixels.</summary>
    internal static readonly EndingSpriteDefinition SuitlessRewardLegs =
        EndingSpriteDefinition.At(new(120, 136), ScreenColourGroup.EndingSuitlessSamus, CinematicSpriteProgram.SuitlessLegsIdle);
    /// <summary>Suited idle-body actor: origin (120,152) pixels.</summary>
    internal static readonly EndingSpriteDefinition ArmoredRewardBody =
        EndingSpriteDefinition.At(new(120, 152), ScreenColourGroup.EndingSuitedSamus, CinematicSpriteProgram.SuitedIdle);
    /// <summary>Helmetless suited-head actor: origin (121,107) pixels.</summary>
    internal static readonly EndingSpriteDefinition HelmetlessRewardHead =
        EndingSpriteDefinition.At(new(121, 107), ScreenColourGroup.EndingSuitlessSamus, CinematicSpriteProgram.HeadIdle);
    /// <summary>Helmeted suited-head actor: origin (124,108) pixels.</summary>
    internal static readonly EndingSpriteDefinition ArmoredRewardHead =
        EndingSpriteDefinition.At(new(124, 108), ScreenColourGroup.EndingSuitedSamus, CinematicSpriteProgram.HelmetIdle);
}
