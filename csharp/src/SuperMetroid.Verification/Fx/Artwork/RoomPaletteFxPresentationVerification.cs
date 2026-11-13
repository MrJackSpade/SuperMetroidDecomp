using SuperMetroid.AssetExtraction.Fx.Artwork;
using System.Text;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Frontend.Title.TitleGradientVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Fx.Artwork;
using SuperMetroid.Verification.Fx.PaletteFx;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Compares extracted room palette-FX colors and installed playback against cartridge output.</summary>
internal static class RoomPaletteFxPresentationVerification
{
    /// <summary>Audits extracted room palette colors and all installed program families against retail color words.</summary>
    internal static void VerifyExtractedRoomPaletteFxPresentation(
        CartridgeImportAddressSpace bus)
    {
        byte[] extracted =
            SuperMetroid.AssetExtraction.Fx.RoomPaletteFxPresentationExtractor.Extract(bus);
        RoomPaletteFxPresentation presentation = RoomPaletteFxPresentationFile.Read(
            new MemoryStream(extracted, writable: false));
        var colorAddresses = new HashSet<int>();
        foreach (NorfairEnvironmentalPaletteFxColorDefinition definition in
                 NorfairEnvironmentalPaletteFxColorDefinitions.All)
        {
            for (int frame = 0;
                 frame < NorfairEnvironmentalPaletteFxColorDefinitionsConstants.FrameCount;
                 frame++)
            {
                for (int color = 0;
                     color < NorfairEnvironmentalPaletteFxColorDefinitions.ColorsPerFrame;
                     color++)
                {
                    ushort pointer = definition.ColorPointer(frame, color);
                    ushort expected = RomDataReader.ReadWordFixedBank(
                        CartridgeImportSource.Require(bus),
                        RoomFxBankSourceDefinitions.PaletteFx | pointer);
                    AssertTrue(presentation.TryReadColor(pointer, out RgbColor actual),
                        $"installed Norfair color resolves $8D:{pointer:X4}");
                    AssertEqual(expected, actual,
                        $"installed Norfair color matches cartridge $8D:{pointer:X4}");
                    colorAddresses.Add(RoomFxBankSourceDefinitions.PaletteFx | pointer);
                    colorAddresses.Add(RoomFxBankSourceDefinitions.PaletteFx |
                        unchecked((ushort)(pointer + 1)));
                }
            }
        }
        AssertEqual(640, colorAddresses.Count,
            "Norfair presentation owns all 320 BGR555 source words");

        var guarded = NewTitlePresentationReadBus(bus, colorAddresses, forbidReads: true);
        var nativeSession = new PaletteFxSessionState();
        var native = new RoomPaletteFxSystem(nativeSession);
        var installedSession = new PaletteFxSessionState();
        var installed = new RoomPaletteFxSystem(installedSession);
        foreach (NorfairEnvironmentalPaletteFxColorDefinition definition in
                 NorfairEnvironmentalPaletteFxColorDefinitions.All)
        {
            native.Start(definition.DefinitionPointer, equippedItems: 0);
            installed.Start(definition.DefinitionPointer, equippedItems: 0);
        }

        var nativeCgram = new SnesCgram();
        var installedCgram = new SnesCgram();
        int frames = NorfairEnvironmentalPaletteFxColorDefinitions.CycleFrames * 2;
        for (int frame = 0; frame < frames; frame++)
        {
            native.Step(new PaletteFxConditions { SamusY = 0, EquippedItems = 0, AreaMiniBossDefeated = false });
            PaletteFxRoomLook.LayShownShades(native, nativeCgram, new ReferencePaletteFxColorSource(bus));
            installed.Step(new PaletteFxConditions { SamusY = 0, EquippedItems = 0, AreaMiniBossDefeated = false });
            PaletteFxRoomLook.LayShownShades(installed, installedCgram, presentation);
            AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(installedCgram)),
                $"installed Norfair palette equals native output on frame {frame}");
            AssertEqual(nativeSession.PublishedHeatPhase, installedSession.PublishedHeatPhase,
                $"installed Norfair heat phase equals native output on frame {frame}");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed Norfair palette loops avoid cartridge color reads");

        Suite(nameof(VerifyExtractedSamusHeatPaletteFxPresentation), () => VerifyExtractedSamusHeatPaletteFxPresentation(bus, presentation));
        Suite(nameof(PaletteFxSpriteShadesVerification.VerifyPaletteFxSpriteShades), () => PaletteFxSpriteShadesVerification.VerifyPaletteFxSpriteShades(bus, presentation));

        Suite(nameof(VerifyExtractedMaridiaPaletteFxPresentation), () => VerifyExtractedMaridiaPaletteFxPresentation(bus, presentation));
        Suite(nameof(VerifyExtractedWreckedShipPaletteFxPresentation), () => VerifyExtractedWreckedShipPaletteFxPresentation(bus, presentation));
        Suite(nameof(VerifyInstalledPaletteFxFamily), () => VerifyInstalledPaletteFxFamily(bus, presentation, "Red Brinstar glow", new PaletteFxFamilyShape { FrameCount = RedBrinstarGlowPaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = RedBrinstarGlowPaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = RedBrinstarGlowPaletteFxColorDefinitions.ColorPointer, Definitions = [RedBrinstarGlowPaletteFxColorDefinitions.DefinitionPointer] }, RedBrinstarGlowPaletteFxColorDefinitions.CycleFrames * 2));
        Suite(nameof(VerifyInstalledPaletteFxFamily), () => VerifyInstalledPaletteFxFamily(bus, presentation, "Tourian glow", new PaletteFxFamilyShape { FrameCount = TourianGlowPaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = TourianGlowPaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = TourianGlowPaletteFxColorDefinitions.ColorPointer, Definitions = [
                TourianGlowPaletteFxColorDefinitions.LiveDefinitionPointer,
            ] }, TourianGlowPaletteFxColorDefinitions.CycleFrames * 2));
        foreach (BrinstarBlueSporePaletteFxColorDefinition definition in
                 BrinstarBlueSporePaletteFxColorDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(bus, presentation, $"Brinstar blue spores ({definition.Owner})", new PaletteFxFamilyShape { FrameCount = BrinstarBlueSporePaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = BrinstarBlueSporePaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = definition.ColorPointer, Definitions = [definition.DefinitionPointer] }, BrinstarBlueSporePaletteFxColorDefinitions.CycleFrames * 2);
        }
        foreach (TorizoBellyPaletteFxColorDefinition definition in
                 TorizoBellyPaletteFxColorDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(bus, presentation, $"{definition.Owner} belly", new PaletteFxFamilyShape { FrameCount = TorizoBellyPaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = TorizoBellyPaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = definition.ColorPointer, Definitions = [definition.DefinitionPointer], LitTorizo = LivingTorizo.Instance }, TorizoBellyPaletteFxColorDefinitions.CycleFrames * 2);
        }
        Suite(nameof(VerifyInstalledPaletteFxFamily), () => VerifyInstalledPaletteFxFamily(bus, presentation, "Tourian statue grey-out", new PaletteFxFamilyShape { FrameCount = TourianStatueGreyPaletteFxColorDefinitions.ColorFrames.FrameCount, ColorsPerFrame = TourianStatueGreyPaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = TourianStatueGreyPaletteFxColorDefinitions.ColorPointer, Definitions = TourianStatueGreyPaletteFxColorToolingDefinitions.All
                .Select(definition => definition.DefinitionPointer)
                .ToArray() }, TourianStatueGreyPaletteFxColorDefinitions.FramesThroughDeletion));
        {
            CrateriaLightningPaletteFxColorDefinition definition = CrateriaLightningPaletteFxColorDefinitions.Surface;
            VerifyInstalledPaletteFxFamily(bus, presentation, "Crateria surface lightning", new PaletteFxFamilyShape { FrameCount = definition.Frames.Count, ColorsPerFrame = definition.ColorsPerFrame, ColorPointer = definition.ColorPointer, Definitions = [CrateriaLightningPaletteFxColorDefinition.DefinitionPointer] }, definition.CycleFrames * 2, CrateriaLightningDefinitions.SamusHighOnSurfaceHeight);
        }
        Suite(nameof(VerifyInstalledPaletteFxFamily), () => VerifyInstalledPaletteFxFamily(bus, presentation, "Ceres gunship-engine lights", new PaletteFxFamilyShape { FrameCount = CeresCinematicLightPaletteFxColorDefinitionsConstants.GunshipEngineFrameCount, ColorsPerFrame = CeresCinematicLightPaletteFxColorDefinitions
                .GunshipEngineColorsPerFrame, ColorPointer = CeresCinematicLightPaletteFxColorDefinitions
                .GunshipEngineColorPointer, Definitions = [CeresCinematicLightPaletteFxColorDefinitions
                .GunshipEngineDefinitionPointer], OnScreen = true }, CeresCinematicLightPaletteFxColorDefinitions
                .GunshipEngineCycleFrames * 2));
        Suite(nameof(VerifyInstalledPaletteFxFamily), () => VerifyInstalledPaletteFxFamily(bus, presentation, "Ceres navigation lights", new PaletteFxFamilyShape { FrameCount = CeresCinematicLightPaletteFxColorDefinitionsConstants.NavigationLightsFrameCount, ColorsPerFrame = CeresCinematicLightPaletteFxColorDefinitions
                .NavigationLightsColorsPerFrame, ColorPointer = CeresCinematicLightPaletteFxColorDefinitions
                .NavigationLightsColorPointer, Definitions = [
                CeresCinematicLightPaletteFxColorDefinitions
                    .SpriteNavigationLightsDefinitionPointer,
                CeresCinematicLightPaletteFxColorDefinitions
                    .BackgroundNavigationLightsDefinitionPointer,
            ], OnScreen = true }, CeresCinematicLightPaletteFxColorDefinitions
                .NavigationLightsCycleFrames * 2));
        foreach (PlanetZebesTextPaletteFxColorDefinition definition in
                 PlanetZebesTextPaletteFxColorDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(bus, presentation, $"PLANET ZEBES {definition.Owner}", new PaletteFxFamilyShape { FrameCount = PlanetZebesTextPaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = PlanetZebesTextPaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = definition.ColorPointer, Definitions = [definition.DefinitionPointer], OnScreen = true }, PlanetZebesTextPaletteFxColorDefinitions.CycleFrames + 1);
        }
        foreach (CinematicGlowPaletteFxColorDefinition definition in
                 CinematicGlowPaletteFxColorDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(bus, presentation, $"Cinematic glow {definition.Owner}", new PaletteFxFamilyShape { FrameCount = CinematicGlowPaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = definition.ColorsPerFrame, ColorPointer = definition.ColorPointer, Definitions = [definition.DefinitionPointer], OnScreen = true }, definition.CycleFrames * 2);
        }
        Suite(nameof(VerifyInstalledPaletteFxFamily), () => VerifyInstalledPaletteFxFamily(bus, presentation, "exploding Zebes fade", new PaletteFxFamilyShape { FrameCount = ExplodingZebesFadePaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = ExplodingZebesFadePaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = ExplodingZebesFadePaletteFxColorDefinitions.ColorPointer, Definitions = [ExplodingZebesFadePaletteFxColorDefinitions.DefinitionPointer], OnScreen = true }, ExplodingZebesFadePaletteFxColorDefinitions.CycleFrames + 1));
        Suite(nameof(VerifyInstalledPaletteFxFamily), () => VerifyInstalledPaletteFxFamily(bus, presentation, "title-logo fade", new PaletteFxFamilyShape { FrameCount = TitleLogoFadePaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = TitleLogoFadePaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = TitleLogoFadePaletteFxColorDefinitions.ColorPointer, Definitions = [TitleLogoFadePaletteFxColorDefinitions.DefinitionPointer], OnScreen = true }, TitleLogoFadePaletteFxColorDefinitions.CycleFrames + 1));
        Suite(nameof(VerifyInstalledPaletteFxFamily), () => VerifyInstalledPaletteFxFamily(bus, presentation, "Nintendo shared fade", new PaletteFxFamilyShape { FrameCount = NintendoLogoFadePaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = NintendoLogoFadePaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = NintendoLogoFadePaletteFxColorDefinitions.ColorPointer, Definitions = [NintendoLogoFadePaletteFxColorDefinitions.BootLogoDefinitionPointer,
                NintendoLogoFadePaletteFxColorDefinitions.CopyrightDefinitionPointer], OnScreen = true }, NintendoLogoFadePaletteFxColorDefinitions.CycleFrames + 1));
        Suite(nameof(VerifyInstalledPaletteFxFamily), () => VerifyInstalledPaletteFxFamily(bus, presentation, "Zebes explosion foreground", new PaletteFxFamilyShape { FrameCount = ZebesExplosionForegroundPaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = ZebesExplosionForegroundPaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = ZebesExplosionForegroundPaletteFxColorDefinitions.ColorPointer, Definitions = [ZebesExplosionForegroundPaletteFxColorDefinitions.DefinitionPointer], OnScreen = true }, ZebesExplosionForegroundPaletteFxColorDefinitionsConstants.CycleFrames + 1));
        Suite(nameof(VerifyInstalledPaletteFxFamily), () => VerifyInstalledPaletteFxFamily(bus, presentation, "Zebes explosion finale", new PaletteFxFamilyShape { FrameCount = ZebesExplosionFinalePaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = ZebesExplosionFinalePaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = ZebesExplosionFinalePaletteFxColorDefinitions.ColorPointer, Definitions = [ZebesExplosionFinalePaletteFxColorDefinitions.DefinitionPointer], OnScreen = true }, ZebesExplosionFinalePaletteFxColorDefinitions.CycleFrames + 1));
        Suite(nameof(VerifyInstalledPaletteFxFamily), () => VerifyInstalledPaletteFxFamily(bus, presentation, "Zebes explosion shared whiteout", new PaletteFxFamilyShape { FrameCount = ZebesExplosionWhiteoutPaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = ZebesExplosionWhiteoutPaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = ZebesExplosionWhiteoutPaletteFxColorDefinitions.ColorPointer, Definitions = [ZebesExplosionWhiteoutPaletteFxColorDefinitions.WideExplosionBackgroundDefinitionPointer,
                ZebesExplosionWhiteoutPaletteFxColorDefinitions.SpaceWhiteoutDefinitionPointer], OnScreen = true }, ZebesExplosionWhiteoutPaletteFxColorDefinitions.CycleFrames + 1));
        foreach (ZebesExplosionAmbientPaletteFxColorDefinition definition in
                 ZebesExplosionAmbientPaletteFxColorDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(bus, presentation, $"Zebes explosion {definition.Owner}", new PaletteFxFamilyShape { FrameCount = definition.FrameCount, ColorsPerFrame = definition.ColorsPerFrame, ColorPointer = definition.ColorPointer, Definitions = [definition.DefinitionPointer], OnScreen = true }, definition.CycleFrames * 2);
        }
        foreach (ZebesExplosionLayerFadePaletteFxColorDefinition definition in
                 ZebesExplosionLayerFadePaletteFxColorDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(bus, presentation, $"Zebes explosion {definition.Owner}", new PaletteFxFamilyShape { FrameCount = ZebesExplosionLayerFadePaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = ZebesExplosionLayerFadePaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = definition.ColorPointer, Definitions = [definition.DefinitionPointer], OnScreen = true }, definition.CycleFrames + 1);
        }
        Suite(nameof(VerifyExtractedEndingGunshipPaletteFxPresentation), () => VerifyExtractedEndingGunshipPaletteFxPresentation(bus, presentation));
        Suite(nameof(VerifyExtractedSamusLoadingPaletteFxPresentation), () => VerifyExtractedSamusLoadingPaletteFxPresentation(bus, presentation));
        Suite(nameof(VerifyExtractedLogoGlarePaletteFxPresentation), () => VerifyExtractedLogoGlarePaletteFxPresentation(bus, presentation));
        foreach (TourianEscapeRedFlashPaletteFxColorDefinition definition in
                 TourianEscapeRedFlashPaletteFxColorDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(bus, presentation, $"Tourian escape {definition.Owner}", new PaletteFxFamilyShape { FrameCount = TourianEscapeRedFlashPaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = definition.ColorsPerFrame, ColorPointer = definition.ColorPointer, Definitions = [definition.DefinitionPointer] }, definition.CycleFrames * 2);
        }
        Suite(nameof(VerifyInstalledPaletteFxFamily), () => VerifyInstalledPaletteFxFamily(bus, presentation, "Tourian escape shared red flash", new PaletteFxFamilyShape { FrameCount = TourianEscapeSharedRedFlashPaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = TourianEscapeSharedRedFlashPaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = TourianEscapeSharedRedFlashPaletteFxColorDefinitions.ColorPointer, Definitions = TourianEscapeSharedRedFlashPaletteFxColorToolingDefinitions.All
                .Select(definition => definition.DefinitionPointer).ToArray() }, TourianEscapeSharedRedFlashPaletteFxColorDefinitions.CycleFrames * 2));
        Suite(nameof(VerifyInstalledPaletteFxFamily), () => VerifyInstalledPaletteFxFamily(bus, presentation, "old Tourian escape red flash", new PaletteFxFamilyShape { FrameCount = OldTourianEscapeRedFlashPaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = OldTourianEscapeRedFlashPaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = OldTourianEscapeRedFlashPaletteFxColorDefinitions.ColorPointer, Definitions = [OldTourianEscapeRedFlashPaletteFxColorDefinitions.DefinitionPointer] }, OldTourianEscapeRedFlashPaletteFxColorDefinitions.CycleFrames * 2));
        foreach (OldTourianEscapeAccentPaletteFxColorDefinition definition in
                 OldTourianEscapeAccentPaletteFxColorDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(bus, presentation, $"old Tourian escape {definition.Owner}", new PaletteFxFamilyShape { FrameCount = OldTourianEscapeAccentPaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = OldTourianEscapeAccentPaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = definition.ColorPointer, Definitions = [definition.DefinitionPointer] }, OldTourianEscapeAccentPaletteFxColorDefinitions.CycleFrames * 2);
        }
        Suite(nameof(VerifyInstalledPaletteFxFamily), () => VerifyInstalledPaletteFxFamily(bus, presentation, "upper Crateria escape red flash", new PaletteFxFamilyShape { FrameCount = UpperCrateriaEscapeRedFlashPaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.ColorPointer, Definitions = [UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.DefinitionPointer] }, UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.CycleFrames * 2));
        foreach (CrateriaEscapeLightningPaletteFxColorDefinition definition in
                 CrateriaEscapeLightningPaletteFxColorDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(bus, presentation, $"Crateria escape {definition.Owner}", new PaletteFxFamilyShape { FrameCount = CrateriaEscapeLightningPaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = definition.ColorsPerFrame, ColorPointer = definition.ColorPointer, Definitions = [definition.DefinitionPointer] }, CrateriaEscapeLightningPaletteFxColorDefinitions.CycleFrames * 2);
        }
        Suite(nameof(VerifyInstalledPaletteFxFamily), () => VerifyInstalledPaletteFxFamily(bus, presentation, "Crateria and Brinstar beacon flash", new PaletteFxFamilyShape { FrameCount = BeaconPaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = BeaconPaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = BeaconPaletteFxColorDefinitions.ColorPointer, Definitions = [BeaconPaletteFxColorDefinitions.DefinitionPointer] }, BeaconPaletteFxColorDefinitions.CycleFrames * 2));
        Suite(nameof(VerifyRoomPaletteFxPresentationValidation), () => VerifyRoomPaletteFxPresentationValidation(extracted));
        Console.WriteLine(
            "  Room palette presentation: 5108 installed palette colors match ROM; " +
            "fifty-five installed programs match native execution without color-source reads.");
    }

    /// <summary>Checks the ending gunship's installed palette colours and playback against its native cycle.</summary>
    internal static void VerifyExtractedEndingGunshipPaletteFxPresentation(
        ISnesAddressSpace bus, RoomPaletteFxPresentation presentation) =>
        VerifyInstalledPaletteFxFamily(bus, presentation, "Zebes explosion gunship", new PaletteFxFamilyShape { FrameCount = 16, ColorsPerFrame = 16, ColorPointer = (frame, color) => (ushort)(0xd6c0 + frame * 36 + color * 2), Definitions = [0xe1e4], OnScreen = true }, 385);

    /// <summary>Checks the post-credits logo glare's installed colours and playback against its native cycle.</summary>
    internal static void VerifyExtractedLogoGlarePaletteFxPresentation(
        ISnesAddressSpace bus, RoomPaletteFxPresentation presentation) =>
        VerifyInstalledPaletteFxFamily(bus, presentation, "post-credits icon glare", new PaletteFxFamilyShape { FrameCount = 14, ColorsPerFrame = 16, ColorPointer = (frame, color) => (ushort)(0xdf9a + frame * 36 + color * 2), Definitions = [0xe200], OnScreen = true }, 15);

    /// <summary>Checks each Samus loading program's installed colours and playback against its native cycle.</summary>
    internal static void VerifyExtractedSamusLoadingPaletteFxPresentation(
        ISnesAddressSpace bus, RoomPaletteFxPresentation presentation)
    {
        foreach (SamusLoadingSuitPaletteFxColorDefinition definition in
                 SamusLoadingSuitPaletteFxColorDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(bus, presentation, $"Samus loading {definition.Owner}", new PaletteFxFamilyShape { FrameCount = SamusLoadingSuitPaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = SamusLoadingSuitPaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = definition.ColorPointer, Definitions = [definition.DefinitionPointer] }, SamusLoadingSuitPaletteFxColorDefinitions.CycleFrames + 1);
        }
    }

    /// <summary>Checks each Samus heat program's installed colours and suit cycle without runtime color-ROM reads.</summary>
    internal static void VerifyExtractedSamusHeatPaletteFxPresentation(
        ISnesAddressSpace bus, RoomPaletteFxPresentation presentation)
    {
        foreach (PaletteFxHeatProgramDefinition definition in
                 PaletteFxHeatProgramMechanicsDefinitions.All)
        {
            var forbidden = new HashSet<int>();
            foreach (var frame in definition.Frames)
            for (int index = 0; index < PaletteFxHeatProgramDefinition.ColorsPerFrame; index++)
            {
                ushort pointer = unchecked((ushort)(frame.FirstColorPointer +
                    index * sizeof(ushort)));
                ushort nativeColor = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                    RoomFxBankSourceDefinitions.PaletteFx | pointer);
                AssertTrue(presentation.TryReadColor(pointer, out RgbColor installedColor),
                    $"installed {definition.Suit} heat color resolves $8D:{pointer:X4}");
                AssertEqual(nativeColor, installedColor,
                    $"installed {definition.Suit} heat color matches cartridge $8D:{pointer:X4}");
                forbidden.Add(RoomFxBankSourceDefinitions.PaletteFx | pointer);
                forbidden.Add(RoomFxBankSourceDefinitions.PaletteFx | unchecked((ushort)(pointer + 1)));
            }
            AssertEqual(definition.Frames.Count *
                PaletteFxHeatProgramDefinition.ColorsPerFrame * sizeof(ushort),
                forbidden.Count, $"{definition.Suit} heat colors have distinct source bytes");

            var guard = NewTitlePresentationReadBus(bus, forbidden, forbidReads: true);
            var nativeSession = new PaletteFxSessionState();
            var native = new RoomPaletteFxSystem(nativeSession);
            var installedSession = new PaletteFxSessionState();
            var installed = new RoomPaletteFxSystem(installedSession);
            ushort equipment = definition.Suit switch
            {
                PaletteFxHeatSuit.Power => 0,
                PaletteFxHeatSuit.Varia => SamusEquipmentFlags.VariaSuit.ToEquipmentWord(),
                PaletteFxHeatSuit.Gravity => SamusEquipmentFlags.GravitySuit.ToEquipmentWord(),
                _ => throw new InvalidOperationException(),
            };
            native.Start(RoomPaletteFxDefinitions.SamusInHeat, SamusEquipmentWordCodec.FromEquipmentWord(equipment));
            installed.Start(RoomPaletteFxDefinitions.SamusInHeat, SamusEquipmentWordCodec.FromEquipmentWord(equipment));
            var nativeCgram = new SnesCgram();
            var installedCgram = new SnesCgram();
            int steps = 1 + Enumerable.Range(0, definition.Frames.Count)
                .Sum(phase => PaletteFxHeatProgramDefinitionConstants.Duration(definition.Suit, phase));
            for (int step = 0; step <= steps; step++)
            {
                native.Step(new PaletteFxConditions { SamusY = 0, EquippedItems = SamusEquipmentWordCodec.FromEquipmentWord(equipment), AreaMiniBossDefeated = false });
                PaletteFxRoomLook.LayShownShades(native, nativeCgram, new ReferencePaletteFxColorSource(bus));
                installed.Step(new PaletteFxConditions { SamusY = 0, EquippedItems = SamusEquipmentWordCodec.FromEquipmentWord(equipment), AreaMiniBossDefeated = false });
                PaletteFxRoomLook.LayShownShades(installed, installedCgram, presentation);
                AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(installedCgram)),
                    $"installed {definition.Suit} heat CGRAM matches native frame {step}");
            }
            AssertEqual(0, guard.ForbiddenReadAttempts,
                $"installed {definition.Suit} heat cycle never rereads color ROM");
        }
    }

    /// <summary>Compares one installed palette family to retail source words and frame-by-frame CGRAM output.</summary>
    private static void VerifyInstalledPaletteFxFamily(ISnesAddressSpace bus, RoomPaletteFxPresentation presentation, string description, PaletteFxFamilyShape shape, int framesToRun, ushort samusY = 0)
    {
        var colorAddresses = new HashSet<int>();
        for (int frame = 0; frame < shape.FrameCount; frame++)
        for (int color = 0; color < shape.ColorsPerFrame; color++)
        {
            ushort pointer = shape.ColorPointer(frame, color);
            ushort expected = RomDataReader.ReadWordFixedBank(
                CartridgeImportSource.Require(bus),
                RoomFxBankSourceDefinitions.PaletteFx | pointer);
            AssertTrue(presentation.TryReadColor(pointer, out RgbColor actual),
                $"installed {description} color resolves $8D:{pointer:X4}");
            AssertEqual(expected, actual,
                $"installed {description} color matches cartridge $8D:{pointer:X4}");
            colorAddresses.Add(RoomFxBankSourceDefinitions.PaletteFx | pointer);
            colorAddresses.Add(RoomFxBankSourceDefinitions.PaletteFx |
                unchecked((ushort)(pointer + 1)));
        }
        AssertEqual(shape.FrameCount * shape.ColorsPerFrame * sizeof(ushort), colorAddresses.Count,
            $"{description} presentation owns every BGR555 source byte");

        foreach (ushort definition in shape.Definitions)
        {
            var guarded = NewTitlePresentationReadBus(bus, colorAddresses, forbidReads: true);
            var nativeSession = new PaletteFxSessionState();
            var native = new RoomPaletteFxSystem(nativeSession);
            var installedSession = new PaletteFxSessionState();
            var installed = new RoomPaletteFxSystem(installedSession);
            StartFamily(native, shape, definition);
            StartFamily(installed, shape, definition);
            var nativeCgram = new SnesCgram();
            var installedCgram = new SnesCgram();
            for (int frame = 0; frame < framesToRun; frame++)
            {
                var conditions = new PaletteFxConditions { SamusY = samusY, EquippedItems = 0, AreaMiniBossDefeated = false };
                StepFamily(native, shape, conditions);
                PaletteFxRoomLook.LayShownShades(native, nativeCgram, new ReferencePaletteFxColorSource(bus));
                StepFamily(installed, shape, conditions);
                PaletteFxRoomLook.LayShownShades(installed, installedCgram, presentation);
                AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(installedCgram)),
                    $"installed {description} definition ${definition:X4} equals native " +
                    $"output on frame {frame}");
                AssertTrue(native.SoundRequests.SequenceEqual(installed.SoundRequests),
                    $"installed {description} definition ${definition:X4} preserves native " +
                    $"audio requests on frame {frame}");
            }
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"installed {description} definition ${definition:X4} avoids color reads");
        }
    }

    /// <summary>Checks Wrecked Ship light colors and installed green-light playback against retail output.</summary>
    private static void VerifyExtractedWreckedShipPaletteFxPresentation(
        ISnesAddressSpace bus,
        RoomPaletteFxPresentation presentation)
    {
        var colorAddresses = new HashSet<int>();
        for (int frame = 0;
             frame < WreckedShipGreenLightPaletteFxColorDefinitionsConstants.FrameCount;
             frame++)
        for (int color = 0;
             color < WreckedShipGreenLightPaletteFxColorDefinitions.ColorsPerFrame;
             color++)
        {
            ushort pointer =
                WreckedShipGreenLightPaletteFxColorDefinitions.ColorPointer(
                    frame, color);
            ushort expected = RomDataReader.ReadWordFixedBank(
                CartridgeImportSource.Require(bus),
                RoomFxBankSourceDefinitions.PaletteFx | pointer);
            AssertTrue(presentation.TryReadColor(pointer, out RgbColor actual),
                $"installed Wrecked Ship color resolves $8D:{pointer:X4}");
            AssertEqual(expected, actual,
                $"installed Wrecked Ship color matches cartridge $8D:{pointer:X4}");
            colorAddresses.Add(RoomFxBankSourceDefinitions.PaletteFx | pointer);
            colorAddresses.Add(RoomFxBankSourceDefinitions.PaletteFx |
                unchecked((ushort)(pointer + 1)));
        }
        AssertEqual(32, colorAddresses.Count,
            "Wrecked Ship presentation owns all 16 BGR555 source words");

        foreach (ushort definition in new ushort[]
                 {
                     WreckedShipGreenLightPaletteFxColorDefinitions.PoweredDefinition,
                 })
        {
            var guarded = NewTitlePresentationReadBus(bus, colorAddresses, forbidReads: true);
            var nativeSession = new PaletteFxSessionState();
            var native = new RoomPaletteFxSystem(nativeSession);
            var installedSession = new PaletteFxSessionState();
            var installed = new RoomPaletteFxSystem(installedSession);
            native.Start(definition, equippedItems: 0);
            installed.Start(definition, equippedItems: 0);
            var nativeCgram = new SnesCgram();
            var installedCgram = new SnesCgram();
            const int frames = 160;
            for (int frame = 0; frame < frames; frame++)
            {
                native.Step(new PaletteFxConditions { SamusY = 0, EquippedItems = 0, AreaMiniBossDefeated = false });
                PaletteFxRoomLook.LayShownShades(native, nativeCgram, new ReferencePaletteFxColorSource(bus));
                installed.Step(new PaletteFxConditions { SamusY = 0, EquippedItems = 0, AreaMiniBossDefeated = false });
                PaletteFxRoomLook.LayShownShades(installed, installedCgram, presentation);
                AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(installedCgram)),
                    $"installed Wrecked Ship definition ${definition:X4} equals native " +
                    $"output on frame {frame}");
            }
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"installed Wrecked Ship definition ${definition:X4} avoids color reads");
        }
    }

    /// <summary>Checks Maridia palette cycles frame by frame against native color-source execution.</summary>
    private static void VerifyExtractedMaridiaPaletteFxPresentation(
        ISnesAddressSpace bus,
        RoomPaletteFxPresentation presentation)
    {
        var colorAddresses = new HashSet<int>();
        foreach (MaridiaEnvironmentalPaletteFxColorDefinition definition in
                 MaridiaEnvironmentalPaletteFxColorDefinitions.All)
        {
            for (int frame = 0; frame < definition.FrameCount; frame++)
            for (int color = 0; color < definition.ColorsPerFrame; color++)
            {
                ushort pointer = definition.ColorPointer(frame, color);
                ushort expected = RomDataReader.ReadWordFixedBank(
                    CartridgeImportSource.Require(bus),
                    RoomFxBankSourceDefinitions.PaletteFx | pointer);
                AssertTrue(presentation.TryReadColor(pointer, out RgbColor actual),
                    $"installed Maridia color resolves $8D:{pointer:X4}");
                AssertEqual(expected, actual,
                    $"installed Maridia color matches cartridge $8D:{pointer:X4}");
                colorAddresses.Add(RoomFxBankSourceDefinitions.PaletteFx | pointer);
                colorAddresses.Add(RoomFxBankSourceDefinitions.PaletteFx |
                    unchecked((ushort)(pointer + 1)));
            }
        }
        AssertEqual(224, colorAddresses.Count,
            "Maridia presentation owns all 112 BGR555 source words");

        var guarded = NewTitlePresentationReadBus(bus, colorAddresses, forbidReads: true);
        var nativeSession = new PaletteFxSessionState();
        var native = new RoomPaletteFxSystem(nativeSession);
        var installedSession = new PaletteFxSessionState();
        var installed = new RoomPaletteFxSystem(installedSession);
        foreach (MaridiaEnvironmentalPaletteFxColorDefinition definition in
                 MaridiaEnvironmentalPaletteFxColorDefinitions.All)
        {
            native.Start(definition.DefinitionPointer, equippedItems: 0);
            installed.Start(definition.DefinitionPointer, equippedItems: 0);
        }

        var nativeCgram = new SnesCgram();
        var installedCgram = new SnesCgram();
        const int frames = 80;
        for (int frame = 0; frame < frames; frame++)
        {
            native.Step(new PaletteFxConditions { SamusY = 0, EquippedItems = 0, AreaMiniBossDefeated = false });
            PaletteFxRoomLook.LayShownShades(native, nativeCgram, new ReferencePaletteFxColorSource(bus));
            installed.Step(new PaletteFxConditions { SamusY = 0, EquippedItems = 0, AreaMiniBossDefeated = false });
            PaletteFxRoomLook.LayShownShades(installed, installedCgram, presentation);
            AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(installedCgram)),
                $"installed Maridia palette equals native output on frame {frame}");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed Maridia palette loops avoid cartridge color reads");
    }

    /// <summary>Checks schema version, shape, and color validation for malformed room palette-FX documents.</summary>
    private static void VerifyRoomPaletteFxPresentationValidation(
        byte[] extracted)
    {
        RoomPaletteFxPresentationDocument document =
            JsonSerializer.Deserialize<RoomPaletteFxPresentationDocument>(
                extracted,
                ArtworkJson.Options)
            ?? throw new InvalidDataException("Extracted room palette-FX document is null.");
        byte[] Json() => JsonSerializer.SerializeToUtf8Bytes(
            document,
            ArtworkJson.Options);
        void Reject(string description) => AssertThrows<InvalidDataException>(
            () => RoomPaletteFxPresentationFile.Read(new MemoryStream(Json(), writable: false)),
            description);

        int version = document.Version;
        document = document with { Version = version + 1 };
        Reject("room palette-FX rejects unsupported schema version");
        document = document with { Version = version };

        PaletteRgb5[][]? powerHeat = document.SamusHeatPowerSuit;
        document = document with { SamusHeatPowerSuit = null };
        Reject("room palette-FX rejects missing Power Suit heat colors");
        document = document with { SamusHeatPowerSuit = powerHeat };

        PaletteRgb5[][]? variaHeat = document.SamusHeatVariaSuit;
        document = document with { SamusHeatVariaSuit = (variaHeat ?? throw new InvalidOperationException("Verification requires variaHeat."))[..^1] };
        Reject("room palette-FX rejects incomplete Varia Suit heat cycle");
        document = document with { SamusHeatVariaSuit = variaHeat };

        PaletteRgb5? gravityColor = (document.SamusHeatGravitySuit ?? throw new InvalidOperationException("Verification requires document.SamusHeatGravitySuit."))[0][0];
        document.SamusHeatGravitySuit[0][0] = gravityColor with { Blue = 32 };
        Reject("room palette-FX rejects invalid Gravity Suit heat color");
        document.SamusHeatGravitySuit[0][0] = gravityColor;

        PaletteRgb5[][] heat = document.NorfairForegroundAndHeatPhase;
        document = document with { NorfairForegroundAndHeatPhase = heat[..^1] };
        Reject("room palette-FX rejects incomplete Norfair animation");
        document = document with { NorfairForegroundAndHeatPhase = heat };

        PaletteRgb5[] frame = document.NorfairGreenForeground[0];
        document.NorfairGreenForeground[0] = frame[..^1];
        Reject("room palette-FX rejects incomplete Norfair frame");
        document.NorfairGreenForeground[0] = frame;

        PaletteRgb5 first = document.NorfairPurpleForeground[0][0];
        document.NorfairPurpleForeground[0][0] = first with { Red = 32 };
        Reject("room palette-FX rejects non-RGB5 room color");
        document.NorfairPurpleForeground[0][0] = first;

        PaletteRgb5[][] waterfalls = document.MaridiaBackgroundWaterfalls;
        document = document with { MaridiaBackgroundWaterfalls = waterfalls[..^1] };
        Reject("room palette-FX rejects incomplete Maridia animation");
        document = document with { MaridiaBackgroundWaterfalls = waterfalls };

        PaletteRgb5[] sandFallFrame = document.MaridiaSandFalls[0];
        document.MaridiaSandFalls[0] = sandFallFrame[..^1];
        Reject("room palette-FX rejects incomplete Maridia frame");
        document.MaridiaSandFalls[0] = sandFallFrame;

        PaletteRgb5[][] greenLights = document.WreckedShipGreenLights;
        document = document with { WreckedShipGreenLights = greenLights[..^1] };
        Reject("room palette-FX rejects incomplete Wrecked Ship animation");
        document = document with { WreckedShipGreenLights = greenLights };

        PaletteRgb5[][] redBrinstar = document.RedBrinstarBackgroundGlow;
        document = document with { RedBrinstarBackgroundGlow = redBrinstar[..^1] };
        Reject("room palette-FX rejects incomplete Red Brinstar animation");
        document = document with { RedBrinstarBackgroundGlow = redBrinstar };

        PaletteRgb5[] tourianFrame = document.TourianGlow[0];
        document.TourianGlow[0] = tourianFrame[..^1];
        Reject("room palette-FX rejects incomplete Tourian glow frame");
        document.TourianGlow[0] = tourianFrame;

        PaletteRgb5[][] blueSpores = document.BrinstarBlueSpores;
        document = document with { BrinstarBlueSpores = blueSpores[..^1] };
        Reject("room palette-FX rejects incomplete blue-spore animation");
        document = document with { BrinstarBlueSpores = blueSpores };

        PaletteRgb5[] bombTorizoFrame = document.BombTorizoBelly[0];
        document.BombTorizoBelly[0] = bombTorizoFrame[..^1];
        Reject("room palette-FX rejects incomplete Bomb Torizo belly frame");
        document.BombTorizoBelly[0] = bombTorizoFrame;

        PaletteRgb5[][] statueGrey = document.TourianStatueGrey;
        document = document with { TourianStatueGrey = statueGrey[..^1] };
        Reject("room palette-FX rejects incomplete Tourian statue grey-out");
        document = document with { TourianStatueGrey = statueGrey };

        PaletteRgb5[][] surfaceLightning = document.CrateriaSurfaceLightning;
        document = document with { CrateriaSurfaceLightning = surfaceLightning[..^1] };
        Reject("room palette-FX rejects incomplete Crateria surface lightning");
        document = document with { CrateriaSurfaceLightning = surfaceLightning };

        PaletteRgb5[][] gunshipLights = document.CeresGunshipEngineLights;
        document = document with { CeresGunshipEngineLights = gunshipLights[..^1] };
        Reject("room palette-FX rejects incomplete Ceres gunship-engine lights");
        document = document with { CeresGunshipEngineLights = gunshipLights };

        PaletteRgb5[] navigationFrame = document.CeresNavigationLights[0];
        document.CeresNavigationLights[0] = navigationFrame[..^1];
        Reject("room palette-FX rejects incomplete Ceres navigation-light frame");
        document.CeresNavigationLights[0] = navigationFrame;

        PaletteRgb5[][] fadeIn = document.PlanetZebesTextFadeIn;
        document = document with { PlanetZebesTextFadeIn = fadeIn[..^1] };
        Reject("room palette-FX rejects incomplete PLANET ZEBES fade-in");
        document = document with { PlanetZebesTextFadeIn = fadeIn };

        PaletteRgb5[] fadeOutFrame = document.PlanetZebesTextFadeOut[0];
        document.PlanetZebesTextFadeOut[0] = fadeOutFrame[..^1];
        Reject("room palette-FX rejects incomplete PLANET ZEBES fade-out frame");
        document.PlanetZebesTextFadeOut[0] = fadeOutFrame;

        PaletteRgb5[][] motherBrainLights = document.OldMotherBrainBackgroundLights;
        document = document with
        {
            OldMotherBrainBackgroundLights = motherBrainLights[..^1],
        };
        Reject("room palette-FX rejects incomplete old Mother Brain glow");
        document = document with { OldMotherBrainBackgroundLights = motherBrainLights };

        PaletteRgb5[] gunshipGlowFrame = document.CinematicGunshipGlow[0];
        document.CinematicGunshipGlow[0] = gunshipGlowFrame[..^1];
        Reject("room palette-FX rejects incomplete cinematic gunship-glow frame");
        document.CinematicGunshipGlow[0] = gunshipGlowFrame;

        PaletteRgb5[][] zebesFade = document.ExplodingZebesFade;
        document = document with { ExplodingZebesFade = zebesFade[..^1] };
        Reject("room palette-FX rejects incomplete exploding-Zebes fade");
        document = document with { ExplodingZebesFade = zebesFade };

        PaletteRgb5[][] titleLogo = document.TitleLogoFade;
        document = document with { TitleLogoFade = titleLogo[..^1] };
        Reject("room palette-FX rejects incomplete title-logo fade");
        document = document with { TitleLogoFade = titleLogo };

        PaletteRgb5[] nintendoFrame = document.NintendoSharedFade[0];
        document.NintendoSharedFade[0] = nintendoFrame[..^1];
        Reject("room palette-FX rejects incomplete Nintendo shared-fade frame");
        document.NintendoSharedFade[0] = nintendoFrame;

        PaletteRgb5[][] explosionForeground = document.ZebesExplosionForeground;
        document = document with { ZebesExplosionForeground = explosionForeground[..^1] };
        Reject("room palette-FX rejects incomplete Zebes explosion foreground");
        document = document with { ZebesExplosionForeground = explosionForeground };

        PaletteRgb5[] finaleFrame = document.ZebesExplosionFinale[0];
        document.ZebesExplosionFinale[0] = finaleFrame[..^1];
        Reject("room palette-FX rejects incomplete Zebes explosion finale frame");
        document.ZebesExplosionFinale[0] = finaleFrame;

        PaletteRgb5[][] whiteout = document.ZebesExplosionWhiteout;
        document = document with { ZebesExplosionWhiteout = whiteout[..^1] };
        Reject("room palette-FX rejects incomplete shared whiteout");
        document = document with { ZebesExplosionWhiteout = whiteout };

        PaletteRgb5[][] afterglow = document.ZebesExplosionAfterglow;
        document = document with { ZebesExplosionAfterglow = afterglow[..^1] };
        Reject("room palette-FX rejects incomplete Zebes afterglow");
        document = document with { ZebesExplosionAfterglow = afterglow };

        PaletteRgb5[] lavaFrame = document.ZebesExplosionLava[0];
        document.ZebesExplosionLava[0] = lavaFrame[..^1];
        Reject("room palette-FX rejects incomplete Zebes explosion lava frame");
        document.ZebesExplosionLava[0] = lavaFrame;

        PaletteRgb5[][] crust = document.ZebesExplosionCrust;
        document = document with { ZebesExplosionCrust = crust[..^1] };
        Reject("room palette-FX rejects incomplete Zebes explosion crust fade");
        document = document with { ZebesExplosionCrust = crust };

        PaletteRgb5[] greyCloudFrame = document.ZebesExplosionGreyClouds[0];
        document.ZebesExplosionGreyClouds[0] = greyCloudFrame[..^1];
        Reject("room palette-FX rejects incomplete Zebes grey-cloud frame");
        document.ZebesExplosionGreyClouds[0] = greyCloudFrame;

        PaletteRgb5[][] explosionGunship = document.ZebesExplosionGunship;
        document = document with { ZebesExplosionGunship = explosionGunship[..^1] };
        Reject("room palette-FX rejects incomplete explosion gunship reveal");
        document = document with { ZebesExplosionGunship = explosionGunship };

        PaletteRgb5[][] powerSuit = document.SamusLoadingPowerSuit;
        document = document with { SamusLoadingPowerSuit = powerSuit[..^1] };
        Reject("room palette-FX rejects incomplete Samus power-suit loading");
        document = document with { SamusLoadingPowerSuit = powerSuit };

        PaletteRgb5[] variaFrame = document.SamusLoadingVariaSuit[0];
        document.SamusLoadingVariaSuit[0] = variaFrame[..^1];
        Reject("room palette-FX rejects incomplete Samus Varia-suit frame");
        document.SamusLoadingVariaSuit[0] = variaFrame;

        PaletteRgb5[][] gravitySuit = document.SamusLoadingGravitySuit;
        document = document with { SamusLoadingGravitySuit = gravitySuit[..^1] };
        Reject("room palette-FX rejects incomplete Samus gravity-suit loading");
        document = document with { SamusLoadingGravitySuit = gravitySuit };

        PaletteRgb5[] iconFrame = document.PostCreditsIconGlare[0];
        document.PostCreditsIconGlare[0] = iconFrame[..^1];
        Reject("room palette-FX rejects incomplete post-credits icon-glare frame");
        document.PostCreditsIconGlare[0] = iconFrame;

        PaletteRgb5[][] shutter = document.TourianEscapeShutter;
        document = document with { TourianEscapeShutter = shutter[..^1] };
        Reject("room palette-FX rejects incomplete Tourian escape shutter flash");
        document = document with { TourianEscapeShutter = shutter };

        PaletteRgb5[] backgroundFrame = document.TourianEscapeBackground[0];
        document.TourianEscapeBackground[0] = backgroundFrame[..^1];
        Reject("room palette-FX rejects incomplete Tourian escape background frame");
        document.TourianEscapeBackground[0] = backgroundFrame;

        PaletteRgb5[][] shared = document.TourianEscapeSharedRedFlash;
        document = document with { TourianEscapeSharedRedFlash = shared[..^1] };
        Reject("room palette-FX rejects incomplete shared Tourian escape flash");
        document = document with { TourianEscapeSharedRedFlash = shared };

        PaletteRgb5[] oldRedFrame = document.OldTourianEscapeRedFlash[0];
        document.OldTourianEscapeRedFlash[0] = oldRedFrame[..^1];
        Reject("room palette-FX rejects incomplete old Tourian escape red-flash frame");
        document.OldTourianEscapeRedFlash[0] = oldRedFrame;

        PaletteRgb5[][] railings = document.OldTourianEscapeOrangeRailings;
        document = document with { OldTourianEscapeOrangeRailings = railings[..^1] };
        Reject("room palette-FX rejects incomplete old Tourian orange railings");
        document = document with { OldTourianEscapeOrangeRailings = railings };

        PaletteRgb5[] panelsFrame = document.OldTourianEscapeYellowPanels[0];
        document.OldTourianEscapeYellowPanels[0] = panelsFrame[..^1];
        Reject("room palette-FX rejects incomplete old Tourian yellow-panel frame");
        document.OldTourianEscapeYellowPanels[0] = panelsFrame;

        PaletteRgb5[][] upperFlash = document.UpperCrateriaEscapeRedFlash;
        document = document with { UpperCrateriaEscapeRedFlash = upperFlash[..^1] };
        Reject("room palette-FX rejects incomplete upper Crateria escape red flash");
        document = document with { UpperCrateriaEscapeRedFlash = upperFlash };

        PaletteRgb5[] lightningFrame = document.CrateriaEscapeYellowLightning[0];
        document.CrateriaEscapeYellowLightning[0] = lightningFrame[..^1];
        Reject("room palette-FX rejects incomplete Crateria escape lightning frame");
        document.CrateriaEscapeYellowLightning[0] = lightningFrame;

        PaletteRgb5[][] pixel = document.ParlorEscapeSky;
        document = document with { ParlorEscapeSky = pixel[..^1] };
        Reject("room palette-FX rejects incomplete Crateria escape CRE pixel");
        document = document with { ParlorEscapeSky = pixel };

        PaletteRgb5[] beaconFrame = document.BeaconFlashing[0];
        document.BeaconFlashing[0] = beaconFrame[..^1];
        Reject("room palette-FX rejects incomplete beacon flash frame");
        document.BeaconFlashing[0] = beaconFrame;

        string unknownField = Encoding.UTF8.GetString(extracted).Replace(
            $"\"version\": {RoomPaletteFxPresentationFormat.Version}",
            $"\"version\": {RoomPaletteFxPresentationFormat.Version},\n  \"nativeAddress\": 9240718",
            StringComparison.Ordinal);
        AssertThrows<InvalidDataException>(
            () => RoomPaletteFxPresentationFile.Read(new MemoryStream(
                Encoding.UTF8.GetBytes(unknownField), writable: false)),
            "room palette-FX rejects native-address escape hatch");
    }

    /// <summary>
    /// Starts <paramref name="definition"/>'s effect as the game starts <paramref name="shape"/>'s family: a Torizo belly glow
    /// with the Torizo it lights, as that Torizo's fight start hands itself to the glow (#1700); every other family plainly.
    /// </summary>
    private static void StartFamily(RoomPaletteFxSystem fx, PaletteFxFamilyShape shape, ushort definition)
    {
        if (shape.LitTorizo is { } torizo)
            fx.StartTorizoBellyGlow(definition, torizo, equippedItems: 0);
        else
            fx.Start(definition, equippedItems: 0);
    }

    /// <summary>A Torizo that keeps its health through the check, so the belly glow lighting it plays its whole cycle.</summary>
    private sealed class LivingTorizo : ILitTorizo
    {
        /// <summary>The one Torizo.</summary>
        internal static LivingTorizo Instance { get; } = new();

        /// <inheritdoc/>
        public bool IsDefeated => false;
    }

    /// <summary>Runs one handler pass of <paramref name="fx"/> as <paramref name="shape"/>'s family plays: on a frontend screen or in gameplay.</summary>
    private static void StepFamily(RoomPaletteFxSystem fx, PaletteFxFamilyShape shape, PaletteFxConditions conditions)
    {
        if (shape.OnScreen)
            fx.StepScreen(conditions);
        else
            fx.Step(conditions);
    }
}
