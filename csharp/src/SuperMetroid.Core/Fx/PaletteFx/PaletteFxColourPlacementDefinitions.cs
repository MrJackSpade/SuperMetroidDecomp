using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Whose colours each palette-FX colour group is (#1464): the one mapping from the colours an effect animates to the
/// owner holding them, one of gameplay's background sets, a sprite set or Samus's suit or beams, and that owner's colour
/// the group starts at; or the frontend screen, which resolves a title or cinematic effect's colours by the group's name.
/// Only the palette-FX handler reads a placement; effects, programs and definitions name the colour group.
/// </summary>
internal static class PaletteFxColourPlacementDefinitions
{
    /// <summary>The owner's colour each colour group's first written colour is.</summary>
    internal static IReadOnlyDictionary<PaletteFxColours, PaletteFxColourPlacement> Placements { get; } =
        new Dictionary<PaletteFxColours, PaletteFxColourPlacement>
        {
            [PaletteFxColours.TitleLogo] = new SpriteSetPlacement(2, 1),
            [PaletteFxColours.NintendoBootLogo] = new SpriteSetPlacement(1, 9),
            [PaletteFxColours.NintendoCopyright] = new SamusSuitPlacement(9),
            [PaletteFxColours.BabyMetroidTubeLight] = new ScreenColoursPlacement(PaletteFxColours.BabyMetroidTubeLight),
            [PaletteFxColours.TitleScreenDisplays] = new ScreenColoursPlacement(PaletteFxColours.TitleScreenDisplays),
            [PaletteFxColours.CutsceneGunshipEngine] = new ScreenColoursPlacement(PaletteFxColours.CutsceneGunshipEngine),
            [PaletteFxColours.CeresNavigationLightsSprite] = new SamusBeamsPlacement(13),
            [PaletteFxColours.CeresNavigationLightsBackground] = new ScreenColoursPlacement(PaletteFxColours.CeresNavigationLightsBackground),
            [PaletteFxColours.PlanetZebesText] = new SpriteSetPlacement(0, 1),
            [PaletteFxColours.OldMotherBrainBackgroundLights] = new ScreenColoursPlacement(PaletteFxColours.OldMotherBrainBackgroundLights),
            [PaletteFxColours.GunshipGlow] = new SpriteSetPlacement(7, 15),
            [PaletteFxColours.ZoomedOutExplodingZebes] = new SpriteSetPlacement(7, 0),
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
            [PaletteFxColours.PostCreditsSuperMetroidIcon] = new SpriteSetPlacement(7, 0),
            [PaletteFxColours.DraygonStatue] = new BackgroundSetPlacement(BackgroundColourSet.TilesFifth, 0),
            [PaletteFxColours.KraidStatue] = new BackgroundSetPlacement(BackgroundColourSet.TilesSixth, 0),
            [PaletteFxColours.RidleyStatue] = new SpriteSetPlacement(1, 0),
            [PaletteFxColours.PhantoonStatue] = new SpriteSetPlacement(2, 0),
            [PaletteFxColours.TorizoBelly] = new SpriteSetPlacement(1, 9),
            [PaletteFxColours.HeatSuitGlow] = new SamusSuitPlacement(1),
            [PaletteFxColours.WreckedShipGreenLights] = new BackgroundSetPlacement(BackgroundColourSet.TilesThird, 12),
            [PaletteFxColours.LandingSiteLightning] = new BackgroundSetPlacement(BackgroundColourSet.TilesFourth, 4),
            [PaletteFxColours.UnusedDarkLightning] = new BackgroundSetPlacement(BackgroundColourSet.TilesThird, 1),
            [PaletteFxColours.BrinstarBlueSpores] = new BackgroundSetPlacement(BackgroundColourSet.TilesSixth, 1),
            [PaletteFxColours.BrinstarRedGlow] = new BackgroundSetPlacement(BackgroundColourSet.TilesFifth, 4),
            [PaletteFxColours.Beacon] = new BackgroundSetPlacement(BackgroundColourSet.TilesSixth, 1),
            [PaletteFxColours.NorfairHeatForeground] = new BackgroundSetPlacement(BackgroundColourSet.TilesSecond, 5),
            [PaletteFxColours.NorfairForegroundPalette4] = new BackgroundSetPlacement(BackgroundColourSet.TilesThird, 1),
            [PaletteFxColours.NorfairForegroundPalette5] = new BackgroundSetPlacement(BackgroundColourSet.TilesFourth, 1),
            [PaletteFxColours.NorfairForegroundPalette6] = new BackgroundSetPlacement(BackgroundColourSet.TilesFifth, 1),
            [PaletteFxColours.MaridiaSandPits] = new BackgroundSetPlacement(BackgroundColourSet.TilesFirst, 4),
            [PaletteFxColours.MaridiaSandFalls] = new BackgroundSetPlacement(BackgroundColourSet.TilesFirst, 8),
            [PaletteFxColours.MaridiaBackgroundWaterfalls] = new BackgroundSetPlacement(BackgroundColourSet.TilesSecond, 4),
            [PaletteFxColours.TourianArkanoidBlocksAndOrbs] = new BackgroundSetPlacement(BackgroundColourSet.TilesSixth, 4),
            [PaletteFxColours.TourianShutters] = new SpriteSetPlacement(1, 9),
            [PaletteFxColours.TourianBackground] = new BackgroundSetPlacement(BackgroundColourSet.TilesSecond, 8),
            [PaletteFxColours.TourianLevel] = new BackgroundSetPlacement(BackgroundColourSet.TilesFourth, 4),
            [PaletteFxColours.OldTourianLevel] = new BackgroundSetPlacement(BackgroundColourSet.TilesFourth, 1),
            [PaletteFxColours.OldTourianRailings] = new BackgroundSetPlacement(BackgroundColourSet.TilesFifth, 9),
            [PaletteFxColours.OldTourianPanels] = new BackgroundSetPlacement(BackgroundColourSet.TilesFourth, 5),
            [PaletteFxColours.UpperCrateriaLevel] = new BackgroundSetPlacement(BackgroundColourSet.TilesThird, 1),
            [PaletteFxColours.CrateriaEscapeLightning] = new BackgroundSetPlacement(BackgroundColourSet.TilesFourth, 1),
            [PaletteFxColours.CreBlockPixel] = new BackgroundSetPlacement(BackgroundColourSet.TilesFourth, 7),
        };
}
