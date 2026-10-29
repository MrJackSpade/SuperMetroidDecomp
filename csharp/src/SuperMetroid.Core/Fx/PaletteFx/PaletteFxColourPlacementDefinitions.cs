namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Where each palette-FX colour group is held (#1464): the one mapping from the colours an effect animates to the
/// owner's colours they start at, kept until colour sets are keyed by owner. Only the palette-FX handler and the
/// title's ambient colours, which derive from the title's initial palette, read a placement; effects, programs and
/// definitions name the colour group.
/// </summary>
internal static class PaletteFxColourPlacementDefinitions
{
    /// <summary>The owner's colour each colour group's first written colour lands on.</summary>
    internal static IReadOnlyDictionary<PaletteFxColours, PaletteFxColourPlacement> Placements { get; } =
        new Dictionary<PaletteFxColours, PaletteFxColourPlacement>
        {
            [PaletteFxColours.TitleLogo] = new(PaletteFxColourOwner.Sprite, 33),
            [PaletteFxColours.NintendoBootLogo] = new(PaletteFxColourOwner.Sprite, 25),
            [PaletteFxColours.NintendoCopyright] = new(PaletteFxColourOwner.SamusSuit, 9),
            [PaletteFxColours.BabyMetroidTubeLight] = new(PaletteFxColourOwner.Background, 42),
            [PaletteFxColours.TitleScreenDisplays] = new(PaletteFxColourOwner.Background, 46),
            [PaletteFxColours.CutsceneGunshipEngine] = new(PaletteFxColourOwner.Background, 95),
            [PaletteFxColours.CeresNavigationLightsSprite] = new(PaletteFxColourOwner.SamusBeams, 13),
            [PaletteFxColours.CeresNavigationLightsBackground] = new(PaletteFxColourOwner.Background, 109),
            [PaletteFxColours.PlanetZebesText] = new(PaletteFxColourOwner.Sprite, 1),
            [PaletteFxColours.OldMotherBrainBackgroundLights] = new(PaletteFxColourOwner.Background, 20),
            [PaletteFxColours.GunshipGlow] = new(PaletteFxColourOwner.Sprite, 127),
            [PaletteFxColours.ZoomedOutExplodingZebes] = new(PaletteFxColourOwner.Sprite, 112),
            [PaletteFxColours.WideZebesExplosionForeground] = new(PaletteFxColourOwner.Background, 1),
            [PaletteFxColours.WideZebesExplosionBackground] = new(PaletteFxColourOwner.Background, 17),
            [PaletteFxColours.ZebesExplosionFinale] = new(PaletteFxColourOwner.Background, 1),
            [PaletteFxColours.SpaceDuringZebesExplosion] = new(PaletteFxColourOwner.Background, 0),
            [PaletteFxColours.ZebesPlanetAfterglow] = new(PaletteFxColourOwner.SamusBeams, 1),
            [PaletteFxColours.ExplodingZebesLava] = new(PaletteFxColourOwner.Background, 64),
            [PaletteFxColours.ExplodingZebesCrust] = new(PaletteFxColourOwner.Background, 65),
            [PaletteFxColours.ExplodingZebesGreyClouds] = new(PaletteFxColourOwner.Background, 81),
            [PaletteFxColours.GunshipEmergingFromExplosion] = new(PaletteFxColourOwner.Background, 80),
            [PaletteFxColours.UnusedCinematicFade] = new(PaletteFxColourOwner.Background, 80),
            [PaletteFxColours.HyperBeam] = new(PaletteFxColourOwner.SamusBeams, 1),
            [PaletteFxColours.SamusLoadingSuit] = new(PaletteFxColourOwner.SamusSuit, 0),
            [PaletteFxColours.PostCreditsSuperMetroidIcon] = new(PaletteFxColourOwner.Sprite, 112),
            [PaletteFxColours.DraygonStatue] = new(PaletteFxColourOwner.Background, 96),
            [PaletteFxColours.KraidStatue] = new(PaletteFxColourOwner.Background, 112),
            [PaletteFxColours.RidleyStatue] = new(PaletteFxColourOwner.Sprite, 16),
            [PaletteFxColours.PhantoonStatue] = new(PaletteFxColourOwner.Sprite, 32),
            [PaletteFxColours.TorizoBelly] = new(PaletteFxColourOwner.Sprite, 25),
            [PaletteFxColours.HeatSuitGlow] = new(PaletteFxColourOwner.SamusSuit, 1),
            [PaletteFxColours.WreckedShipGreenLights] = new(PaletteFxColourOwner.Background, 76),
            [PaletteFxColours.LandingSiteLightning] = new(PaletteFxColourOwner.Background, 84),
            [PaletteFxColours.UnusedDarkLightning] = new(PaletteFxColourOwner.Background, 65),
            [PaletteFxColours.BrinstarBlueSpores] = new(PaletteFxColourOwner.Background, 113),
            [PaletteFxColours.BrinstarRedGlow] = new(PaletteFxColourOwner.Background, 100),
            [PaletteFxColours.Beacon] = new(PaletteFxColourOwner.Background, 113),
            [PaletteFxColours.NorfairHeatForeground] = new(PaletteFxColourOwner.Background, 53),
            [PaletteFxColours.NorfairForegroundPalette4] = new(PaletteFxColourOwner.Background, 65),
            [PaletteFxColours.NorfairForegroundPalette5] = new(PaletteFxColourOwner.Background, 81),
            [PaletteFxColours.NorfairForegroundPalette6] = new(PaletteFxColourOwner.Background, 97),
            [PaletteFxColours.MaridiaSandPits] = new(PaletteFxColourOwner.Background, 36),
            [PaletteFxColours.MaridiaSandFalls] = new(PaletteFxColourOwner.Background, 40),
            [PaletteFxColours.MaridiaBackgroundWaterfalls] = new(PaletteFxColourOwner.Background, 52),
            [PaletteFxColours.TourianArkanoidBlocksAndOrbs] = new(PaletteFxColourOwner.Background, 116),
            [PaletteFxColours.TourianShutters] = new(PaletteFxColourOwner.Sprite, 25),
            [PaletteFxColours.TourianBackground] = new(PaletteFxColourOwner.Background, 56),
            [PaletteFxColours.TourianLevel] = new(PaletteFxColourOwner.Background, 84),
            [PaletteFxColours.OldTourianLevel] = new(PaletteFxColourOwner.Background, 81),
            [PaletteFxColours.OldTourianRailings] = new(PaletteFxColourOwner.Background, 105),
            [PaletteFxColours.OldTourianPanels] = new(PaletteFxColourOwner.Background, 85),
            [PaletteFxColours.UpperCrateriaLevel] = new(PaletteFxColourOwner.Background, 65),
            [PaletteFxColours.CrateriaEscapeLightning] = new(PaletteFxColourOwner.Background, 81),
            [PaletteFxColours.CreBlockPixel] = new(PaletteFxColourOwner.Background, 87),
        };
}
