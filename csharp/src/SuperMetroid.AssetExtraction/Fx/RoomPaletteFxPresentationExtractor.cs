using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Extracts authored room and cinematic palette animation colors to RGB5 JSON.</summary>
internal static class RoomPaletteFxPresentationExtractor
{
    /// <summary>Reads compiled room and cinematic palette-FX source colors and writes their RGB5 presentation document.</summary>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        using var json = new MemoryStream();
        RoomPaletteFxPresentationFile.Write(json, new RoomPaletteFxPresentationDocument
        {
            Version = RoomPaletteFxPresentationFormat.Version,
            SamusHeatPowerSuit = ExtractFrames(Heat(PaletteFxHeatSuit.Power)),
            SamusHeatVariaSuit = ExtractFrames(Heat(PaletteFxHeatSuit.Varia)),
            SamusHeatGravitySuit = ExtractFrames(Heat(PaletteFxHeatSuit.Gravity)),
            NorfairForegroundAndHeatPhase = ExtractFrames(Norfair(NorfairEnvironmentalPaletteOwner.ForegroundAndHeatPhase)),
            NorfairGreenForeground = ExtractFrames(Norfair(NorfairEnvironmentalPaletteOwner.GreenForeground)),
            NorfairPurpleForeground = ExtractFrames(Norfair(NorfairEnvironmentalPaletteOwner.PurpleForeground)),
            NorfairDarkForeground = ExtractFrames(Norfair(NorfairEnvironmentalPaletteOwner.DarkForeground)),
            MaridiaSandPits = ExtractFrames(Maridia(MaridiaEnvironmentalPaletteOwner.SandPits)),
            MaridiaSandFalls = ExtractFrames(Maridia(MaridiaEnvironmentalPaletteOwner.SandFalls)),
            MaridiaBackgroundWaterfalls = ExtractFrames(Maridia(MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls)),
            WreckedShipGreenLights = ExtractFrames(WreckedShipGreenLightPaletteFxColorDefinitions.ColorFrames),
            RedBrinstarBackgroundGlow = ExtractFrames(RedBrinstarGlowPaletteFxColorDefinitions.ColorFrames),
            TourianGlow = ExtractFrames(TourianGlowPaletteFxColorDefinitions.ColorFrames),
            BrinstarBlueSpores = ExtractFrames(BrinstarBlueSporePaletteFxColorDefinitions.All[0].ColorFrames),
            BombTorizoBelly = ExtractFrames(TorizoBellyPaletteFxColorDefinitions.All.Single(item => item.Owner == TorizoBellyPaletteOwner.BombTorizo).ColorFrames),
            GoldenTorizoBelly = ExtractFrames(TorizoBellyPaletteFxColorDefinitions.All.Single(item => item.Owner == TorizoBellyPaletteOwner.GoldenTorizo).ColorFrames),
            TourianStatueGrey = ExtractFrames(TourianStatueGreyPaletteFxColorDefinitions.ColorFrames),
            CrateriaSurfaceLightning = ExtractFrames(CrateriaLightningPaletteFxColorDefinitions.Surface.ColorFrames),
            CeresGunshipEngineLights = ExtractFrames(CeresCinematicLightPaletteFxColorDefinitions.GunshipEngineFrames),
            CeresNavigationLights = ExtractFrames(CeresCinematicLightPaletteFxColorDefinitions.NavigationLightsFrames),
            PlanetZebesTextFadeIn = ExtractFrames(PlanetZebesTextPaletteFxColorDefinitions.All.Single(
                item => item.Owner == PlanetZebesTextPaletteFxProgramOwner.FadeIn).ColorFrames),
            PlanetZebesTextFadeOut = ExtractFrames(PlanetZebesTextPaletteFxColorDefinitions.All.Single(
                item => item.Owner == PlanetZebesTextPaletteFxProgramOwner.FadeOut).ColorFrames),
            OldMotherBrainBackgroundLights = ExtractFrames(CinematicGlowPaletteFxColorDefinitions.All.Single(
                item => item.Owner == CinematicGlowPaletteFxProgramOwner.OldMotherBrainBackgroundLights).ColorFrames),
            CinematicGunshipGlow = ExtractFrames(CinematicGlowPaletteFxColorDefinitions.All.Single(
                item => item.Owner == CinematicGlowPaletteFxProgramOwner.GunshipGlow).ColorFrames),
            ExplodingZebesFade = ExtractFrames(ExplodingZebesFadePaletteFxColorDefinitions.ColorFrames),
            TitleLogoFade = ExtractFrames(TitleLogoFadePaletteFxColorDefinitions.ColorFrames),
            NintendoSharedFade = ExtractFrames(NintendoLogoFadePaletteFxColorDefinitions.ColorFrames),
            ZebesExplosionForeground = ExtractFrames(ZebesExplosionForegroundPaletteFxColorDefinitions.ColorFrames),
            ZebesExplosionFinale = ExtractFrames(ZebesExplosionFinalePaletteFxColorDefinitions.ColorFrames),
            ZebesExplosionWhiteout = ExtractFrames(ZebesExplosionWhiteoutPaletteFxColorDefinitions.ColorFrames),
            ZebesExplosionAfterglow = ExtractFrames(ZebesExplosionAmbientPaletteFxColorDefinitions.All.Single(
                item => item.Owner == ZebesExplosionAmbientPaletteFxProgramOwner.PlanetAfterglow).ColorFrames),
            ZebesExplosionLava = ExtractFrames(ZebesExplosionAmbientPaletteFxColorDefinitions.All.Single(
                item => item.Owner == ZebesExplosionAmbientPaletteFxProgramOwner.Lava).ColorFrames),
            ZebesExplosionCrust = ExtractFrames(ZebesExplosionLayerFadePaletteFxColorDefinitions.All.Single(
                item => item.Owner == ZebesExplosionLayerFadePaletteFxProgramOwner.Crust).ColorFrames),
            ZebesExplosionGreyClouds = ExtractFrames(ZebesExplosionLayerFadePaletteFxColorDefinitions.All.Single(
                item => item.Owner == ZebesExplosionLayerFadePaletteFxProgramOwner.GreyClouds).ColorFrames),
            ZebesExplosionGunship = ExtractFrames(ZebesExplosionGunshipPaletteFxColorDefinitions.ColorFrames),
            SamusLoadingPowerSuit = ExtractFrames(Loading(SamusLoadingSuitPaletteFxProgramOwner.PowerSuit)),
            SamusLoadingVariaSuit = ExtractFrames(Loading(SamusLoadingSuitPaletteFxProgramOwner.VariaSuit)),
            SamusLoadingGravitySuit = ExtractFrames(Loading(SamusLoadingSuitPaletteFxProgramOwner.GravitySuit)),
            PostCreditsIconGlare = ExtractFrames(PostCreditsIconGlarePaletteFxColorDefinitions.ColorFrames),
            TourianEscapeShutter = ExtractFrames(TourianEscapeRedFlashPaletteFxColorDefinitions.All.Single(
                item => item.Owner == TourianEscapeRedFlashPaletteOwner.Shutter).ColorFrames),
            TourianEscapeBackground = ExtractFrames(TourianEscapeRedFlashPaletteFxColorDefinitions.All.Single(
                item => item.Owner == TourianEscapeRedFlashPaletteOwner.Background).ColorFrames),
            TourianEscapeSharedRedFlash = ExtractFrames(TourianEscapeSharedRedFlashPaletteFxColorDefinitions.ColorFrames),
            OldTourianEscapeRedFlash = ExtractFrames(OldTourianEscapeRedFlashPaletteFxColorDefinitions.ColorFrames),
            OldTourianEscapeOrangeRailings = ExtractFrames(OldTourianEscapeAccentPaletteFxColorDefinitions.All.Single(
                item => item.Owner == OldTourianEscapeAccentPaletteOwner.OrangeRailings).ColorFrames),
            OldTourianEscapeYellowPanels = ExtractFrames(OldTourianEscapeAccentPaletteFxColorDefinitions.All.Single(
                item => item.Owner == OldTourianEscapeAccentPaletteOwner.YellowPanels).ColorFrames),
            UpperCrateriaEscapeRedFlash = ExtractFrames(UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.ColorFrames),
            CrateriaEscapeYellowLightning = ExtractFrames(CrateriaEscapeLightningPaletteFxColorDefinitions.All.Single(
                item => item.Owner == CrateriaEscapeLightningPaletteOwner.YellowLightning).ColorFrames),
            ParlorEscapeSky = ExtractFrames(CrateriaEscapeLightningPaletteFxColorDefinitions.All.Single(
                item => item.Owner == CrateriaEscapeLightningPaletteOwner.ParlorSky).ColorFrames),
            BeaconFlashing = ExtractFrames(BeaconPaletteFxColorDefinitions.ColorFrames),
        });
        return json.ToArray();

        static PaletteFxColorFrames Norfair(NorfairEnvironmentalPaletteOwner owner) =>
            NorfairEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == owner).ColorFrames;

        static PaletteFxColorFrames Maridia(MaridiaEnvironmentalPaletteOwner owner) =>
            MaridiaEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == owner).ColorFrames;

        static PaletteFxColorFrames Heat(PaletteFxHeatSuit suit) =>
            PaletteFxHeatProgramMechanicsDefinitions.All.Single(item => item.Suit == suit).ColorFrames;

        static PaletteFxColorFrames Loading(SamusLoadingSuitPaletteFxProgramOwner owner) =>
            SamusLoadingSuitPaletteFxColorDefinitions.All.Single(item => item.Owner == owner).ColorFrames;

        PaletteRgb5[][] ExtractFrames(PaletteFxColorFrames colorFrames) => PaletteFxColorExtraction.ReadFrames(bus, colorFrames);
    }
}
