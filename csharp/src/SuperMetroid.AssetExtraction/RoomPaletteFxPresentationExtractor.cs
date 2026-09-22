using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts authored room and cinematic palette animation colors to RGB5 JSON.</summary>
internal static class RoomPaletteFxPresentationExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        using var json = new MemoryStream();
        RoomPaletteFxPresentation.Write(json, new RoomPaletteFxPresentationDocument
        {
            Version = RoomPaletteFxPresentationFormat.Version,
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
                WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.FrameCount,
                WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.ColorPointer),
            RedBrinstarBackgroundGlow = ExtractFrames(
                RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.FrameCount,
                RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.ColorPointer),
            TourianGlow = ExtractFrames(
                TourianGlowPaletteFxProgramMechanicsDefinitions.FrameCount,
                TourianGlowPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                TourianGlowPaletteFxProgramMechanicsDefinitions.ColorPointer),
            BrinstarBlueSpores = ExtractFrames(
                BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.FrameCount,
                BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.All[0].ColorPointer),
            BombTorizoBelly = ExtractTorizo(TorizoBellyPaletteOwner.BombTorizo),
            GoldenTorizoBelly = ExtractTorizo(TorizoBellyPaletteOwner.GoldenTorizo),
            TourianStatueGrey = ExtractFrames(
                TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FrameCount,
                TourianStatueGreyPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                TourianStatueGreyPaletteFxProgramMechanicsDefinitions.ColorPointer),
            CrateriaSurfaceLightning = ExtractCrateriaLightning(
                CrateriaLightningPaletteOwner.SurfaceLightning),
            CrateriaUnusedDarkLightning = ExtractCrateriaLightning(
                CrateriaLightningPaletteOwner.UnusedDarkLightning),
            CeresGunshipEngineLights = ExtractFrames(
                CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                    .GunshipEngineFrameCount,
                CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                    .GunshipEngineColorsPerFrame,
                CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                    .GunshipEngineColorPointer),
            CeresNavigationLights = ExtractFrames(
                CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                    .NavigationLightsFrameCount,
                CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                    .NavigationLightsColorsPerFrame,
                CeresCinematicLightPaletteFxProgramMechanicsDefinitions
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
                ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.FrameCount,
                ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.ColorPointer),
            UnusedCinematicFade = ExtractFrames(
                UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.FrameCount,
                UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.ColorPointer),
            TitleLogoFade = ExtractFrames(
                TitleLogoFadePaletteFxProgramMechanicsDefinitions.FrameCount,
                TitleLogoFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                TitleLogoFadePaletteFxProgramMechanicsDefinitions.ColorPointer),
            NintendoSharedFade = ExtractFrames(
                NintendoLogoFadePaletteFxProgramMechanicsDefinitions.FrameCount,
                NintendoLogoFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                NintendoLogoFadePaletteFxProgramMechanicsDefinitions.ColorPointer),
            ZebesExplosionForeground = ExtractFrames(
                ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.FrameCount,
                ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.ColorPointer),
            ZebesExplosionFinale = ExtractFrames(
                ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.FrameCount,
                ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.ColorPointer),
            ZebesExplosionWhiteout = ExtractFrames(
                ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.FrameCount,
                ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.ColorPointer),
            ZebesExplosionAfterglow = ExtractZebesExplosionAmbient(
                ZebesExplosionAmbientPaletteFxProgramOwner.PlanetAfterglow),
            ZebesExplosionLava = ExtractZebesExplosionAmbient(
                ZebesExplosionAmbientPaletteFxProgramOwner.Lava),
            ZebesExplosionCrust = ExtractZebesExplosionLayerFade(
                ZebesExplosionLayerFadePaletteFxProgramOwner.Crust),
            ZebesExplosionGreyClouds = ExtractZebesExplosionLayerFade(
                ZebesExplosionLayerFadePaletteFxProgramOwner.GreyClouds),
            ZebesExplosionGunship = ExtractFrames(
                ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.FrameCount,
                ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorPointer),
            SamusLoadingPowerSuit = ExtractSamusLoadingSuit(
                SamusLoadingSuitPaletteFxProgramOwner.PowerSuit),
            SamusLoadingVariaSuit = ExtractSamusLoadingSuit(
                SamusLoadingSuitPaletteFxProgramOwner.VariaSuit),
            SamusLoadingGravitySuit = ExtractSamusLoadingSuit(
                SamusLoadingSuitPaletteFxProgramOwner.GravitySuit),
            PostCreditsIconGlare = ExtractFrames(
                PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.FrameCount,
                PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.ColorPointer),
            TourianEscapeShutter = ExtractTourianEscapeRedFlash(
                TourianEscapeRedFlashPaletteOwner.Shutter),
            TourianEscapeBackground = ExtractTourianEscapeRedFlash(
                TourianEscapeRedFlashPaletteOwner.Background),
            TourianEscapeSharedRedFlash = ExtractFrames(
                TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount,
                TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.ColorPointer),
            OldTourianEscapeRedFlash = ExtractFrames(
                OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount,
                OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorPointer),
            OldTourianEscapeOrangeRailings = ExtractOldTourianEscapeAccent(
                OldTourianEscapeAccentPaletteOwner.OrangeRailings),
            OldTourianEscapeYellowPanels = ExtractOldTourianEscapeAccent(
                OldTourianEscapeAccentPaletteOwner.YellowPanels),
            UpperCrateriaEscapeRedFlash = ExtractFrames(
                UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount,
                UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorPointer),
            CrateriaEscapeYellowLightning = ExtractCrateriaEscapeLightning(
                CrateriaEscapeLightningPaletteOwner.YellowLightning),
            CrateriaEscapeCreBlockPixel = ExtractCrateriaEscapeLightning(
                CrateriaEscapeLightningPaletteOwner.CreBlockPixel),
        });
        return json.ToArray();

        PaletteRgb5[][] ExtractNorfair(NorfairEnvironmentalPaletteOwner owner)
        {
            NorfairEnvironmentalPaletteFxProgramDefinition definition =
                NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.All.Single(
                    item => item.Owner == owner);
            int frameCount = NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.FrameCount;
            int colorCount = NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.ColorsPerFrame;
            var frames = new PaletteRgb5[frameCount][];
            for (int frame = 0; frame < frameCount; frame++)
            {
                frames[frame] = new PaletteRgb5[colorCount];
                for (int index = 0; index < colorCount; index++)
                {
                    ushort pointer = definition.ColorPointer(frame, index);
                    ushort bgr555 = RomDataReader.ReadWordFixedBank(
                        bus,
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
            MaridiaEnvironmentalPaletteFxProgramDefinition definition =
                MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.All.Single(
                    item => item.Owner == owner);
            var frames = new PaletteRgb5[definition.FrameCount][];
            for (int frame = 0; frame < definition.FrameCount; frame++)
            {
                frames[frame] = new PaletteRgb5[definition.ColorsPerFrame];
                for (int index = 0; index < definition.ColorsPerFrame; index++)
                {
                    ushort pointer = definition.ColorPointer(frame, index);
                    ushort bgr555 = RomDataReader.ReadWordFixedBank(
                        bus,
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
                        bus,
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
            TorizoBellyPaletteFxProgramDefinition definition =
                TorizoBellyPaletteFxProgramMechanicsDefinitions.All.Single(
                    item => item.Owner == owner);
            return ExtractFrames(
                TorizoBellyPaletteFxProgramMechanicsDefinitions.FrameCount,
                TorizoBellyPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer);
        }

        PaletteRgb5[][] ExtractCrateriaLightning(CrateriaLightningPaletteOwner owner)
        {
            CrateriaLightningPaletteFxProgramDefinition definition =
                CrateriaLightningPaletteFxProgramMechanicsDefinitions.All.Single(
                    item => item.Owner == owner);
            return ExtractFrames(
                definition.Frames.Count,
                definition.ColorsPerFrame,
                definition.ColorPointer);
        }

        PaletteRgb5[][] ExtractPlanetZebesText(PlanetZebesTextPaletteFxProgramOwner owner)
        {
            PlanetZebesTextPaletteFxProgramDefinition definition =
                PlanetZebesTextPaletteFxProgramMechanicsDefinitions.All.Single(
                    item => item.Owner == owner);
            return ExtractFrames(
                PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount,
                PlanetZebesTextPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer);
        }

        PaletteRgb5[][] ExtractCinematicGlow(CinematicGlowPaletteFxProgramOwner owner)
        {
            CinematicGlowPaletteFxProgramDefinition definition =
                CinematicGlowPaletteFxProgramMechanicsDefinitions.All.Single(
                    item => item.Owner == owner);
            return ExtractFrames(
                CinematicGlowPaletteFxProgramMechanicsDefinitions.FrameCount,
                definition.ColorsPerFrame,
                definition.ColorPointer);
        }

        PaletteRgb5[][] ExtractZebesExplosionAmbient(
            ZebesExplosionAmbientPaletteFxProgramOwner owner)
        {
            ZebesExplosionAmbientPaletteFxProgramDefinition definition =
                ZebesExplosionAmbientPaletteFxProgramMechanicsDefinitions.All.Single(
                    item => item.Owner == owner);
            return ExtractFrames(
                definition.FrameCount,
                definition.ColorsPerFrame,
                definition.ColorPointer);
        }

        PaletteRgb5[][] ExtractZebesExplosionLayerFade(
            ZebesExplosionLayerFadePaletteFxProgramOwner owner)
        {
            ZebesExplosionLayerFadePaletteFxProgramDefinition definition =
                ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.All.Single(
                    item => item.Owner == owner);
            return ExtractFrames(
                ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.FrameCount,
                ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer);
        }

        PaletteRgb5[][] ExtractSamusLoadingSuit(SamusLoadingSuitPaletteFxProgramOwner owner)
        {
            SamusLoadingSuitPaletteFxProgramDefinition definition =
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.All.Single(
                    item => item.Owner == owner);
            return ExtractFrames(
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameCount,
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer);
        }

        PaletteRgb5[][] ExtractTourianEscapeRedFlash(
            TourianEscapeRedFlashPaletteOwner owner)
        {
            TourianEscapeRedFlashPaletteFxProgramDefinition definition =
                TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.All.Single(
                    item => item.Owner == owner);
            return ExtractFrames(
                TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount,
                definition.ColorsPerFrame,
                definition.ColorPointer);
        }

        PaletteRgb5[][] ExtractOldTourianEscapeAccent(
            OldTourianEscapeAccentPaletteOwner owner)
        {
            OldTourianEscapeAccentPaletteFxProgramDefinition definition =
                OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.All.Single(
                    item => item.Owner == owner);
            return ExtractFrames(
                OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.FrameCount,
                OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer);
        }

        PaletteRgb5[][] ExtractCrateriaEscapeLightning(
            CrateriaEscapeLightningPaletteOwner owner)
        {
            CrateriaEscapeLightningPaletteFxProgramDefinition definition =
                CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.All.Single(
                    item => item.Owner == owner);
            return ExtractFrames(
                CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.FrameCount,
                definition.ColorsPerFrame,
                definition.ColorPointer);
        }
    }
}
