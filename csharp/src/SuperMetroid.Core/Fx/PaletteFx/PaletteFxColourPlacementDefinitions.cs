namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Whose colours each palette-FX colour group is (#1464): the one mapping from the colours an effect animates to the
/// owner holding them, a background or sprite set or Samus's suit or beams, and that owner's colour the group starts
/// at. Only the palette-FX handler reads a placement; effects, programs and definitions name the colour group.
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
            [PaletteFxColours.BabyMetroidTubeLight] = new BackgroundSetPlacement(2, 10),
            [PaletteFxColours.TitleScreenDisplays] = new BackgroundSetPlacement(2, 14),
            [PaletteFxColours.CutsceneGunshipEngine] = new BackgroundSetPlacement(5, 15),
            [PaletteFxColours.CeresNavigationLightsSprite] = new SamusBeamsPlacement(13),
            [PaletteFxColours.CeresNavigationLightsBackground] = new BackgroundSetPlacement(6, 13),
            [PaletteFxColours.PlanetZebesText] = new SpriteSetPlacement(0, 1),
            [PaletteFxColours.OldMotherBrainBackgroundLights] = new BackgroundSetPlacement(1, 4),
            [PaletteFxColours.GunshipGlow] = new SpriteSetPlacement(7, 15),
            [PaletteFxColours.ZoomedOutExplodingZebes] = new SpriteSetPlacement(7, 0),
            [PaletteFxColours.WideZebesExplosionForeground] = new BackgroundSetPlacement(0, 1),
            [PaletteFxColours.WideZebesExplosionBackground] = new BackgroundSetPlacement(1, 1),
            [PaletteFxColours.ZebesExplosionFinale] = new BackgroundSetPlacement(0, 1),
            [PaletteFxColours.SpaceDuringZebesExplosion] = new BackgroundSetPlacement(0, 0),
            [PaletteFxColours.ZebesPlanetAfterglow] = new SamusBeamsPlacement(1),
            [PaletteFxColours.ExplodingZebesLava] = new BackgroundSetPlacement(4, 0),
            [PaletteFxColours.ExplodingZebesCrust] = new BackgroundSetPlacement(4, 1),
            [PaletteFxColours.ExplodingZebesGreyClouds] = new BackgroundSetPlacement(5, 1),
            [PaletteFxColours.GunshipEmergingFromExplosion] = new BackgroundSetPlacement(5, 0),
            [PaletteFxColours.UnusedCinematicFade] = new BackgroundSetPlacement(5, 0),
            [PaletteFxColours.HyperBeam] = new SamusBeamsPlacement(1),
            [PaletteFxColours.SamusLoadingSuit] = new SamusSuitPlacement(0),
            [PaletteFxColours.PostCreditsSuperMetroidIcon] = new SpriteSetPlacement(7, 0),
            [PaletteFxColours.DraygonStatue] = new BackgroundSetPlacement(6, 0),
            [PaletteFxColours.KraidStatue] = new BackgroundSetPlacement(7, 0),
            [PaletteFxColours.RidleyStatue] = new SpriteSetPlacement(1, 0),
            [PaletteFxColours.PhantoonStatue] = new SpriteSetPlacement(2, 0),
            [PaletteFxColours.TorizoBelly] = new SpriteSetPlacement(1, 9),
            [PaletteFxColours.HeatSuitGlow] = new SamusSuitPlacement(1),
            [PaletteFxColours.WreckedShipGreenLights] = new BackgroundSetPlacement(4, 12),
            [PaletteFxColours.LandingSiteLightning] = new BackgroundSetPlacement(5, 4),
            [PaletteFxColours.UnusedDarkLightning] = new BackgroundSetPlacement(4, 1),
            [PaletteFxColours.BrinstarBlueSpores] = new BackgroundSetPlacement(7, 1),
            [PaletteFxColours.BrinstarRedGlow] = new BackgroundSetPlacement(6, 4),
            [PaletteFxColours.Beacon] = new BackgroundSetPlacement(7, 1),
            [PaletteFxColours.NorfairHeatForeground] = new BackgroundSetPlacement(3, 5),
            [PaletteFxColours.NorfairForegroundPalette4] = new BackgroundSetPlacement(4, 1),
            [PaletteFxColours.NorfairForegroundPalette5] = new BackgroundSetPlacement(5, 1),
            [PaletteFxColours.NorfairForegroundPalette6] = new BackgroundSetPlacement(6, 1),
            [PaletteFxColours.MaridiaSandPits] = new BackgroundSetPlacement(2, 4),
            [PaletteFxColours.MaridiaSandFalls] = new BackgroundSetPlacement(2, 8),
            [PaletteFxColours.MaridiaBackgroundWaterfalls] = new BackgroundSetPlacement(3, 4),
            [PaletteFxColours.TourianArkanoidBlocksAndOrbs] = new BackgroundSetPlacement(7, 4),
            [PaletteFxColours.TourianShutters] = new SpriteSetPlacement(1, 9),
            [PaletteFxColours.TourianBackground] = new BackgroundSetPlacement(3, 8),
            [PaletteFxColours.TourianLevel] = new BackgroundSetPlacement(5, 4),
            [PaletteFxColours.OldTourianLevel] = new BackgroundSetPlacement(5, 1),
            [PaletteFxColours.OldTourianRailings] = new BackgroundSetPlacement(6, 9),
            [PaletteFxColours.OldTourianPanels] = new BackgroundSetPlacement(5, 5),
            [PaletteFxColours.UpperCrateriaLevel] = new BackgroundSetPlacement(4, 1),
            [PaletteFxColours.CrateriaEscapeLightning] = new BackgroundSetPlacement(5, 1),
            [PaletteFxColours.CreBlockPixel] = new BackgroundSetPlacement(5, 7),
        };
}
