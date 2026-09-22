using System.Text;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyExtractedRoomPaletteFxPresentation(
        SuperMetroidAddressSpace bus)
    {
        byte[] extracted =
            SuperMetroid.AssetExtraction.RoomPaletteFxPresentationExtractor.Extract(bus);
        RoomPaletteFxPresentation presentation = RoomPaletteFxPresentation.Load(
            new MemoryStream(extracted, writable: false));
        var colorAddresses = new HashSet<int>();
        foreach (NorfairEnvironmentalPaletteFxProgramDefinition definition in
                 NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.All)
        {
            for (int frame = 0;
                 frame < NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.FrameCount;
                 frame++)
            {
                for (int color = 0;
                     color < NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.ColorsPerFrame;
                     color++)
                {
                    ushort pointer = definition.ColorPointer(frame, color);
                    ushort expected = RomDataReader.ReadWordFixedBank(
                        bus,
                        RoomFxRomData.Banks.PaletteFx | pointer);
                    AssertTrue(presentation.TryReadColor(pointer, out ushort actual),
                        $"installed Norfair color resolves $8D:{pointer:X4}");
                    AssertEqual(expected, actual,
                        $"installed Norfair color matches cartridge $8D:{pointer:X4}");
                    colorAddresses.Add(RoomFxRomData.Banks.PaletteFx | pointer);
                    colorAddresses.Add(RoomFxRomData.Banks.PaletteFx |
                        unchecked((ushort)(pointer + 1)));
                }
            }
        }
        AssertEqual(640, colorAddresses.Count,
            "Norfair presentation owns all 320 BGR555 source words");

        var guarded = new TitlePresentationReadBus(bus, colorAddresses, forbidReads: true);
        var native = new RoomPaletteFxSystem();
        var installed = new RoomPaletteFxSystem();
        installed.BindPresentationColors(presentation);
        foreach (NorfairEnvironmentalPaletteFxProgramDefinition definition in
                 NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.All)
        {
            native.SpawnDefinition(bus, definition.DefinitionPointer, equippedItems: 0);
            installed.SpawnDefinition(guarded, definition.DefinitionPointer, equippedItems: 0);
        }

        var nativeCgram = new SnesCgram();
        var installedCgram = new SnesCgram();
        int frames = NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.CycleFrames * 2;
        for (int frame = 0; frame < frames; frame++)
        {
            native.Step(bus, nativeCgram, 0, 0, false, false);
            installed.Step(guarded, installedCgram, 0, 0, false, false);
            AssertTrue(nativeCgram.Colors.SequenceEqual(installedCgram.Colors),
                $"installed Norfair palette equals native output on frame {frame}");
            AssertEqual(native.SamusInHeatPaletteIndex, installed.SamusInHeatPaletteIndex,
                $"installed Norfair heat phase equals native output on frame {frame}");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed Norfair palette loops avoid cartridge color reads");

        VerifyExtractedMaridiaPaletteFxPresentation(bus, presentation);
        VerifyExtractedWreckedShipPaletteFxPresentation(bus, presentation);
        VerifyInstalledPaletteFxFamily(
            bus,
            presentation,
            "Red Brinstar glow",
            RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.FrameCount,
            RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.ColorPointer,
            [RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.DefinitionPointer],
            RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.CycleFrames * 2);
        VerifyInstalledPaletteFxFamily(
            bus,
            presentation,
            "Tourian glow",
            TourianGlowPaletteFxProgramMechanicsDefinitions.FrameCount,
            TourianGlowPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            TourianGlowPaletteFxProgramMechanicsDefinitions.ColorPointer,
            [
                TourianGlowPaletteFxProgramMechanicsDefinitions.LiveDefinitionPointer,
                TourianGlowPaletteFxProgramMechanicsDefinitions.CloneDefinitionPointer,
            ],
            TourianGlowPaletteFxProgramMechanicsDefinitions.CycleFrames * 2);
        foreach (BrinstarBlueSporePaletteFxProgramDefinition definition in
                 BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(
                bus,
                presentation,
                $"Brinstar blue spores ({definition.Owner})",
                BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.FrameCount,
                BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer,
                [definition.DefinitionPointer],
                BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.CycleFrames * 2);
        }
        foreach (TorizoBellyPaletteFxProgramDefinition definition in
                 TorizoBellyPaletteFxProgramMechanicsDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(
                bus,
                presentation,
                $"{definition.Owner} belly",
                TorizoBellyPaletteFxProgramMechanicsDefinitions.FrameCount,
                TorizoBellyPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer,
                [definition.DefinitionPointer],
                TorizoBellyPaletteFxProgramMechanicsDefinitions.CycleFrames * 2);
        }
        VerifyInstalledPaletteFxFamily(
            bus,
            presentation,
            "Tourian statue grey-out",
            TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FrameCount,
            TourianStatueGreyPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            TourianStatueGreyPaletteFxProgramMechanicsDefinitions.ColorPointer,
            TourianStatueGreyPaletteFxProgramMechanicsDefinitions.All
                .Select(definition => definition.DefinitionPointer)
                .ToArray(),
            TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FramesThroughDeletion);
        foreach (CrateriaLightningPaletteFxProgramDefinition definition in
                 CrateriaLightningPaletteFxProgramMechanicsDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(
                bus,
                presentation,
                $"Crateria {definition.Owner}",
                definition.Frames.Count,
                definition.ColorsPerFrame,
                definition.ColorPointer,
                [definition.DefinitionPointer],
                definition.CycleFrames * 2,
                CrateriaLightningPaletteFxProgramMechanicsDefinitions.VerticalSwitchSamusY);
        }
        VerifyInstalledPaletteFxFamily(
            bus,
            presentation,
            "Ceres gunship-engine lights",
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions.GunshipEngineFrameCount,
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .GunshipEngineColorsPerFrame,
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .GunshipEngineColorPointer,
            [CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .GunshipEngineDefinitionPointer],
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .GunshipEngineCycleFrames * 2);
        VerifyInstalledPaletteFxFamily(
            bus,
            presentation,
            "Ceres navigation lights",
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions.NavigationLightsFrameCount,
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .NavigationLightsColorsPerFrame,
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .NavigationLightsColorPointer,
            [
                CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                    .SpriteNavigationLightsDefinitionPointer,
                CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                    .BackgroundNavigationLightsDefinitionPointer,
            ],
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .NavigationLightsCycleFrames * 2);
        foreach (PlanetZebesTextPaletteFxProgramDefinition definition in
                 PlanetZebesTextPaletteFxProgramMechanicsDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(
                bus,
                presentation,
                $"PLANET ZEBES {definition.Owner}",
                PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount,
                PlanetZebesTextPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer,
                [definition.DefinitionPointer],
                PlanetZebesTextPaletteFxProgramMechanicsDefinitions.CycleFrames + 1);
        }
        foreach (CinematicGlowPaletteFxProgramDefinition definition in
                 CinematicGlowPaletteFxProgramMechanicsDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(
                bus,
                presentation,
                $"Cinematic glow {definition.Owner}",
                CinematicGlowPaletteFxProgramMechanicsDefinitions.FrameCount,
                definition.ColorsPerFrame,
                definition.ColorPointer,
                [definition.DefinitionPointer],
                definition.CycleFrames * 2);
        }
        VerifyInstalledPaletteFxFamily(
            bus,
            presentation,
            "exploding Zebes fade",
            ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.FrameCount,
            ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.ColorPointer,
            [ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.DefinitionPointer],
            ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.CycleFrames + 1);
        VerifyInstalledPaletteFxFamily(
            bus,
            presentation,
            "unused cinematic fade",
            UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.FrameCount,
            UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.ColorPointer,
            [UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.DefinitionPointer],
            UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.CycleFrames + 1);
        VerifyInstalledPaletteFxFamily(
            bus,
            presentation,
            "title-logo fade",
            TitleLogoFadePaletteFxProgramMechanicsDefinitions.FrameCount,
            TitleLogoFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            TitleLogoFadePaletteFxProgramMechanicsDefinitions.ColorPointer,
            [TitleLogoFadePaletteFxProgramMechanicsDefinitions.DefinitionPointer],
            TitleLogoFadePaletteFxProgramMechanicsDefinitions.CycleFrames + 1);
        VerifyInstalledPaletteFxFamily(
            bus,
            presentation,
            "Nintendo shared fade",
            NintendoLogoFadePaletteFxProgramMechanicsDefinitions.FrameCount,
            NintendoLogoFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            NintendoLogoFadePaletteFxProgramMechanicsDefinitions.ColorPointer,
            NintendoLogoFadePaletteFxProgramMechanicsDefinitions.All
                .Select(definition => definition.DefinitionPointer)
                .ToArray(),
            NintendoLogoFadePaletteFxProgramMechanicsDefinitions.CycleFrames + 1);
        VerifyInstalledPaletteFxFamily(
            bus, presentation, "Zebes explosion foreground",
            ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.FrameCount,
            ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.ColorPointer,
            [ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.DefinitionPointer],
            ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.CycleFrames + 1);
        VerifyInstalledPaletteFxFamily(
            bus, presentation, "Zebes explosion finale",
            ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.FrameCount,
            ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.ColorPointer,
            [ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.DefinitionPointer],
            ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.CycleFrames + 1);
        VerifyInstalledPaletteFxFamily(
            bus, presentation, "Zebes explosion shared whiteout",
            ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.FrameCount,
            ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.ColorPointer,
            ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.All
                .Select(definition => definition.DefinitionPointer).ToArray(),
            ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.CycleFrames + 1);
        foreach (ZebesExplosionAmbientPaletteFxProgramDefinition definition in
                 ZebesExplosionAmbientPaletteFxProgramMechanicsDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(
                bus, presentation, $"Zebes explosion {definition.Owner}",
                definition.FrameCount, definition.ColorsPerFrame, definition.ColorPointer,
                [definition.DefinitionPointer], definition.CycleFrames * 2);
        }
        foreach (ZebesExplosionLayerFadePaletteFxProgramDefinition definition in
                 ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(
                bus, presentation, $"Zebes explosion {definition.Owner}",
                ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.FrameCount,
                ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer, [definition.DefinitionPointer],
                definition.CycleFrames + 1);
        }
        VerifyInstalledPaletteFxFamily(
            bus, presentation, "Zebes explosion gunship",
            ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.FrameCount,
            ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorPointer,
            [ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.DefinitionPointer],
            ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.CycleFrames + 1);
        foreach (SamusLoadingSuitPaletteFxProgramDefinition definition in
                 SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(
                bus, presentation, $"Samus loading {definition.Owner}",
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameCount,
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer, [definition.DefinitionPointer],
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.CycleFrames + 1);
        }
        VerifyInstalledPaletteFxFamily(
            bus, presentation, "post-credits icon glare",
            PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.FrameCount,
            PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.ColorPointer,
            [PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.DefinitionPointer],
            PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.CycleFrames + 1);
        foreach (TourianEscapeRedFlashPaletteFxProgramDefinition definition in
                 TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(
                bus, presentation, $"Tourian escape {definition.Owner}",
                TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount,
                definition.ColorsPerFrame, definition.ColorPointer,
                [definition.DefinitionPointer], definition.CycleFrames * 2);
        }
        VerifyInstalledPaletteFxFamily(
            bus, presentation, "Tourian escape shared red flash",
            TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount,
            TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.ColorPointer,
            TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.All
                .Select(definition => definition.DefinitionPointer).ToArray(),
            TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.CycleFrames * 2);
        VerifyInstalledPaletteFxFamily(
            bus, presentation, "old Tourian escape red flash",
            OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount,
            OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorPointer,
            [OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.DefinitionPointer],
            OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.CycleFrames * 2);
        foreach (OldTourianEscapeAccentPaletteFxProgramDefinition definition in
                 OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(
                bus, presentation, $"old Tourian escape {definition.Owner}",
                OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.FrameCount,
                OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer, [definition.DefinitionPointer],
                OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.CycleFrames * 2);
        }
        VerifyInstalledPaletteFxFamily(
            bus, presentation, "upper Crateria escape red flash",
            UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount,
            UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorPointer,
            [UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.DefinitionPointer],
            UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.CycleFrames * 2);
        foreach (CrateriaEscapeLightningPaletteFxProgramDefinition definition in
                 CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.All)
        {
            VerifyInstalledPaletteFxFamily(
                bus, presentation, $"Crateria escape {definition.Owner}",
                CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.FrameCount,
                definition.ColorsPerFrame, definition.ColorPointer,
                [definition.DefinitionPointer],
                CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.CycleFrames * 2);
        }
        VerifyRoomPaletteFxPresentationValidation(extracted);
        Console.WriteLine(
            "  Room palette presentation: 4348 editable palette colors match ROM; " +
            "fifty-one installed programs match native execution without color-source reads.");
    }

    private static void VerifyInstalledPaletteFxFamily(
        ISnesAddressSpace bus,
        RoomPaletteFxPresentation presentation,
        string description,
        int frameCount,
        int colorsPerFrame,
        Func<int, int, ushort> colorPointer,
        IReadOnlyList<ushort> definitions,
        int framesToRun,
        ushort samusY = 0)
    {
        var colorAddresses = new HashSet<int>();
        for (int frame = 0; frame < frameCount; frame++)
        for (int color = 0; color < colorsPerFrame; color++)
        {
            ushort pointer = colorPointer(frame, color);
            ushort expected = RomDataReader.ReadWordFixedBank(
                bus,
                RoomFxRomData.Banks.PaletteFx | pointer);
            AssertTrue(presentation.TryReadColor(pointer, out ushort actual),
                $"installed {description} color resolves $8D:{pointer:X4}");
            AssertEqual(expected, actual,
                $"installed {description} color matches cartridge $8D:{pointer:X4}");
            colorAddresses.Add(RoomFxRomData.Banks.PaletteFx | pointer);
            colorAddresses.Add(RoomFxRomData.Banks.PaletteFx |
                unchecked((ushort)(pointer + 1)));
        }
        AssertEqual(frameCount * colorsPerFrame * sizeof(ushort), colorAddresses.Count,
            $"{description} presentation owns every BGR555 source byte");

        foreach (ushort definition in definitions)
        {
            var guarded = new TitlePresentationReadBus(bus, colorAddresses, forbidReads: true);
            var native = new RoomPaletteFxSystem();
            var installed = new RoomPaletteFxSystem();
            installed.BindPresentationColors(presentation);
            native.SpawnDefinition(bus, definition, equippedItems: 0);
            installed.SpawnDefinition(guarded, definition, equippedItems: 0);
            var nativeCgram = new SnesCgram();
            var installedCgram = new SnesCgram();
            for (int frame = 0; frame < framesToRun; frame++)
            {
                native.Step(bus, nativeCgram, samusY, 0, false, false);
                installed.Step(guarded, installedCgram, samusY, 0, false, false);
                AssertTrue(nativeCgram.Colors.SequenceEqual(installedCgram.Colors),
                    $"installed {description} definition ${definition:X4} equals native " +
                    $"output on frame {frame}");
            }
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"installed {description} definition ${definition:X4} avoids color reads");
        }
    }

    private static void VerifyExtractedWreckedShipPaletteFxPresentation(
        ISnesAddressSpace bus,
        RoomPaletteFxPresentation presentation)
    {
        var colorAddresses = new HashSet<int>();
        for (int frame = 0;
             frame < WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        for (int color = 0;
             color < WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.ColorsPerFrame;
             color++)
        {
            ushort pointer =
                WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.ColorPointer(
                    frame, color);
            ushort expected = RomDataReader.ReadWordFixedBank(
                bus,
                RoomFxRomData.Banks.PaletteFx | pointer);
            AssertTrue(presentation.TryReadColor(pointer, out ushort actual),
                $"installed Wrecked Ship color resolves $8D:{pointer:X4}");
            AssertEqual(expected, actual,
                $"installed Wrecked Ship color matches cartridge $8D:{pointer:X4}");
            colorAddresses.Add(RoomFxRomData.Banks.PaletteFx | pointer);
            colorAddresses.Add(RoomFxRomData.Banks.PaletteFx |
                unchecked((ushort)(pointer + 1)));
        }
        AssertEqual(32, colorAddresses.Count,
            "Wrecked Ship presentation owns all 16 BGR555 source words");

        foreach (ushort definition in new ushort[]
                 {
                     WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.PoweredDefinition,
                     WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.PoweredDefinitionAlternate,
                 })
        {
            var guarded = new TitlePresentationReadBus(bus, colorAddresses, forbidReads: true);
            var native = new RoomPaletteFxSystem();
            var installed = new RoomPaletteFxSystem();
            installed.BindPresentationColors(presentation);
            native.SpawnDefinition(bus, definition, equippedItems: 0);
            installed.SpawnDefinition(guarded, definition, equippedItems: 0);
            var nativeCgram = new SnesCgram();
            var installedCgram = new SnesCgram();
            const int frames = 160;
            for (int frame = 0; frame < frames; frame++)
            {
                native.Step(bus, nativeCgram, 0, 0, false, false);
                installed.Step(guarded, installedCgram, 0, 0, false, false);
                AssertTrue(nativeCgram.Colors.SequenceEqual(installedCgram.Colors),
                    $"installed Wrecked Ship definition ${definition:X4} equals native " +
                    $"output on frame {frame}");
            }
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"installed Wrecked Ship definition ${definition:X4} avoids color reads");
        }
    }

    private static void VerifyExtractedMaridiaPaletteFxPresentation(
        ISnesAddressSpace bus,
        RoomPaletteFxPresentation presentation)
    {
        var colorAddresses = new HashSet<int>();
        foreach (MaridiaEnvironmentalPaletteFxProgramDefinition definition in
                 MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.All)
        {
            for (int frame = 0; frame < definition.FrameCount; frame++)
            for (int color = 0; color < definition.ColorsPerFrame; color++)
            {
                ushort pointer = definition.ColorPointer(frame, color);
                ushort expected = RomDataReader.ReadWordFixedBank(
                    bus,
                    RoomFxRomData.Banks.PaletteFx | pointer);
                AssertTrue(presentation.TryReadColor(pointer, out ushort actual),
                    $"installed Maridia color resolves $8D:{pointer:X4}");
                AssertEqual(expected, actual,
                    $"installed Maridia color matches cartridge $8D:{pointer:X4}");
                colorAddresses.Add(RoomFxRomData.Banks.PaletteFx | pointer);
                colorAddresses.Add(RoomFxRomData.Banks.PaletteFx |
                    unchecked((ushort)(pointer + 1)));
            }
        }
        AssertEqual(224, colorAddresses.Count,
            "Maridia presentation owns all 112 BGR555 source words");

        var guarded = new TitlePresentationReadBus(bus, colorAddresses, forbidReads: true);
        var native = new RoomPaletteFxSystem();
        var installed = new RoomPaletteFxSystem();
        installed.BindPresentationColors(presentation);
        foreach (MaridiaEnvironmentalPaletteFxProgramDefinition definition in
                 MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.All)
        {
            native.SpawnDefinition(bus, definition.DefinitionPointer, equippedItems: 0);
            installed.SpawnDefinition(guarded, definition.DefinitionPointer, equippedItems: 0);
        }

        var nativeCgram = new SnesCgram();
        var installedCgram = new SnesCgram();
        const int frames = 80;
        for (int frame = 0; frame < frames; frame++)
        {
            native.Step(bus, nativeCgram, 0, 0, false, false);
            installed.Step(guarded, installedCgram, 0, 0, false, false);
            AssertTrue(nativeCgram.Colors.SequenceEqual(installedCgram.Colors),
                $"installed Maridia palette equals native output on frame {frame}");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed Maridia palette loops avoid cartridge color reads");
    }

    private static void VerifyRoomPaletteFxPresentationValidation(byte[] extracted)
    {
        RoomPaletteFxPresentationDocument document =
            JsonSerializer.Deserialize<RoomPaletteFxPresentationDocument>(
                extracted,
                MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Extracted room palette-FX document is null.");
        byte[] Json() => JsonSerializer.SerializeToUtf8Bytes(
            document,
            MapPresentationFormat.JsonOptions);
        void Reject(string description) => AssertThrows<InvalidDataException>(
            () => RoomPaletteFxPresentation.Load(new MemoryStream(Json(), writable: false)),
            description);

        int version = document.Version;
        document = document with { Version = version + 1 };
        Reject("room palette-FX rejects unsupported schema version");
        document = document with { Version = version };

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

        string unknownField = Encoding.UTF8.GetString(extracted).Replace(
            "\"version\": 16",
            "\"version\": 16,\n  \"nativeAddress\": 9240718",
            StringComparison.Ordinal);
        AssertThrows<InvalidDataException>(
            () => RoomPaletteFxPresentation.Load(new MemoryStream(
                Encoding.UTF8.GetBytes(unknownField), writable: false)),
            "room palette-FX rejects native-address escape hatch");
    }

}
