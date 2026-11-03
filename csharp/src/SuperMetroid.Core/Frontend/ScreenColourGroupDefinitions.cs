namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Which of a frontend screen's colour owners holds each screen colour group, and where in it (#1464): the owner's set,
/// the group's first colour in that set and its colour count. Only <see cref="ScreenColourGroups"/> reads this catalog.
/// </summary>
internal static class ScreenColourGroupDefinitions
{
    /// <summary>Each group's owner, set, first colour and colour count.</summary>
    public static IReadOnlyDictionary<ScreenColourGroup, ScreenColourPlacement> Groups { get; } =
        new Dictionary<ScreenColourGroup, ScreenColourPlacement>
        {
            [ScreenColourGroup.SamusSuit] = ScreenSpriteSetPlacement.WholeSet(4),
            [ScreenColourGroup.SamusBeams] = ScreenSpriteSetPlacement.WholeSet(6),
            [ScreenColourGroup.TitleTubeLight] = new ScreenBackgroundSetPlacement(2, 10, 4),
            [ScreenColourGroup.TitleDisplays] = new ScreenBackgroundSetPlacement(2, 14, 2),
            [ScreenColourGroup.EndingClearTimeDigits] = ScreenSpriteSetPlacement.WholeSet(0),
            [ScreenColourGroup.EndingTimeLabel] = ScreenSpriteSetPlacement.WholeSet(1),
            [ScreenColourGroup.EndingText] = ScreenSpriteSetPlacement.WholeSet(2),
            [ScreenColourGroup.EndingClouds] = ScreenSpriteSetPlacement.WholeSet(5),
            [ScreenColourGroup.EndingExplosionLava] = ScreenSpriteSetPlacement.WholeSet(5),
            [ScreenColourGroup.EndingExplodingPlanet] = ScreenSpriteSetPlacement.WholeSet(7),
            [ScreenColourGroup.EndingExplodingPlanetGlow] = new ScreenSpriteSetPlacement(7, 14, 1),
            [ScreenColourGroup.EndingExplosionAfterglow] = ScreenSpriteSetPlacement.WholeSet(6),
            [ScreenColourGroup.EndingExplosionBackground] = ScreenBackgroundSetPlacement.WholeSet(7),
            [ScreenColourGroup.EndingFlashBackground] = ScreenBackgroundSetPlacement.WholeSet(1),
            [ScreenColourGroup.EndingFlashBackgroundTransparent] = new ScreenBackgroundSetPlacement(1, 0, 1),
            [ScreenColourGroup.EndingFlashSpriteTransparent] = new ScreenSpriteSetPlacement(0, 0, 1),
            [ScreenColourGroup.EndingFinalGunship] = ScreenBackgroundSetPlacement.WholeSet(5),
            [ScreenColourGroup.EndingAnimalEscape] = ScreenSpriteSetPlacement.WholeSet(7),
            [ScreenColourGroup.EndingLogoBackground] = ScreenBackgroundSetPlacement.WholeSet(1),
            [ScreenColourGroup.EndingLogoSprites] = ScreenSpriteSetPlacement.WholeSet(7),
            [ScreenColourGroup.EndingPostCreditsRestoredBackground] = new ScreenBackgroundSetPlacement(0, 4, 124),
            [ScreenColourGroup.EndingPostCreditsRestoredSprites] = new ScreenSpriteSetPlacement(0, 0, 128),
            [ScreenColourGroup.EndingWaitingBackdrop] = ScreenBackgroundSetPlacement.WholeSet(2),
            [ScreenColourGroup.EndingSuitlessSamus] = ScreenSpriteSetPlacement.WholeSet(5),
            [ScreenColourGroup.EndingSuitedSamus] = ScreenSpriteSetPlacement.WholeSet(6),
            [ScreenColourGroup.EndingFallingSamus] = ScreenSpriteSetPlacement.WholeSet(6),
            [ScreenColourGroup.EndingShootingSamus] = ScreenSpriteSetPlacement.WholeSet(7),
            [ScreenColourGroup.EndingShot] = ScreenSpriteSetPlacement.WholeSet(4),
            [ScreenColourGroup.CeresNavigationLights] = new ScreenBackgroundSetPlacement(6, 13, 3),
            [ScreenColourGroup.IntroOldMotherBrainLights] = new ScreenBackgroundSetPlacement(1, 4, 12),
            [ScreenColourGroup.EndingWideExplosionForeground] = new ScreenBackgroundSetPlacement(0, 1, 15),
            [ScreenColourGroup.EndingWideExplosionBackground] = new ScreenBackgroundSetPlacement(1, 1, 15),
            [ScreenColourGroup.EndingExplosionFinale] = new ScreenBackgroundSetPlacement(0, 1, 15),
            [ScreenColourGroup.EndingExplosionSpace] = ScreenBackgroundSetPlacement.WholeSet(0),
            [ScreenColourGroup.EndingExplodingZebesLava] = ScreenBackgroundSetPlacement.WholeSet(4),
            [ScreenColourGroup.EndingExplodingZebesCrust] = new ScreenBackgroundSetPlacement(4, 1, 15),
            [ScreenColourGroup.EndingExplodingZebesGreyClouds] = new ScreenBackgroundSetPlacement(5, 1, 15),
            [ScreenColourGroup.EndingGunshipEmerging] = ScreenBackgroundSetPlacement.WholeSet(5),
            [ScreenColourGroup.UnusedCinematicFade] = ScreenBackgroundSetPlacement.WholeSet(5),
            [ScreenColourGroup.TitleLogoGlow] = new ScreenSpriteSetPlacement(2, 1, 15),
            [ScreenColourGroup.NintendoBootLogo] = new ScreenSpriteSetPlacement(1, 9, 7),
            [ScreenColourGroup.PlanetZebesText] = new ScreenSpriteSetPlacement(0, 1, 15),
            [ScreenColourGroup.EndingGunshipGlow] = new ScreenSpriteSetPlacement(7, 15, 1),
            [ScreenColourGroup.EndingZoomedOutExplodingZebes] = ScreenSpriteSetPlacement.WholeSet(7),
            [ScreenColourGroup.EndingPostCreditsIcon] = ScreenSpriteSetPlacement.WholeSet(7),
        };
}
