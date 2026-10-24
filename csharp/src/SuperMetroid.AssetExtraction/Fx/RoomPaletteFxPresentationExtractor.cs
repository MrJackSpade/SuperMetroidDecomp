using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Extracts authored room and cinematic palette animation colors to RGB5 JSON.</summary>
[CpuBusBoundary("codec: ROM room and cinematic palette-FX colors → palette-FX presentation")]
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
            SamusHeatPowerSuit = ExtractFrames(HeatPaletteColorDefinitions.Rows(PaletteFxHeatSuit.Power)),
            SamusHeatVariaSuit = ExtractFrames(HeatPaletteColorDefinitions.Rows(PaletteFxHeatSuit.Varia)),
            SamusHeatGravitySuit = ExtractFrames(HeatPaletteColorDefinitions.Rows(PaletteFxHeatSuit.Gravity)),
            NorfairForegroundAndHeatPhase = ExtractFrames(Norfair(NorfairEnvironmentalPaletteOwner.ForegroundAndHeatPhase)),
            NorfairForegroundPalette4 = ExtractFrames(Norfair(NorfairEnvironmentalPaletteOwner.ForegroundPalette4)),
            NorfairForegroundPalette5 = ExtractFrames(Norfair(NorfairEnvironmentalPaletteOwner.ForegroundPalette5)),
            NorfairForegroundPalette6 = ExtractFrames(Norfair(NorfairEnvironmentalPaletteOwner.ForegroundPalette6)),
            MaridiaSandPits = ExtractFrames(Maridia(MaridiaEnvironmentalPaletteOwner.SandPits)),
            MaridiaSandFalls = ExtractFrames(Maridia(MaridiaEnvironmentalPaletteOwner.SandFalls)),
            MaridiaBackgroundWaterfalls = ExtractFrames(Maridia(MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls)),
            WreckedShipGreenLights = ExtractFrames(WreckedShipGreenLightPaletteFxColorDefinitions.Rows),
            RedBrinstarBackgroundGlow = ExtractFrames(RedBrinstarGlowPaletteFxColorDefinitions.Rows),
            TourianGlow = ExtractFrames(TourianGlowPaletteFxColorDefinitions.Rows),
            BrinstarBlueSpores = ExtractFrames(BrinstarBlueSporePaletteFxColorDefinitions.All[0].Rows),
            BombTorizoBelly = ExtractFrames(TorizoBellyPaletteFxColorDefinitions.All.Single(item => item.Owner == TorizoBellyPaletteOwner.BombTorizo).Rows),
            GoldenTorizoBelly = ExtractFrames(TorizoBellyPaletteFxColorDefinitions.All.Single(item => item.Owner == TorizoBellyPaletteOwner.GoldenTorizo).Rows),
            TourianStatueGrey = ExtractFrames(TourianStatueGreyPaletteFxColorDefinitions.Rows),
            CrateriaSurfaceLightning = ExtractFrames(CrateriaLightningPaletteFxColorDefinitions.All.Single(
                item => item.Owner == CrateriaLightningPaletteOwner.SurfaceLightning).Rows),
            CrateriaUnusedDarkLightning = ExtractFrames(CrateriaLightningPaletteFxColorDefinitions.All.Single(
                item => item.Owner == CrateriaLightningPaletteOwner.UnusedDarkLightning).Rows),
            CeresGunshipEngineLights = ExtractFrames(CeresCinematicLightPaletteFxColorDefinitions.GunshipEngineRows),
            CeresNavigationLights = ExtractFrames(CeresCinematicLightPaletteFxColorDefinitions.NavigationLightsRows),
            PlanetZebesTextFadeIn = ExtractFrames(PlanetZebesTextPaletteFxColorDefinitions.All.Single(
                item => item.Owner == PlanetZebesTextPaletteFxProgramOwner.FadeIn).Rows),
            PlanetZebesTextFadeOut = ExtractFrames(PlanetZebesTextPaletteFxColorDefinitions.All.Single(
                item => item.Owner == PlanetZebesTextPaletteFxProgramOwner.FadeOut).Rows),
            OldMotherBrainBackgroundLights = ExtractFrames(CinematicGlowPaletteFxColorDefinitions.All.Single(
                item => item.Owner == CinematicGlowPaletteFxProgramOwner.OldMotherBrainBackgroundLights).Rows),
            CinematicGunshipGlow = ExtractFrames(CinematicGlowPaletteFxColorDefinitions.All.Single(
                item => item.Owner == CinematicGlowPaletteFxProgramOwner.GunshipGlow).Rows),
            ExplodingZebesFade = ExtractFrames(ExplodingZebesFadePaletteFxColorDefinitions.Rows),
            UnusedCinematicFade = ExtractFrames(UnusedCinematicFadePaletteFxColorDefinitions.Rows),
            TitleLogoFade = ExtractFrames(TitleLogoFadePaletteFxColorDefinitions.Rows),
            NintendoSharedFade = ExtractFrames(NintendoLogoFadePaletteFxColorDefinitions.Rows),
            ZebesExplosionForeground = ExtractFrames(ZebesExplosionForegroundPaletteFxColorDefinitions.Rows),
            ZebesExplosionFinale = ExtractFrames(ZebesExplosionFinalePaletteFxColorDefinitions.Rows),
            ZebesExplosionWhiteout = ExtractFrames(ZebesExplosionWhiteoutPaletteFxColorDefinitions.Rows),
            ZebesExplosionAfterglow = ExtractFrames(ZebesExplosionAmbientPaletteFxColorDefinitions.All.Single(
                item => item.Owner == ZebesExplosionAmbientPaletteFxProgramOwner.PlanetAfterglow).Rows),
            ZebesExplosionLava = ExtractFrames(ZebesExplosionAmbientPaletteFxColorDefinitions.All.Single(
                item => item.Owner == ZebesExplosionAmbientPaletteFxProgramOwner.Lava).Rows),
            ZebesExplosionCrust = ExtractFrames(ZebesExplosionLayerFadePaletteFxColorDefinitions.All.Single(
                item => item.Owner == ZebesExplosionLayerFadePaletteFxProgramOwner.Crust).Rows),
            ZebesExplosionGreyClouds = ExtractFrames(ZebesExplosionLayerFadePaletteFxColorDefinitions.All.Single(
                item => item.Owner == ZebesExplosionLayerFadePaletteFxProgramOwner.GreyClouds).Rows),
            ZebesExplosionGunship = ExtractFrames(ZebesExplosionGunshipPaletteFxColorDefinitions.Rows),
            SamusLoadingPowerSuit = ExtractFrames(LoadingPaletteColorDefinitions.Rows(PaletteFxHeatSuit.Power)),
            SamusLoadingVariaSuit = ExtractFrames(LoadingPaletteColorDefinitions.Rows(PaletteFxHeatSuit.Varia)),
            SamusLoadingGravitySuit = ExtractFrames(LoadingPaletteColorDefinitions.Rows(PaletteFxHeatSuit.Gravity)),
            PostCreditsIconGlare = ExtractFrames(PostCreditsIconGlarePaletteFxColorDefinitions.Rows),
            TourianEscapeShutter = ExtractFrames(TourianEscapeRedFlashPaletteFxColorDefinitions.All.Single(
                item => item.Owner == TourianEscapeRedFlashPaletteOwner.Shutter).Rows),
            TourianEscapeBackground = ExtractFrames(TourianEscapeRedFlashPaletteFxColorDefinitions.All.Single(
                item => item.Owner == TourianEscapeRedFlashPaletteOwner.Background).Rows),
            TourianEscapeSharedRedFlash = ExtractFrames(TourianEscapeSharedRedFlashPaletteFxColorDefinitions.Rows),
            OldTourianEscapeRedFlash = ExtractFrames(OldTourianEscapeRedFlashPaletteFxColorDefinitions.Rows),
            OldTourianEscapeOrangeRailings = ExtractFrames(OldTourianEscapeAccentPaletteFxColorDefinitions.All.Single(
                item => item.Owner == OldTourianEscapeAccentPaletteOwner.OrangeRailings).Rows),
            OldTourianEscapeYellowPanels = ExtractFrames(OldTourianEscapeAccentPaletteFxColorDefinitions.All.Single(
                item => item.Owner == OldTourianEscapeAccentPaletteOwner.YellowPanels).Rows),
            UpperCrateriaEscapeRedFlash = ExtractFrames(UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.Rows),
            CrateriaEscapeYellowLightning = ExtractFrames(CrateriaEscapeLightningPaletteFxColorDefinitions.All.Single(
                item => item.Owner == CrateriaEscapeLightningPaletteOwner.YellowLightning).Rows),
            CrateriaEscapeCreBlockPixel = ExtractFrames(CrateriaEscapeLightningPaletteFxColorDefinitions.All.Single(
                item => item.Owner == CrateriaEscapeLightningPaletteOwner.CreBlockPixel).Rows),
            BeaconFlashing = ExtractFrames(BeaconPaletteFxColorDefinitions.Rows),
        });
        return json.ToArray();

        static PaletteFxColorRows Norfair(NorfairEnvironmentalPaletteOwner owner) =>
            NorfairEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == owner).Rows;

        static PaletteFxColorRows Maridia(MaridiaEnvironmentalPaletteOwner owner) =>
            MaridiaEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == owner).Rows;

        PaletteRgb5[][] ExtractFrames(PaletteFxColorRows rows) => PaletteFxColorExtraction.ReadFrames(bus, rows);
    }
}
