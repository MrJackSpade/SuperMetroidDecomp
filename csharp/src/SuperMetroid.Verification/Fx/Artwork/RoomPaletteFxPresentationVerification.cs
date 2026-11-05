using SuperMetroid.AssetExtraction.Fx.Artwork;
using System.Text;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
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
        SuperMetroidAddressSpace bus)
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
            native.SpawnDefinition(definition.DefinitionPointer, equippedItems: 0);
            installed.SpawnDefinition(definition.DefinitionPointer, equippedItems: 0);
        }

        var nativeCgram = new SnesCgram();
        var installedCgram = new SnesCgram();
        int frames = NorfairEnvironmentalPaletteFxColorDefinitions.CycleFrames * 2;
        for (int frame = 0; frame < frames; frame++)
        {
            native.Step(new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
            PaletteFxRoomLook.LayShownShades(native, nativeCgram, new ReferencePaletteFxColorSource(bus));
            installed.Step(new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
            PaletteFxRoomLook.LayShownShades(installed, installedCgram, presentation);
            AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(installedCgram)),
                $"installed Norfair palette equals native output on frame {frame}");
            AssertEqual(nativeSession.SamusInHeatPaletteIndex, installedSession.SamusInHeatPaletteIndex,
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
                TourianGlowPaletteFxColorDefinitions.CloneDefinitionPointer,
            ] }, TourianGlowPaletteFxColorDefinitions.CycleFrames * 2));
        foreach (BrinstarBlueSporePaletteFxColorDefinition definition in
                 BrinstarBlueSporePaletteFxColorDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(bus, presentation, $"Brinstar blue spores ({definition.Owner})", new PaletteFxFamilyShape { FrameCount = BrinstarBlueSporePaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = BrinstarBlueSporePaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = definition.ColorPointer, Definitions = [definition.DefinitionPointer] }, BrinstarBlueSporePaletteFxColorDefinitions.CycleFrames * 2);
        }
        foreach (TorizoBellyPaletteFxColorDefinition definition in
                 TorizoBellyPaletteFxColorDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(bus, presentation, $"{definition.Owner} belly", new PaletteFxFamilyShape { FrameCount = TorizoBellyPaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = TorizoBellyPaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = definition.ColorPointer, Definitions = [definition.DefinitionPointer] }, TorizoBellyPaletteFxColorDefinitions.CycleFrames * 2);
        }
        Suite(nameof(VerifyInstalledPaletteFxFamily), () => VerifyInstalledPaletteFxFamily(bus, presentation, "Tourian statue grey-out", new PaletteFxFamilyShape { FrameCount = TourianStatueGreyPaletteFxColorDefinitions.ColorFrames.FrameCount, ColorsPerFrame = TourianStatueGreyPaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = TourianStatueGreyPaletteFxColorDefinitions.ColorPointer, Definitions = TourianStatueGreyPaletteFxColorToolingDefinitions.All
                .Select(definition => definition.DefinitionPointer)
                .ToArray() }, TourianStatueGreyPaletteFxColorDefinitions.FramesThroughDeletion));
        foreach (CrateriaLightningPaletteFxColorDefinition definition in
                 CrateriaLightningPaletteFxColorDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(bus, presentation, $"Crateria {definition.Owner}", new PaletteFxFamilyShape { FrameCount = definition.Frames.Count, ColorsPerFrame = definition.ColorsPerFrame, ColorPointer = definition.ColorPointer, Definitions = [definition.DefinitionPointer] }, definition.CycleFrames * 2, CrateriaLightningPaletteFxColorDefinitions.VerticalSwitchSamusY);
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
            VerifyInstalledPaletteFxFamily(bus, presentation, $"PLANET ZEBES {definition.Owner}", new PaletteFxFamilyShape { FrameCount = PlanetZebesTextPaletteFxColorDefinitions.FrameCount, ColorsPerFrame = PlanetZebesTextPaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = definition.ColorPointer, Definitions = [definition.DefinitionPointer], OnScreen = true }, PlanetZebesTextPaletteFxColorDefinitions.CycleFrames + 1);
        }
        foreach (CinematicGlowPaletteFxColorDefinition definition in
                 CinematicGlowPaletteFxColorDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(bus, presentation, $"Cinematic glow {definition.Owner}", new PaletteFxFamilyShape { FrameCount = CinematicGlowPaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = definition.ColorsPerFrame, ColorPointer = definition.ColorPointer, Definitions = [definition.DefinitionPointer], OnScreen = true }, definition.CycleFrames * 2);
        }
        Suite(nameof(VerifyInstalledPaletteFxFamily), () => VerifyInstalledPaletteFxFamily(bus, presentation, "exploding Zebes fade", new PaletteFxFamilyShape { FrameCount = ExplodingZebesFadePaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = ExplodingZebesFadePaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = ExplodingZebesFadePaletteFxColorDefinitions.ColorPointer, Definitions = [ExplodingZebesFadePaletteFxColorDefinitions.DefinitionPointer], OnScreen = true }, ExplodingZebesFadePaletteFxColorDefinitions.CycleFrames + 1));
        Suite(nameof(VerifyInstalledPaletteFxFamily), () => VerifyInstalledPaletteFxFamily(bus, presentation, "unused cinematic fade", new PaletteFxFamilyShape { FrameCount = UnusedCinematicFadePaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = UnusedCinematicFadePaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = UnusedCinematicFadePaletteFxColorDefinitions.ColorPointer, Definitions = [UnusedCinematicFadePaletteFxColorDefinitions.DefinitionPointer], OnScreen = true }, UnusedCinematicFadePaletteFxColorDefinitions.CycleFrames + 1));
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

    /// <summary>Checks the ending gunship's calculated palette samples against its native cycle.</summary>
    internal static void VerifyExtractedEndingGunshipPaletteFxPresentation(
        ISnesAddressSpace bus, RoomPaletteFxPresentation presentation)
    {
        Suite(nameof(VerifyInstalledPaletteFxFamily), () => VerifyInstalledPaletteFxFamily(bus, presentation, "Zebes explosion gunship", new PaletteFxFamilyShape { FrameCount = 16, ColorsPerFrame = 16, ColorPointer = (frame, color) => (ushort)(0xd6c0 + frame * 36 + color * 2), Definitions = [0xe1e4], OnScreen = true }, 385));
        var original = new Dictionary<ushort, ushort>();
        var coordinates = new Dictionary<ushort, Coordinate>();
        for (int frame = 0; frame < 16; frame++)
        for (int color = 0; color < 16; color++)
        {
            ushort pointer = (ushort)(0xd6c0 + frame * 36 + color * 2);
            original.Add(pointer, RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x8d0000 | pointer));
            coordinates.Add(pointer, new Coordinate(frame, color));
        }
        // Restrict the input view to the actual remaining source values, ensuring
        // the composed calculation works without generated endpoint/sample caches.
        var inputs = original.Where(pair => coordinates[pair.Key].Frame == 15 || coordinates[pair.Key] == new Coordinate(7, 15))
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        AssertEqual(17, inputs.Count, "gunship reveal independent source-color count");
        ushort[] inputPointers = inputs.Keys.ToArray();
        var inputView = new EndingGunshipPaletteInputView(PaletteFxColorAddressAccess.Keyed(ToColors(inputs)));
        AssertTrue(inputPointers.ToHashSet().SetEquals(inputView.Pointers), "gunship input identities survive shared-channel calculation");
        AssertEqual(17, inputView.Count, "gunship input view keeps its complete logical key count");
        foreach (ushort pointer in inputPointers)
            AssertEqual(original[pointer], inputView[PaletteFxColorAddressAccess.KeyAt(pointer)], "original gunship base word reconstructed from independent channels");
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort pointer = (ushort)raw;
            bool owned = coordinates.TryGetValue(pointer, out var expected);
            AssertEqual(owned, EndingGunshipPaletteColors.TryCoordinates(pointer, out int frame, out int color),
                "gunship exact original color-word ownership");
            if (owned)
            {
                AssertEqual(expected.Frame, frame, "gunship original frame identity");
                AssertEqual(expected.Color, color, "gunship original color column");
            }
            bool computed = owned && expected.Frame != 15 && expected != new Coordinate(7, 15);
            AssertEqual(computed, EndingGunshipPaletteColors.TryCalculatedColor(pointer, inputView, out RgbColor calculated),
                "gunship calculated color domain");
            if (computed) AssertEqual(original[pointer], calculated, "gunship calculated color matches original ROM");
        }
        ushort[] identities = presentation.ColorPointers.Where(original.ContainsKey).ToArray();
        AssertEqual(256, identities.Length, "gunship auditor identities include calculated colors");
        AssertEqual(256, identities.Distinct().Count(), "gunship auditor identities are unique");
        AssertTrue(!EndingGunshipPaletteColors.TryCalculatedColor(0xd6e4, ToColors(new Dictionary<ushort, ushort>()), out _),
            "gunship does not invent missing interpolation inputs");
    }

    /// <summary>Checks post-credits logo glare ownership, calculated colors, and installed playback.</summary>
    internal static void VerifyExtractedLogoGlarePaletteFxPresentation(
        ISnesAddressSpace bus, RoomPaletteFxPresentation presentation)
    {
        Suite(nameof(VerifyInstalledPaletteFxFamily), () => VerifyInstalledPaletteFxFamily(bus, presentation, "post-credits icon glare", new PaletteFxFamilyShape { FrameCount = 14, ColorsPerFrame = 16, ColorPointer = (frame, color) => (ushort)(0xdf9a + frame * 36 + color * 2), Definitions = [0xe200], OnScreen = true }, 15));
        var original = new Dictionary<ushort, ushort>();
        var coordinates = new Dictionary<ushort, Coordinate>();
        for (int frame = 0; frame < 14; frame++)
        for (int color = 0; color < 16; color++)
        {
            ushort pointer = (ushort)(0xdf9a + frame * 36 + color * 2);
            original.Add(pointer, RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x8d0000 | pointer));
            coordinates.Add(pointer, new Coordinate(frame, color));
        }
        // The original words keyed by record colour once, for every address the domain sweep asks about.
        Dictionary<PaletteFxColorKey, RgbColor> originalColors = PaletteFxColorAddressAccess.Keyed(ToColors(original));
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort pointer = (ushort)raw;
            bool owned = coordinates.TryGetValue(pointer, out var expected);
            AssertEqual(owned, LogoGlarePaletteColors.TryCoordinates(pointer, out int frame, out int color),
                "glare owns exact original color words, excluding controls and odd bytes");
            if (owned)
            {
                AssertEqual(expected.Frame, frame, "glare original frame identity");
                AssertEqual(expected.Color, color, "glare original color column");
            }
            bool computed = owned && expected.Frame < 13;
            AssertEqual(computed, LogoGlarePaletteColors.TryCalculatedColor(pointer, originalColors, out RgbColor calculated),
                "glare intermediate-color calculation domain");
            if (computed) AssertEqual(original[pointer], calculated, "glare calculated word matches original ROM");
        }
        ushort[] identities = presentation.ColorPointers.Where(original.ContainsKey).ToArray();
        AssertEqual(224, identities.Length, "glare computed and supplied identities remain enumerable");
        AssertEqual(224, identities.Distinct().Count(), "glare identities have no duplicates");
        AssertTrue(!LogoGlarePaletteColors.TryCalculatedColor(0xdf9a, ToColors(new Dictionary<ushort, ushort>()), out _),
            "glare does not invent a missing base color");
    }

    /// <summary>Checks Samus loading colors, endpoint channels, aliases, and installed cycle output.</summary>
    internal static void VerifyExtractedSamusLoadingPaletteFxPresentation(
        ISnesAddressSpace bus, RoomPaletteFxPresentation presentation)
    {
        var stored = presentation.colors;
        var inputView = presentation.loadingInputs;
        ushort[] splitEndpoints = [0xdc7e, 0xdc80, 0xdba1, 0xdc94, 0xdd09, 0xdd0b, 0xddfa, 0xdf4c];
        LoadingPaletteInputView.Channels[] endpointChannels =
        [
            inputView.powerDim1, inputView.powerDim2, inputView.powerBright9, inputView.powerDim12,
            inputView.variaBright10, inputView.variaBright11, inputView.variaDim12, inputView.gravityDim2,
        ];
        int independentComponents = endpointChannels.Sum(channels =>
            (channels.red is null ? 0 : 1) + (channels.green is null ? 0 : 1) + (channels.blue is null ? 0 : 1));
        AssertEqual(9, independentComponents, "Stock endpoint words reduce to nine independent channel inputs");
        var aliases = new Dictionary<ushort, ushort>();
        List<RoomPaletteFxPresentationRow>? powerRows = null;
        var tinted = new HashSet<ushort>();
        foreach (int start in new[] { 0xdb62, 0xdcc8, 0xde2e })
        {
            var rows = new List<RoomPaletteFxPresentationRow>();
            int cursor = start + 4;
            for (int group = 0; group < 4; group++)
            {
                AssertEqual((ushort)0xc648, ReadVerificationWord(bus, 0x8d0000 | cursor), "Loading row decoder timer command");
                cursor += 3;
                for (int frame = 0; frame < 2; frame++)
                {
                    cursor += 2;
                    rows.Add(new RoomPaletteFxPresentationRow(cursor, Enumerable.Range(0, 16).Select(color => ReadVerificationWord(bus, 0x8d0000 | (cursor + 2 * color))).ToArray()));
                    cursor += 34;
                }
                AssertEqual((ushort)0xc639, ReadVerificationWord(bus, 0x8d0000 | cursor), "Loading row decoder replay command");
                cursor += 4;
            }
            cursor += 2;
            rows.Add(new RoomPaletteFxPresentationRow(cursor, Enumerable.Range(0, 16).Select(color => ReadVerificationWord(bus, 0x8d0000 | (cursor + 2 * color))).ToArray()));
            if (start == 0xdb62) powerRows = rows;
            foreach (int rowIndex in new[] { 1, 5, 7 })
            foreach (int slot in start == 0xdb62 ? new[] { 3, 4, 5, 6, 7, 8, 13, 14, 15 } : start == 0xde2e ? new[] { 10, 11 } : Array.Empty<int>())
                tinted.Add((ushort)(rows[rowIndex].Pointer + 2 * slot));
            if (start == 0xdcc8)
            foreach (int rowIndex in new[] { 1, 5, 7 })
            foreach (int slot in rowIndex == 1 ? new[] { 1, 2 } : new[] { 1, 2, 10, 11 })
                tinted.Add((ushort)(rows[rowIndex].Pointer + 2 * slot));
            foreach (int rowIndex in new[] { 1, 5 })
            foreach (int slot in start == 0xdb62 ? new[] { 1, 2, 10, 11, 12 } : start == 0xdcc8 ? new[] { 12 } : new[] { 2 })
                tinted.Add((ushort)(rows[rowIndex].Pointer + 2 * slot));
            if (start == 0xdb62)
            {
                foreach (int slot in new[] { 0, 9, 10, 11 }) tinted.Add((ushort)(rows[7].Pointer + 2 * slot));
                tinted.Add((ushort)(rows[5].Pointer + 2 * 9));
            }
            foreach (var row in rows)
            {
                var original = rows.First(candidate => candidate.Colors.SequenceEqual(row.Colors));
                for (int color = 0; color < 16; color++)
                {
                    ushort pointer = (ushort)(row.Pointer + 2 * color);
                    ushort canonical = (ushort)(original.Pointer + 2 * color);
                    var power = (powerRows ?? throw new InvalidOperationException("Verification requires powerRows."))[rows.IndexOf(original)];
                    if (power.Colors[color] == row.Colors[color]) canonical = (ushort)(power.Pointer + 2 * color);
                    if (start == 0xdcc8 && color == 0 && rows.IndexOf(original) == 0)
                    {
                        AssertEqual(row.Colors[0], powerRows[1].Colors[0], "Varia normal and Power bright transparent words match natively");
                        canonical = (ushort)powerRows[1].Pointer;
                    }
                    aliases.Add(pointer, canonical);
                    AssertTrue(presentation.TryReadColor(pointer, out RgbColor actual), "All native loading colors remain installed");
                    AssertEqual(row.Colors[color], actual, "Loading color equals original payload");
                    AssertEqual(pointer == canonical && !tinted.Contains(pointer) && !splitEndpoints.Contains(pointer), stored.ContainsKey(pointer), "Stock stores only independent suit/shade inputs");
                }
            }
        }
        AssertEqual(432, aliases.Count, "Loading color domain");
        AssertEqual(62, tinted.Count, "Base tints, endpoint brightening and shared channels");
        AssertEqual(25, aliases.Keys.Count(stored.ContainsKey), "Endpoint splitting and transparent sharing remove whole words");
        foreach (int invalid in new[] { -1, 3, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => LoadingPaletteColors.TintColor(new RgbColor(0, 0, 0), invalid), "Tint shade rejects outside domain");
        foreach (int invalid in new[] { -1, 3, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => LoadingPaletteColors.VariaTintColor(new RgbColor(0, 0, 0), invalid), "Varia tint shade rejects outside domain");
        foreach (int invalid in new[] { -1, 2, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => LoadingPaletteColors.BrightenDimColor(new RgbColor(0, 0, 0), invalid, 0, false), "Dim brightening rejects invalid shade");
        foreach (int invalid in new[] { -1, 32, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => LoadingPaletteColors.BrightenDimColor(new RgbColor(0, 0, 0), 0, invalid, false), "Dim brightening rejects invalid green peak");
        AssertEqual(95, aliases.Count(item => item.Key == item.Value), "Shared suit and transparent colors reduce four-row inputs");
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool expected = aliases.TryGetValue((ushort)address, out ushort canonical);
            AssertEqual(expected, LoadingPaletteColors.TryCanonicalPointer((ushort)address, out ushort actual), "Complete loading color address domain");
            AssertEqual(canonical, actual, "Original earliest identical row or unowned zero");
            bool calculated = LoadingPaletteColors.TryCalculatedColor((ushort)address, inputView, out RgbColor tint);
            AssertEqual(tinted.Contains((ushort)address), calculated, "Complete tint-only address domain");
            AssertEqual(calculated ? ReadVerificationWord(bus, 0x8d0000 | address) : (ushort)0, tint, "Original tint word or unowned zero");
        }
        // The auditor's identities are enumerated once; each alias is then looked up among them.
        HashSet<ushort> auditedPointers = [.. presentation.ColorPointers];
        AssertTrue(aliases.Keys.All(auditedPointers.Contains), "Auditor includes calculated loading aliases");
        foreach (SamusLoadingSuitPaletteFxColorDefinition definition in
                 SamusLoadingSuitPaletteFxColorDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(bus, presentation, $"Samus loading {definition.Owner}", new PaletteFxFamilyShape { FrameCount = SamusLoadingSuitPaletteFxColorDefinitionsConstants.FrameCount, ColorsPerFrame = SamusLoadingSuitPaletteFxColorDefinitions.ColorsPerFrame, ColorPointer = definition.ColorPointer, Definitions = [definition.DefinitionPointer] }, SamusLoadingSuitPaletteFxColorDefinitions.CycleFrames + 1);
        }
    }

    /// <summary>Checks Samus heat palette derivation and suit cycles without runtime color-ROM reads.</summary>
    internal static void VerifyExtractedSamusHeatPaletteFxPresentation(
        ISnesAddressSpace bus, RoomPaletteFxPresentation presentation)
    {
        var stored = presentation.colors;
        var inputs = presentation.heatInputs;
        foreach (int? channel in new[] { inputs.greenOverride, inputs.blueOverride, inputs.mixedBlueOverride })
            AssertTrue(channel is null, "Stock endpoint channels are calculated, not stored overrides");
        var expectedAliases = new Dictionary<ushort, ushort>();
        foreach (int first in new[] { 0xe468, 0xe694, 0xe8c0 })
        for (int phase = 0; phase < 16; phase++)
        {
            // Independently discover the first identical original row, not the
            // mirrored formula under test. This reads only the identified native data.
            int earliest = 0;
            for (; earliest < phase; earliest++)
            {
                bool same = true;
                for (int color = 0; color < 15; color++)
                    same &= ReadVerificationWord(bus, 0x8d0000 | (first + phase * 34 + color * 2)) ==
                        ReadVerificationWord(bus, 0x8d0000 | (first + earliest * 34 + color * 2));
                if (same) break;
            }
            for (int color = 0; color < 15; color++)
            {
                ushort pointer = (ushort)(first + phase * 34 + color * 2);
                ushort canonical = (ushort)(first + earliest * 34 + color * 2);
                ushort power = (ushort)(0xe468 + earliest * 34 + color * 2);
                if (first != 0xe468 && ReadVerificationWord(bus, 0x8d0000 | canonical) ==
                    ReadVerificationWord(bus, 0x8d0000 | power)) canonical = power;
                int canonicalFirst = canonical >= 0xe8c0 ? 0xe8c0 : canonical >= 0xe694 ? 0xe694 : 0xe468;
                bool constant = true;
                for (int sample = 1; sample < 16; sample++)
                    constant &= ReadVerificationWord(bus, 0x8d0000 | (canonicalFirst + color * 2)) ==
                        ReadVerificationWord(bus, 0x8d0000 | (canonicalFirst + sample * 34 + color * 2));
                if (constant) canonical = (ushort)(canonicalFirst + color * 2);
                expectedAliases.Add(pointer, canonical);

            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
        {
            bool owned = expectedAliases.TryGetValue((ushort)pointer, out ushort expected);
            AssertEqual(owned, HeatPaletteColors.TryCanonicalPointer((ushort)pointer, out ushort actual),
                "Every native color pointer/odd/control exclusion");
            AssertEqual(expected, actual, "Original repeated-row alias or unowned zero");
        }
        AssertEqual(0, expectedAliases.Keys.Count(stored.ContainsKey), "No complete stock heat word remains stored");
        var baseSources = new Dictionary<ushort, ushort>();
        foreach (var (heat, loading, normal) in new VerifyExtractedSamusHeatPaletteFxPresentationCase[]
            { new VerifyExtractedSamusHeatPaletteFxPresentationCase(0xe468, 0xdb6d, 0x9b9402), new VerifyExtractedSamusHeatPaletteFxPresentationCase(0xe694, 0xdcd3, 0x9b9522), new VerifyExtractedSamusHeatPaletteFxPresentationCase(0xe8c0, 0xde39, 0x9b9802) })
        for (int color = 0; color < 15; color++)
        {
            ushort heatPointer = (ushort)(heat + 2 * color);
            ushort loadingPointer = (ushort)(loading + 2 * color);
            ushort native = ReadVerificationWord(bus, 0x8d0000 | heatPointer);
            AssertEqual(native, ReadVerificationWord(bus, 0x8d0000 | loadingPointer), "Initial heat/loading suit colors share original art");
            AssertEqual(native, ReadVerificationWord(bus, normal + 2 * color), "Independent normal suit palette corroborates source");
            baseSources.Add(heatPointer, loadingPointer);
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
        {
            bool owned = baseSources.TryGetValue((ushort)pointer, out ushort expected);
            AssertEqual(owned, HeatPaletteColors.TryBasePalettePointer((ushort)pointer, out ushort actual), "Complete heat base-pointer domain");
            AssertEqual(expected, actual, "Exact loading source or unowned zero");
        }
        foreach (ushort pointer in expectedAliases.Values.Distinct())
        {
            if (!HeatPaletteColors.TryCalculatedColor(pointer, inputs, out RgbColor calculated))
            {
                AssertTrue(inputs.ContainsKey(pointer), "Independent endpoint input remains available");
                continue;
            }
            AssertEqual(ReadVerificationWord(bus, 0x8d0000 | pointer), calculated, "Every calculated shared-red color matches original");
            AssertTrue(!stored.ContainsKey(pointer), "Calculated shared-red word is not cached");
        }
        // The audit's identities are enumerated once; each alias is then looked up among them.
        HashSet<ushort> auditedPointers = [.. presentation.ColorPointers];
        AssertTrue(expectedAliases.Keys.All(auditedPointers.Contains), "Audit enumeration includes removed aliases");
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
            native.SpawnDefinition(RoomPaletteFxDefinitions.SamusInHeat, SamusEquipmentWordCodec.FromEquipmentWord(equipment));
            installed.SpawnDefinition(RoomPaletteFxDefinitions.SamusInHeat, SamusEquipmentWordCodec.FromEquipmentWord(equipment));
            var nativeCgram = new SnesCgram();
            var installedCgram = new SnesCgram();
            int steps = 1 + Enumerable.Range(0, definition.Frames.Count)
                .Sum(phase => PaletteFxHeatProgramDefinitionConstants.Duration(definition.Suit, phase));
            for (int step = 0; step <= steps; step++)
            {
                native.Step(new PaletteFxConditions { SamusY = 0, EquippedItems = SamusEquipmentWordCodec.FromEquipmentWord(equipment), EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
                PaletteFxRoomLook.LayShownShades(native, nativeCgram, new ReferencePaletteFxColorSource(bus));
                installed.Step(new PaletteFxConditions { SamusY = 0, EquippedItems = SamusEquipmentWordCodec.FromEquipmentWord(equipment), EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
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
            native.SpawnDefinition(definition, equippedItems: 0);
            installed.SpawnDefinition(definition, equippedItems: 0);
            var nativeCgram = new SnesCgram();
            var installedCgram = new SnesCgram();
            for (int frame = 0; frame < framesToRun; frame++)
            {
                var conditions = new PaletteFxConditions { SamusY = samusY, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false };
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
                     WreckedShipGreenLightPaletteFxColorDefinitions.PoweredDefinitionAlternate,
                 })
        {
            var guarded = NewTitlePresentationReadBus(bus, colorAddresses, forbidReads: true);
            var nativeSession = new PaletteFxSessionState();
            var native = new RoomPaletteFxSystem(nativeSession);
            var installedSession = new PaletteFxSessionState();
            var installed = new RoomPaletteFxSystem(installedSession);
            native.SpawnDefinition(definition, equippedItems: 0);
            installed.SpawnDefinition(definition, equippedItems: 0);
            var nativeCgram = new SnesCgram();
            var installedCgram = new SnesCgram();
            const int frames = 160;
            for (int frame = 0; frame < frames; frame++)
            {
                native.Step(new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
                PaletteFxRoomLook.LayShownShades(native, nativeCgram, new ReferencePaletteFxColorSource(bus));
                installed.Step(new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
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
            native.SpawnDefinition(definition.DefinitionPointer, equippedItems: 0);
            installed.SpawnDefinition(definition.DefinitionPointer, equippedItems: 0);
        }

        var nativeCgram = new SnesCgram();
        var installedCgram = new SnesCgram();
        const int frames = 80;
        for (int frame = 0; frame < frames; frame++)
        {
            native.Step(new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
            PaletteFxRoomLook.LayShownShades(native, nativeCgram, new ReferencePaletteFxColorSource(bus));
            installed.Step(new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
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

        PaletteRgb5[] frame = document.NorfairForegroundPalette4[0];
        document.NorfairForegroundPalette4[0] = frame[..^1];
        Reject("room palette-FX rejects incomplete Norfair frame");
        document.NorfairForegroundPalette4[0] = frame;

        PaletteRgb5 first = document.NorfairForegroundPalette5[0][0];
        document.NorfairForegroundPalette5[0][0] = first with { Red = 32 };
        Reject("room palette-FX rejects non-RGB5 room color");
        document.NorfairForegroundPalette5[0][0] = first;

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

        PaletteRgb5[] darkLightningFrame = document.CrateriaUnusedDarkLightning[0];
        document.CrateriaUnusedDarkLightning[0] = darkLightningFrame[..^1];
        Reject("room palette-FX rejects incomplete Crateria dark-lightning frame");
        document.CrateriaUnusedDarkLightning[0] = darkLightningFrame;

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

        PaletteRgb5[] unusedFadeFrame = document.UnusedCinematicFade[0];
        document.UnusedCinematicFade[0] = unusedFadeFrame[..^1];
        Reject("room palette-FX rejects incomplete unused cinematic-fade frame");
        document.UnusedCinematicFade[0] = unusedFadeFrame;

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

        PaletteRgb5[][] pixel = document.CrateriaEscapeCreBlockPixel;
        document = document with { CrateriaEscapeCreBlockPixel = pixel[..^1] };
        Reject("room palette-FX rejects incomplete Crateria escape CRE pixel");
        document = document with { CrateriaEscapeCreBlockPixel = pixel };

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

    /// <summary>Runs one handler pass of <paramref name="fx"/> as <paramref name="shape"/>'s family plays: on a frontend screen or in gameplay.</summary>
    private static void StepFamily(RoomPaletteFxSystem fx, PaletteFxFamilyShape shape, PaletteFxConditions conditions)
    {
        if (shape.OnScreen)
            fx.StepScreen(conditions);
        else
            fx.Step(conditions);
    }
}
