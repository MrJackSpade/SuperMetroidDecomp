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
            [ScreenColourGroup.SamusSuit] = SpriteColourGroupDefinition.WholeRow(4),
            [ScreenColourGroup.SamusBeams] = SpriteColourGroupDefinition.WholeRow(6),
            [ScreenColourGroup.MapHighlight] = SpriteColourGroupDefinition.WholeRow(3),
            [ScreenColourGroup.PauseReserveArrowShade] = new BackgroundColourGroupDefinition(6, 6, 1),
            [ScreenColourGroup.PauseReserveArrowLight] = new BackgroundColourGroupDefinition(6, 11, 1),
            [ScreenColourGroup.FileCopyArrows] = new SpriteColourGroupDefinition(1, 1, 7),
            [ScreenColourGroup.TitleCopyrightWhite] = new SpriteColourGroupDefinition(4, 9, 1),
            [ScreenColourGroup.TitleCopyrightRed] = new SpriteColourGroupDefinition(4, 10, 1),
            [ScreenColourGroup.TitleTubeLight] = new BackgroundColourGroupDefinition(2, 10, 4),
            [ScreenColourGroup.TitleDisplays] = new BackgroundColourGroupDefinition(2, 14, 2),
            [ScreenColourGroup.TitleDimWarmPaint] = new BackgroundColourGroupDefinition(0, 3, 1),
            [ScreenColourGroup.TitleDimGreenPaint] = new BackgroundColourGroupDefinition(1, 3, 1),
            [ScreenColourGroup.GameOverBaby] = SpriteColourGroupDefinition.WholeRow(4),
            [ScreenColourGroup.IntroMotherBrain] = SpriteColourGroupDefinition.WholeRow(7),
            [ScreenColourGroup.IntroGameplayBackground] = new BackgroundColourGroupDefinition(0, 0, 20),
            [ScreenColourGroup.IntroGameplayClearedBackground] = BackgroundColourGroupDefinition.WholeRow(0),
            [ScreenColourGroup.IntroGameplayEnvironment] = BackgroundColourGroupDefinition.WholeRow(3),
            [ScreenColourGroup.IntroGameplaySamus] = new SpriteColourGroupDefinition(6, 9, 6),
            [ScreenColourGroup.IntroNarrationText] = new BackgroundColourGroupDefinition(1, 4, 3),
            [ScreenColourGroup.IntroNarrationBackground] = BackgroundColourGroupDefinition.WholeRow(7),
            [ScreenColourGroup.IntroNarrationPortrait] = new SpriteColourGroupDefinition(4, 0, 32),
            [ScreenColourGroup.IntroNarrationObjects] = SpriteColourGroupDefinition.WholeRow(7),
            [ScreenColourGroup.IntroDiscoveryBackground] = BackgroundColourGroupDefinition.WholeRow(2),
            [ScreenColourGroup.IntroDiscoveryScientists] = new SpriteColourGroupDefinition(6, 0, 9),
            [ScreenColourGroup.CeresExplosions] = SpriteColourGroupDefinition.WholeRow(5),
            [ScreenColourGroup.EndingClearTimeDigits] = SpriteColourGroupDefinition.WholeRow(0),
            [ScreenColourGroup.EndingTimeLabel] = SpriteColourGroupDefinition.WholeRow(1),
            [ScreenColourGroup.EndingText] = SpriteColourGroupDefinition.WholeRow(2),
            [ScreenColourGroup.EndingClouds] = SpriteColourGroupDefinition.WholeRow(5),
            [ScreenColourGroup.EndingExplosionLava] = SpriteColourGroupDefinition.WholeRow(5),
            [ScreenColourGroup.EndingExplodingPlanet] = SpriteColourGroupDefinition.WholeRow(7),
            [ScreenColourGroup.EndingExplodingPlanetGlow] = new SpriteColourGroupDefinition(7, 14, 1),
            [ScreenColourGroup.EndingExplosionAfterglow] = SpriteColourGroupDefinition.WholeRow(6),
            [ScreenColourGroup.EndingExplosionBackground] = BackgroundColourGroupDefinition.WholeRow(7),
            [ScreenColourGroup.EndingFlashBackground] = BackgroundColourGroupDefinition.WholeRow(1),
            [ScreenColourGroup.EndingFlashBackgroundTransparent] = new BackgroundColourGroupDefinition(1, 0, 1),
            [ScreenColourGroup.EndingFlashSpriteTransparent] = new SpriteColourGroupDefinition(0, 0, 1),
            [ScreenColourGroup.EndingFinalGunship] = BackgroundColourGroupDefinition.WholeRow(5),
            [ScreenColourGroup.EndingAnimalEscape] = SpriteColourGroupDefinition.WholeRow(7),
            [ScreenColourGroup.EndingLogoBackground] = BackgroundColourGroupDefinition.WholeRow(1),
            [ScreenColourGroup.EndingLogoSprites] = SpriteColourGroupDefinition.WholeRow(7),
            [ScreenColourGroup.EndingPostCreditsRestored] = new BackgroundColourGroupDefinition(0, 4, 252),
            [ScreenColourGroup.EndingWaitingBackdrop] = BackgroundColourGroupDefinition.WholeRow(2),
            [ScreenColourGroup.EndingSuitlessSamus] = SpriteColourGroupDefinition.WholeRow(5),
            [ScreenColourGroup.EndingSuitedSamus] = SpriteColourGroupDefinition.WholeRow(6),
            [ScreenColourGroup.EndingFallingSamus] = SpriteColourGroupDefinition.WholeRow(6),
            [ScreenColourGroup.EndingShootingSamus] = SpriteColourGroupDefinition.WholeRow(7),
            [ScreenColourGroup.EndingShot] = SpriteColourGroupDefinition.WholeRow(4),
        };
}
