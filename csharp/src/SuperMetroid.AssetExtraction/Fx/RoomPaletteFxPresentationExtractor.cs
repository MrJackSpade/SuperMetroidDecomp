using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

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
        RoomPaletteFxPresentation.Write(json, new RoomPaletteFxPresentationDocument
        {
            Version = RoomPaletteFxPresentationFormat.Version,
            SamusHeatPowerSuit = ExtractHeat(PaletteFxHeatSuit.Power),
            SamusHeatVariaSuit = ExtractHeat(PaletteFxHeatSuit.Varia),
            SamusHeatGravitySuit = ExtractHeat(PaletteFxHeatSuit.Gravity),
            NorfairForegroundAndHeatPhase = ExtractNorfair(
                NorfairEnvironmentalPaletteOwner.ForegroundAndHeatPhase),
            NorfairForegroundPalette4 = ExtractNorfair(
                NorfairEnvironmentalPaletteOwner.ForegroundPalette4),
            NorfairForegroundPalette5 = ExtractNorfair(
                NorfairEnvironmentalPaletteOwner.ForegroundPalette5),
            NorfairForegroundPalette6 = ExtractNorfair(
                NorfairEnvironmentalPaletteOwner.ForegroundPalette6),
            MaridiaSandPits = ExtractMaridia(MaridiaEnvironmentalPaletteOwner.SandPits),
            MaridiaSandFalls = ExtractMaridia(MaridiaEnvironmentalPaletteOwner.SandFalls),
            MaridiaBackgroundWaterfalls = ExtractMaridia(
                MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls),
            WreckedShipGreenLights = ExtractFrames(
                WreckedShipGreenLightPaletteFxColorDefinitions.FrameCount,
                WreckedShipGreenLightPaletteFxColorDefinitions.ColorsPerFrame,
                WreckedShipGreenLightPaletteFxColorDefinitions.ColorPointer),
            RedBrinstarBackgroundGlow = ExtractFrames(
                RedBrinstarGlowPaletteFxColorDefinitions.FrameCount,
                RedBrinstarGlowPaletteFxColorDefinitions.ColorsPerFrame,
                RedBrinstarGlowPaletteFxColorDefinitions.ColorPointer),
            TourianGlow = ExtractFrames(
                TourianGlowPaletteFxColorDefinitions.FrameCount,
                TourianGlowPaletteFxColorDefinitions.ColorsPerFrame,
                TourianGlowPaletteFxColorDefinitions.ColorPointer),
            BrinstarBlueSpores = ExtractFrames(
                BrinstarBlueSporePaletteFxColorDefinitions.FrameCount,
                BrinstarBlueSporePaletteFxColorDefinitions.ColorsPerFrame,
                BrinstarBlueSporePaletteFxColorDefinitions.All[0].ColorPointer),
            BombTorizoBelly = ExtractTorizo(TorizoBellyPaletteOwner.BombTorizo),
            GoldenTorizoBelly = ExtractTorizo(TorizoBellyPaletteOwner.GoldenTorizo),
            TourianStatueGrey = ExtractFrames(
                TourianStatueGreyPaletteFxColorDefinitions.FrameCount,
                TourianStatueGreyPaletteFxColorDefinitions.ColorsPerFrame,
                TourianStatueGreyPaletteFxColorDefinitions.ColorPointer),
            CrateriaSurfaceLightning = ExtractCrateriaLightning(
                CrateriaLightningPaletteOwner.SurfaceLightning),
            CrateriaUnusedDarkLightning = ExtractCrateriaLightning(
                CrateriaLightningPaletteOwner.UnusedDarkLightning),
            CeresGunshipEngineLights = ExtractFrames(
                CeresCinematicLightPaletteFxColorDefinitions
                    .GunshipEngineFrameCount,
                CeresCinematicLightPaletteFxColorDefinitions
                    .GunshipEngineColorsPerFrame,
                CeresCinematicLightPaletteFxColorDefinitions
                    .GunshipEngineColorPointer),
            CeresNavigationLights = ExtractFrames(
                CeresCinematicLightPaletteFxColorDefinitions
                    .NavigationLightsFrameCount,
                CeresCinematicLightPaletteFxColorDefinitions
                    .NavigationLightsColorsPerFrame,
                CeresCinematicLightPaletteFxColorDefinitions
                    .NavigationLightsColorPointer),
            PlanetZebesTextFadeIn = ExtractPlanetZebesText(
                PlanetZebesTextPaletteFxProgramOwner.FadeIn),
            PlanetZebesTextFadeOut = ExtractPlanetZebesText(
                PlanetZebesTextPaletteFxProgramOwner.FadeOut),
            OldMotherBrainBackgroundLights = ExtractCinematicGlow(
                CinematicGlowPaletteFxProgramOwner.OldMotherBrainBackgroundLights),
            CinematicGunshipGlow = ExtractCinematicGlow(
                CinematicGlowPaletteFxProgramOwner.GunshipGlow),
            ExplodingZebesFade = ExtractFrames(
                ExplodingZebesFadePaletteFxColorDefinitions.FrameCount,
                ExplodingZebesFadePaletteFxColorDefinitions.ColorsPerFrame,
                ExplodingZebesFadePaletteFxColorDefinitions.ColorPointer),
            UnusedCinematicFade = ExtractFrames(
                UnusedCinematicFadePaletteFxColorDefinitions.FrameCount,
                UnusedCinematicFadePaletteFxColorDefinitions.ColorsPerFrame,
                UnusedCinematicFadePaletteFxColorDefinitions.ColorPointer),
            TitleLogoFade = ExtractFrames(
                TitleLogoFadePaletteFxColorDefinitions.FrameCount,
                TitleLogoFadePaletteFxColorDefinitions.ColorsPerFrame,
                TitleLogoFadePaletteFxColorDefinitions.ColorPointer),
            NintendoSharedFade = ExtractFrames(
                NintendoLogoFadePaletteFxColorDefinitions.FrameCount,
                NintendoLogoFadePaletteFxColorDefinitions.ColorsPerFrame,
                NintendoLogoFadePaletteFxColorDefinitions.ColorPointer),
            ZebesExplosionForeground = ExtractFrames(
                ZebesExplosionForegroundPaletteFxColorDefinitions.FrameCount,
                ZebesExplosionForegroundPaletteFxColorDefinitions.ColorsPerFrame,
                ZebesExplosionForegroundPaletteFxColorDefinitions.ColorPointer),
            ZebesExplosionFinale = ExtractFrames(
                ZebesExplosionFinalePaletteFxColorDefinitions.FrameCount,
                ZebesExplosionFinalePaletteFxColorDefinitions.ColorsPerFrame,
                ZebesExplosionFinalePaletteFxColorDefinitions.ColorPointer),
            ZebesExplosionWhiteout = ExtractFrames(
                ZebesExplosionWhiteoutPaletteFxColorDefinitions.FrameCount,
                ZebesExplosionWhiteoutPaletteFxColorDefinitions.ColorsPerFrame,
                ZebesExplosionWhiteoutPaletteFxColorDefinitions.ColorPointer),
            ZebesExplosionAfterglow = ExtractZebesExplosionAmbient(
                ZebesExplosionAmbientPaletteFxProgramOwner.PlanetAfterglow),
            ZebesExplosionLava = ExtractZebesExplosionAmbient(
                ZebesExplosionAmbientPaletteFxProgramOwner.Lava),
            ZebesExplosionCrust = ExtractZebesExplosionLayerFade(
                ZebesExplosionLayerFadePaletteFxProgramOwner.Crust),
            ZebesExplosionGreyClouds = ExtractZebesExplosionLayerFade(
                ZebesExplosionLayerFadePaletteFxProgramOwner.GreyClouds),
            ZebesExplosionGunship = ExtractFrames(
                ZebesExplosionGunshipPaletteFxColorDefinitions.FrameCount,
                ZebesExplosionGunshipPaletteFxColorDefinitions.ColorsPerFrame,
                ZebesExplosionGunshipPaletteFxColorDefinitions.ColorPointer),
            SamusLoadingPowerSuit = ExtractSamusLoadingSuit(
                SamusLoadingSuitPaletteFxProgramOwner.PowerSuit),
            SamusLoadingVariaSuit = ExtractSamusLoadingSuit(
                SamusLoadingSuitPaletteFxProgramOwner.VariaSuit),
            SamusLoadingGravitySuit = ExtractSamusLoadingSuit(
                SamusLoadingSuitPaletteFxProgramOwner.GravitySuit),
            PostCreditsIconGlare = ExtractFrames(
                PostCreditsIconGlarePaletteFxColorDefinitions.FrameCount,
                PostCreditsIconGlarePaletteFxColorDefinitions.ColorsPerFrame,
                PostCreditsIconGlarePaletteFxColorDefinitions.ColorPointer),
            TourianEscapeShutter = ExtractTourianEscapeRedFlash(
                TourianEscapeRedFlashPaletteOwner.Shutter),
            TourianEscapeBackground = ExtractTourianEscapeRedFlash(
                TourianEscapeRedFlashPaletteOwner.Background),
            TourianEscapeSharedRedFlash = ExtractFrames(
                TourianEscapeSharedRedFlashPaletteFxColorDefinitions.FrameCount,
                TourianEscapeSharedRedFlashPaletteFxColorDefinitions.ColorsPerFrame,
                TourianEscapeSharedRedFlashPaletteFxColorDefinitions.ColorPointer),
            OldTourianEscapeRedFlash = ExtractFrames(
                OldTourianEscapeRedFlashPaletteFxColorDefinitions.FrameCount,
                OldTourianEscapeRedFlashPaletteFxColorDefinitions.ColorsPerFrame,
                OldTourianEscapeRedFlashPaletteFxColorDefinitions.ColorPointer),
            OldTourianEscapeOrangeRailings = ExtractOldTourianEscapeAccent(
                OldTourianEscapeAccentPaletteOwner.OrangeRailings),
            OldTourianEscapeYellowPanels = ExtractOldTourianEscapeAccent(
                OldTourianEscapeAccentPaletteOwner.YellowPanels),
            UpperCrateriaEscapeRedFlash = ExtractFrames(
                UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.FrameCount,
                UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.ColorsPerFrame,
                UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.ColorPointer),
            CrateriaEscapeYellowLightning = ExtractCrateriaEscapeLightning(
                CrateriaEscapeLightningPaletteOwner.YellowLightning),
            CrateriaEscapeCreBlockPixel = ExtractCrateriaEscapeLightning(
                CrateriaEscapeLightningPaletteOwner.CreBlockPixel),
            BeaconFlashing = ExtractFrames(
                BeaconPaletteFxColorDefinitions.FrameCount,
                BeaconPaletteFxColorDefinitions.ColorsPerFrame,
                BeaconPaletteFxColorDefinitions.ColorPointer),
        });
        return json.ToArray();

        PaletteRgb5[][] ExtractHeat(PaletteFxHeatSuit suit)
        {
            PaletteFxHeatProgramDefinition definition =
                PaletteFxHeatProgramMechanicsDefinitions.All.Single(item => item.Suit == suit);
            return ExtractFrames(definition.Frames.Count,
                PaletteFxHeatProgramDefinition.ColorsPerFrame,
                (frame, index) => unchecked((ushort)(
                    definition.Frames[frame].FirstColorPointer + index * sizeof(ushort))));
        }

        PaletteRgb5[][] ExtractNorfair(NorfairEnvironmentalPaletteOwner owner)
        {
            NorfairEnvironmentalPaletteFxColorDefinition definition =
                NorfairEnvironmentalPaletteFxColorDefinitions.All.Single(
                    item => item.Owner == owner);
            int frameCount = NorfairEnvironmentalPaletteFxColorDefinitions.FrameCount;
            int colorCount = NorfairEnvironmentalPaletteFxColorDefinitions.ColorsPerFrame;
            var frames = new PaletteRgb5[frameCount][];
            for (int frame = 0; frame < frameCount; frame++)
            {
                frames[frame] = new PaletteRgb5[colorCount];
                for (int index = 0; index < colorCount; index++)
                {
                    ushort pointer = definition.ColorPointer(frame, index);
                    ushort bgr555 = RomDataReader.ReadWordFixedBank(
                        CartridgeImportSource.Require(bus),
                        RoomFxRomData.Banks.PaletteFx | pointer);
                    frames[frame][index] = new PaletteRgb5
                    {
                        Red = bgr555 & 31,
                        Green = bgr555 >> 5 & 31,
                        Blue = bgr555 >> 10 & 31,
                    };
                }
            }
            return frames;
        }

        PaletteRgb5[][] ExtractMaridia(MaridiaEnvironmentalPaletteOwner owner)
        {
            MaridiaEnvironmentalPaletteFxColorDefinition definition =
                MaridiaEnvironmentalPaletteFxColorDefinitions.All.Single(
                    item => item.Owner == owner);
            var frames = new PaletteRgb5[definition.FrameCount][];
            for (int frame = 0; frame < definition.FrameCount; frame++)
            {
                frames[frame] = new PaletteRgb5[definition.ColorsPerFrame];
                for (int index = 0; index < definition.ColorsPerFrame; index++)
                {
                    ushort pointer = definition.ColorPointer(frame, index);
                    ushort bgr555 = RomDataReader.ReadWordFixedBank(
                        CartridgeImportSource.Require(bus),
                        RoomFxRomData.Banks.PaletteFx | pointer);
                    frames[frame][index] = new PaletteRgb5
                    {
                        Red = bgr555 & 31,
                        Green = bgr555 >> 5 & 31,
                        Blue = bgr555 >> 10 & 31,
                    };
                }
            }
            return frames;
        }

        PaletteRgb5[][] ExtractFrames(
            int frameCount,
            int colorCount,
            Func<int, int, ushort> colorPointer)
        {
            var frames = new PaletteRgb5[frameCount][];
            for (int frame = 0; frame < frameCount; frame++)
            {
                frames[frame] = new PaletteRgb5[colorCount];
                for (int index = 0; index < colorCount; index++)
                {
                    ushort bgr555 = RomDataReader.ReadWordFixedBank(
                        CartridgeImportSource.Require(bus),
                        RoomFxRomData.Banks.PaletteFx | colorPointer(frame, index));
                    frames[frame][index] = new PaletteRgb5
                    {
                        Red = bgr555 & 31,
                        Green = bgr555 >> 5 & 31,
                        Blue = bgr555 >> 10 & 31,
                    };
                }
            }
            return frames;
        }

        PaletteRgb5[][] ExtractTorizo(TorizoBellyPaletteOwner owner)
        {
            TorizoBellyPaletteFxColorDefinition definition =
                TorizoBellyPaletteFxColorDefinitions.All.Single(
                    item => item.Owner == owner);
            return ExtractFrames(
                TorizoBellyPaletteFxColorDefinitions.FrameCount,
                TorizoBellyPaletteFxColorDefinitions.ColorsPerFrame,
                definition.ColorPointer);
        }

        PaletteRgb5[][] ExtractCrateriaLightning(CrateriaLightningPaletteOwner owner)
        {
            CrateriaLightningPaletteFxColorDefinition definition =
                CrateriaLightningPaletteFxColorDefinitions.All.Single(
                    item => item.Owner == owner);
            return ExtractFrames(
                definition.Frames.Count,
                definition.ColorsPerFrame,
                definition.ColorPointer);
        }

        PaletteRgb5[][] ExtractPlanetZebesText(PlanetZebesTextPaletteFxProgramOwner owner)
        {
            PlanetZebesTextPaletteFxColorDefinition definition =
                PlanetZebesTextPaletteFxColorDefinitions.All.Single(
                    item => item.Owner == owner);
            return ExtractFrames(
                PlanetZebesTextPaletteFxColorDefinitions.FrameCount,
                PlanetZebesTextPaletteFxColorDefinitions.ColorsPerFrame,
                definition.ColorPointer);
        }

        PaletteRgb5[][] ExtractCinematicGlow(CinematicGlowPaletteFxProgramOwner owner)
        {
            CinematicGlowPaletteFxColorDefinition definition =
                CinematicGlowPaletteFxColorDefinitions.All.Single(
                    item => item.Owner == owner);
            return ExtractFrames(
                CinematicGlowPaletteFxColorDefinitions.FrameCount,
                definition.ColorsPerFrame,
                definition.ColorPointer);
        }

        PaletteRgb5[][] ExtractZebesExplosionAmbient(
            ZebesExplosionAmbientPaletteFxProgramOwner owner)
        {
            ZebesExplosionAmbientPaletteFxColorDefinition definition =
                ZebesExplosionAmbientPaletteFxColorDefinitions.All.Single(
                    item => item.Owner == owner);
            return ExtractFrames(
                definition.FrameCount,
                definition.ColorsPerFrame,
                definition.ColorPointer);
        }

        PaletteRgb5[][] ExtractZebesExplosionLayerFade(
            ZebesExplosionLayerFadePaletteFxProgramOwner owner)
        {
            ZebesExplosionLayerFadePaletteFxColorDefinition definition =
                ZebesExplosionLayerFadePaletteFxColorDefinitions.All.Single(
                    item => item.Owner == owner);
            return ExtractFrames(
                ZebesExplosionLayerFadePaletteFxColorDefinitions.FrameCount,
                ZebesExplosionLayerFadePaletteFxColorDefinitions.ColorsPerFrame,
                definition.ColorPointer);
        }

        PaletteRgb5[][] ExtractSamusLoadingSuit(SamusLoadingSuitPaletteFxProgramOwner owner)
        {
            SamusLoadingSuitPaletteFxColorDefinition definition =
                SamusLoadingSuitPaletteFxColorDefinitions.All.Single(
                    item => item.Owner == owner);
            return ExtractFrames(
                SamusLoadingSuitPaletteFxColorDefinitions.FrameCount,
                SamusLoadingSuitPaletteFxColorDefinitions.ColorsPerFrame,
                definition.ColorPointer);
        }

        PaletteRgb5[][] ExtractTourianEscapeRedFlash(
            TourianEscapeRedFlashPaletteOwner owner)
        {
            TourianEscapeRedFlashPaletteFxColorDefinition definition =
                TourianEscapeRedFlashPaletteFxColorDefinitions.All.Single(
                    item => item.Owner == owner);
            return ExtractFrames(
                TourianEscapeRedFlashPaletteFxColorDefinitions.FrameCount,
                definition.ColorsPerFrame,
                definition.ColorPointer);
        }

        PaletteRgb5[][] ExtractOldTourianEscapeAccent(
            OldTourianEscapeAccentPaletteOwner owner)
        {
            OldTourianEscapeAccentPaletteFxColorDefinition definition =
                OldTourianEscapeAccentPaletteFxColorDefinitions.All.Single(
                    item => item.Owner == owner);
            return ExtractFrames(
                OldTourianEscapeAccentPaletteFxColorDefinitions.FrameCount,
                OldTourianEscapeAccentPaletteFxColorDefinitions.ColorsPerFrame,
                definition.ColorPointer);
        }

        PaletteRgb5[][] ExtractCrateriaEscapeLightning(
            CrateriaEscapeLightningPaletteOwner owner)
        {
            CrateriaEscapeLightningPaletteFxColorDefinition definition =
                CrateriaEscapeLightningPaletteFxColorDefinitions.All.Single(
                    item => item.Owner == owner);
            return ExtractFrames(
                CrateriaEscapeLightningPaletteFxColorDefinitions.FrameCount,
                definition.ColorsPerFrame,
                definition.ColorPointer);
        }
    }
}
