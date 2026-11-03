using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Whose colours each palette-FX colour group is (#1464): the one mapping from the colours an effect animates to the
/// owner holding them, a sprite set or Samus's suit or beams, and that owner's colour the group starts at; or the frontend
/// screen, which resolves a title or cinematic effect's colours by the group's name. The room's own groups are shades of
/// its pictures (#1512), which the import places.
/// Only the palette-FX handler reads a placement; effects, programs and definitions name the colour group.
/// </summary>
internal static class PaletteFxColourPlacementDefinitions
{
    /// <summary>The owner's colour each colour group's first written colour is.</summary>
    internal static IReadOnlyDictionary<PaletteFxColours, PaletteFxColourPlacement> Placements { get; } =
        new Dictionary<PaletteFxColours, PaletteFxColourPlacement>
        {
            [PaletteFxColours.TitleLogo] = new ScreenColoursPlacement(PaletteFxColours.TitleLogo),
            [PaletteFxColours.NintendoBootLogo] = new ScreenColoursPlacement(PaletteFxColours.NintendoBootLogo),
            [PaletteFxColours.NintendoCopyright] = new SamusSuitPlacement(9),
            [PaletteFxColours.BabyMetroidTubeLight] = new ScreenColoursPlacement(PaletteFxColours.BabyMetroidTubeLight),
            [PaletteFxColours.TitleScreenDisplays] = new ScreenColoursPlacement(PaletteFxColours.TitleScreenDisplays),
            [PaletteFxColours.CutsceneGunshipEngine] = new ScreenColoursPlacement(PaletteFxColours.CutsceneGunshipEngine),
            [PaletteFxColours.CeresNavigationLightsSprite] = new SamusBeamsPlacement(13),
            [PaletteFxColours.CeresNavigationLightsBackground] = new ScreenColoursPlacement(PaletteFxColours.CeresNavigationLightsBackground),
            [PaletteFxColours.PlanetZebesText] = new ScreenColoursPlacement(PaletteFxColours.PlanetZebesText),
            [PaletteFxColours.OldMotherBrainBackgroundLights] = new ScreenColoursPlacement(PaletteFxColours.OldMotherBrainBackgroundLights),
            [PaletteFxColours.GunshipGlow] = new ScreenColoursPlacement(PaletteFxColours.GunshipGlow),
            [PaletteFxColours.ZoomedOutExplodingZebes] = new ScreenColoursPlacement(PaletteFxColours.ZoomedOutExplodingZebes),
            [PaletteFxColours.WideZebesExplosionForeground] = new ScreenColoursPlacement(PaletteFxColours.WideZebesExplosionForeground),
            [PaletteFxColours.WideZebesExplosionBackground] = new ScreenColoursPlacement(PaletteFxColours.WideZebesExplosionBackground),
            [PaletteFxColours.ZebesExplosionFinale] = new ScreenColoursPlacement(PaletteFxColours.ZebesExplosionFinale),
            [PaletteFxColours.SpaceDuringZebesExplosion] = new ScreenColoursPlacement(PaletteFxColours.SpaceDuringZebesExplosion),
            [PaletteFxColours.ZebesPlanetAfterglow] = new SamusBeamsPlacement(1),
            [PaletteFxColours.ExplodingZebesLava] = new ScreenColoursPlacement(PaletteFxColours.ExplodingZebesLava),
            [PaletteFxColours.ExplodingZebesCrust] = new ScreenColoursPlacement(PaletteFxColours.ExplodingZebesCrust),
            [PaletteFxColours.ExplodingZebesGreyClouds] = new ScreenColoursPlacement(PaletteFxColours.ExplodingZebesGreyClouds),
            [PaletteFxColours.GunshipEmergingFromExplosion] = new ScreenColoursPlacement(PaletteFxColours.GunshipEmergingFromExplosion),
            [PaletteFxColours.UnusedCinematicFade] = new ScreenColoursPlacement(PaletteFxColours.UnusedCinematicFade),
            [PaletteFxColours.HyperBeam] = new SamusBeamsPlacement(1),
            [PaletteFxColours.SamusLoadingSuit] = new SamusSuitPlacement(0),
            [PaletteFxColours.PostCreditsSuperMetroidIcon] = new ScreenColoursPlacement(PaletteFxColours.PostCreditsSuperMetroidIcon),
            [PaletteFxColours.RidleyStatue] = new SpriteSetPlacement(SpriteColourStore.EnemiesFirst, 0),
            [PaletteFxColours.PhantoonStatue] = new SpriteSetPlacement(SpriteColourStore.EnemiesSecond, 0),
            [PaletteFxColours.TorizoBelly] = new SpriteSetPlacement(SpriteColourStore.EnemiesFirst, 9),
            [PaletteFxColours.HeatSuitGlow] = new SamusSuitPlacement(1),
            [PaletteFxColours.TourianShutters] = new SpriteSetPlacement(SpriteColourStore.EnemiesFirst, 9),
        };
}
