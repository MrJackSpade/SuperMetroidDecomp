namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Where each frontend screen colour group lies among the screen's colours today (#1464). The positions are the screen
/// palette's rows until owner-keyed colour sets replace them; only <see cref="ScreenColourGroups"/> reads this catalog.
/// </summary>
internal static class ScreenColourGroupDefinitions
{
    /// <summary>Each group's row, first colour and colour count.</summary>
    public static IReadOnlyDictionary<ScreenColourGroup, ScreenColourGroupDefinition> Groups { get; } =
        new Dictionary<ScreenColourGroup, ScreenColourGroupDefinition>
        {
            [ScreenColourGroup.SamusSuit] = ScreenColourGroupDefinition.SpriteRow(4),
            [ScreenColourGroup.SamusBeams] = ScreenColourGroupDefinition.SpriteRow(6),
            [ScreenColourGroup.MapHighlight] = ScreenColourGroupDefinition.SpriteRow(3),
            [ScreenColourGroup.PauseReserveArrowShade] = new(ScreenColourKind.Background, 6, 6, 1),
            [ScreenColourGroup.PauseReserveArrowLight] = new(ScreenColourKind.Background, 6, 11, 1),
            [ScreenColourGroup.FileCopyArrows] = new(ScreenColourKind.Sprite, 1, 1, 7),
            [ScreenColourGroup.TitleCopyrightWhite] = new(ScreenColourKind.Sprite, 4, 9, 1),
            [ScreenColourGroup.TitleCopyrightRed] = new(ScreenColourKind.Sprite, 4, 10, 1),
            [ScreenColourGroup.TitleTubeLight] = new(ScreenColourKind.Background, 2, 10, 4),
            [ScreenColourGroup.TitleDisplays] = new(ScreenColourKind.Background, 2, 14, 2),
            [ScreenColourGroup.TitleDimWarmPaint] = new(ScreenColourKind.Background, 0, 3, 1),
            [ScreenColourGroup.TitleDimGreenPaint] = new(ScreenColourKind.Background, 1, 3, 1),
            [ScreenColourGroup.GameOverBaby] = ScreenColourGroupDefinition.SpriteRow(4),
            [ScreenColourGroup.IntroMotherBrain] = ScreenColourGroupDefinition.SpriteRow(7),
            [ScreenColourGroup.IntroGameplayBackground] = new(ScreenColourKind.Background, 0, 0, 20),
            [ScreenColourGroup.IntroGameplayClearedBackground] = ScreenColourGroupDefinition.BackgroundRow(0),
            [ScreenColourGroup.IntroGameplayEnvironment] = ScreenColourGroupDefinition.BackgroundRow(3),
            [ScreenColourGroup.IntroGameplaySamus] = new(ScreenColourKind.Sprite, 6, 9, 6),
            [ScreenColourGroup.IntroNarrationText] = new(ScreenColourKind.Background, 1, 4, 3),
            [ScreenColourGroup.IntroNarrationBackground] = ScreenColourGroupDefinition.BackgroundRow(7),
            [ScreenColourGroup.IntroNarrationPortrait] = new(ScreenColourKind.Sprite, 4, 0, 32),
            [ScreenColourGroup.IntroNarrationObjects] = ScreenColourGroupDefinition.SpriteRow(7),
            [ScreenColourGroup.IntroDiscoveryBackground] = ScreenColourGroupDefinition.BackgroundRow(2),
            [ScreenColourGroup.IntroDiscoveryScientists] = new(ScreenColourKind.Sprite, 6, 0, 9),
            [ScreenColourGroup.CeresExplosions] = ScreenColourGroupDefinition.SpriteRow(5),
            [ScreenColourGroup.EndingClearTimeDigits] = ScreenColourGroupDefinition.SpriteRow(0),
            [ScreenColourGroup.EndingTimeLabel] = ScreenColourGroupDefinition.SpriteRow(1),
            [ScreenColourGroup.EndingText] = ScreenColourGroupDefinition.SpriteRow(2),
            [ScreenColourGroup.EndingClouds] = ScreenColourGroupDefinition.SpriteRow(5),
            [ScreenColourGroup.EndingExplosionLava] = ScreenColourGroupDefinition.SpriteRow(5),
            [ScreenColourGroup.EndingExplodingPlanet] = ScreenColourGroupDefinition.SpriteRow(7),
            [ScreenColourGroup.EndingExplodingPlanetGlow] = new(ScreenColourKind.Sprite, 7, 14, 1),
            [ScreenColourGroup.EndingExplosionAfterglow] = ScreenColourGroupDefinition.SpriteRow(6),
            [ScreenColourGroup.EndingExplosionBackground] = ScreenColourGroupDefinition.BackgroundRow(7),
            [ScreenColourGroup.EndingFlashBackground] = ScreenColourGroupDefinition.BackgroundRow(1),
            [ScreenColourGroup.EndingFlashBackgroundTransparent] = new(ScreenColourKind.Background, 1, 0, 1),
            [ScreenColourGroup.EndingFlashSpriteTransparent] = new(ScreenColourKind.Sprite, 0, 0, 1),
            [ScreenColourGroup.EndingFinalGunship] = ScreenColourGroupDefinition.BackgroundRow(5),
            [ScreenColourGroup.EndingAnimalEscape] = ScreenColourGroupDefinition.SpriteRow(7),
            [ScreenColourGroup.EndingLogoBackground] = ScreenColourGroupDefinition.BackgroundRow(1),
            [ScreenColourGroup.EndingLogoSprites] = ScreenColourGroupDefinition.SpriteRow(7),
            [ScreenColourGroup.EndingPostCreditsRestored] = new(ScreenColourKind.Background, 0, 4, 252),
            [ScreenColourGroup.EndingWaitingBackdrop] = ScreenColourGroupDefinition.BackgroundRow(2),
            [ScreenColourGroup.EndingSuitlessSamus] = ScreenColourGroupDefinition.SpriteRow(5),
            [ScreenColourGroup.EndingSuitedSamus] = ScreenColourGroupDefinition.SpriteRow(6),
            [ScreenColourGroup.EndingFallingSamus] = ScreenColourGroupDefinition.SpriteRow(6),
            [ScreenColourGroup.EndingShootingSamus] = ScreenColourGroupDefinition.SpriteRow(7),
            [ScreenColourGroup.EndingShot] = ScreenColourGroupDefinition.SpriteRow(4),
        };
}
