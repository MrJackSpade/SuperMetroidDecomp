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
        VerifyRoomPaletteFxPresentationValidation(extracted);
        Console.WriteLine(
            "  Room palette presentation: 790 editable environmental colors match ROM; " +
            "fourteen installed programs match native execution without color-source reads.");
    }

    private static void VerifyInstalledPaletteFxFamily(
        ISnesAddressSpace bus,
        RoomPaletteFxPresentation presentation,
        string description,
        int frameCount,
        int colorsPerFrame,
        Func<int, int, ushort> colorPointer,
        IReadOnlyList<ushort> definitions,
        int framesToRun)
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
                native.Step(bus, nativeCgram, 0, 0, false, false);
                installed.Step(guarded, installedCgram, 0, 0, false, false);
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

        string unknownField = Encoding.UTF8.GetString(extracted).Replace(
            "\"version\": 6",
            "\"version\": 6,\n  \"nativeAddress\": 9240718",
            StringComparison.Ordinal);
        AssertThrows<InvalidDataException>(
            () => RoomPaletteFxPresentation.Load(new MemoryStream(
                Encoding.UTF8.GetBytes(unknownField), writable: false)),
            "room palette-FX rejects native-address escape hatch");
    }

}
