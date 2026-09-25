using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rom;
using System.Text.Json;

internal static partial class Program
{
    private static void VerifyTitleGradientTables(SuperMetroidAddressSpace bus)
    {
        VerifyTitleGradientObjectEligibility();
        VerifyExtractedTitleGraphics(bus);
        VerifyExtractedTitlePalette(bus);
        VerifyExtractedTitleGradient(bus);
        // Independently transcribed boundaries from $8C:BC7D and $88:EB95.
        var lines = TitleGradient.Decode(bus, 0);
        AssertEqual(new TitleGradientLine(15, 15, 15, 0xa1), lines[0], "title gradient starts subtracting fifteen");
        AssertEqual(new TitleGradientLine(14, 14, 14, 0xa1), lines[4], "title gradient advances after four scanlines");
        AssertEqual(new TitleGradientLine(0, 0, 0, 0xa1), lines[121], "last subtractive title line");
        AssertEqual(new TitleGradientLine(0, 0, 0, 0x31), lines[122], "first additive title line");
        AssertEqual(new TitleGradientLine(0, 1, 1, 0x31), lines[132], "lower title cyan band begins");
        AssertEqual(new TitleGradientLine(0, 2, 2, 0x31), lines[144], "second lower cyan band begins");
        for (ushort zoom = 0; zoom < 256; zoom++)
        {
            var actual = TitleGradient.Decode(bus, zoom);
            var nibble = TitleGradient.Decode(bus, (ushort)(zoom & 0xf0));
            if (!actual.AsSpan().SequenceEqual(nibble))
                throw new InvalidDataException("Title gradient index used low zoom bits.");
            AssertEqual((byte)0xa1, actual[121].Control, "zoom preserves subtractive boundary");
            AssertEqual((byte)0x31, actual[122].Control, "zoom preserves additive boundary");
        }
        Console.WriteLine("  Title gradient: 256 zoom selections, native cyan bands and subtract/add boundary agree.");
    }

    private static void VerifyExtractedTitleGraphics(ISnesAddressSpace bus)
    {
        for (int address = TitleSequenceInstructionDefinitions.StartAddress;
            address < TitleSequenceInstructionDefinitions.EndAddress; address++)
            AssertEqual(bus.ReadByte(address),
                TitleSequenceInstructionDefinitions.ReadByte(address),
                $"compiled title-card byte ${address:X6}");
        AssertThrows<InvalidDataException>(() =>
            TitleSequenceInstructionDefinitions.ReadWord(
                TitleSequenceInstructionDefinitions.EndAddress),
            "installed title card reader rejects adjacent bank-$8B code");
        var cartridgeReads = new HashSet<int>();
        var tracingBus = new TitlePresentationReadBus(bus, cartridgeReads, forbidReads: false);
        IReadOnlyDictionary<string, byte[]> files =
            SuperMetroid.AssetExtraction.TitleGraphicsExtractor.Extract(tracingBus);
        TitleGraphicsPresentation presentation = TitleGraphicsPresentation.Load(
            new MemoryStream(files[TitleGraphicsFormat.Mode7TilesFile]),
            new MemoryStream(files[TitleGraphicsFormat.Mode7MapFile]),
            new MemoryStream(files[TitleGraphicsFormat.ObjectTilesFile]),
            new MemoryStream(files[TitleGraphicsFormat.BabyTilesFile]));
        AssertSource(TitleSequenceRomData.Assets.Mode7CharactersAddress,
            TitleSequenceRomData.Vram.Mode7CharacterByteCount, presentation.Mode7Characters,
            "title Mode 7 characters");
        AssertSource(TitleSequenceRomData.Assets.Mode7MapAddress,
            TitleSequenceRomData.Vram.Mode7MapByteCount, presentation.Mode7Map,
            "title Mode 7 map");
        AssertSource(TitleSequenceRomData.Assets.ObjectCharactersAddress,
            TitleSequenceRomData.Vram.ObjectCharacterByteCount, presentation.ObjectCharacters,
            "title OBJ characters");
        AssertSource(TitleSequenceRomData.Assets.BabyMetroidCharactersAddress,
            TitleSequenceRomData.Vram.BabyCharacterByteCount, presentation.BabyCharacters,
            "title Baby characters");

        // The installed title uses compiled card timing and sprite selectors alongside
        // editable bank-$8C OAM compositions. The original source addresses are all
        // blocked while the complete natural scene is compared with the ROM-backed path.
        var guardedBus = new TitlePresentationReadBus(bus, cartridgeReads, forbidReads: true);
        var stock = new TitleSequenceState(bus);
        var installed = new TitleSequenceState(guardedBus, titleGraphicsPresentation: presentation);
        for (int frame = 0; frame < 140; frame++)
        {
            stock.Step(0);
            installed.Step(0);
            if (!stock.Render().AsSpan().SequenceEqual(installed.Render()))
                throw new InvalidDataException($"Installed title graphics differ from stock at frame {frame}.");
        }
        bool reachedTitle = false;
        for (int frame = 140; frame < 1800; frame++)
        {
            stock.Step(0);
            installed.Step(0);
            Mode7ObjRenderSnapshot native = stock.CaptureRenderSnapshot();
            Mode7ObjRenderSnapshot extracted = installed.CaptureRenderSnapshot();
            if (stock.Phase != installed.Phase ||
                !native.Memory.Oam.SequenceEqual(extracted.Memory.Oam))
                throw new InvalidDataException($"Installed title sprite composition differs at frame {frame} ({stock.Phase}).");
            if (stock.Phase == TitleSequencePhase.TitleScreen)
            {
                reachedTitle = true;
                break;
            }
        }
        if (!reachedTitle)
            throw new InvalidDataException("Title sprite parity test never reached the logo/copyright scene.");
        if (guardedBus.ForbiddenReadAttempts != 0)
            throw new InvalidDataException("Production title reread compressed graphics or native spritemaps.");

        TitleMode7MapDocument spriteDocument = JsonSerializer.Deserialize<TitleMode7MapDocument>(
            files[TitleGraphicsFormat.Mode7MapFile], MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Extracted title map document is null.");
        AssertEqual(TitleGraphicsFormat.SpriteFrameCount, spriteDocument.Sprites.Length,
            "title extraction covers all cartridge-selected OBJ frames");
        ushort yearPointer = RomDataReader.ReadWordFixedBank(bus,
            TitleSequenceRomData.TextSequences.Year.InstructionAddress + 6);
        TitleSpriteFrame year = spriteDocument.Sprites.Single(frame => frame.Pointer == yearPointer);
        SpriteVisualPart originalPart = year.Parts[0];
        year.Parts[0] = originalPart with { OffsetX = originalPart.OffsetX + 8 };
        byte[] editedMap = JsonSerializer.SerializeToUtf8Bytes(spriteDocument, MapPresentationFormat.JsonOptions);
        TitleGraphicsPresentation editedPresentation = TitleGraphicsPresentation.Load(
            new MemoryStream(files[TitleGraphicsFormat.Mode7TilesFile]),
            new MemoryStream(editedMap),
            new MemoryStream(files[TitleGraphicsFormat.ObjectTilesFile]),
            new MemoryStream(files[TitleGraphicsFormat.BabyTilesFile]));
        var originalTitle = new TitleSequenceState(bus, titleGraphicsPresentation: presentation);
        var editedTitle = new TitleSequenceState(bus, titleGraphicsPresentation: editedPresentation);
        bool editVisible = false;
        bool pixelChanged = false;
        for (int frame = 0; frame < 100; frame++)
        {
            originalTitle.Step(0);
            editedTitle.Step(0);
            if (originalTitle.Phase != editedTitle.Phase)
                throw new InvalidDataException("Title sprite edit altered the native title timing.");
            if (!originalTitle.CaptureRenderSnapshot().Memory.Oam.SequenceEqual(
                    editedTitle.CaptureRenderSnapshot().Memory.Oam))
            {
                editVisible = true;
                pixelChanged |= !originalTitle.Render().AsSpan().SequenceEqual(editedTitle.Render());
            }
        }
        if (!editVisible)
            throw new InvalidDataException("Editing the Year composition did not alter production OAM.");
        if (!pixelChanged)
            throw new InvalidDataException("Editing the Year composition did not alter visible title pixels.");
        var reboundTitle = new TitleSequenceState(bus, titleGraphicsPresentation: presentation);
        for (int frame = 0; frame < 64; frame++) reboundTitle.Step(0);
        byte[] beforeRebind = reboundTitle.CaptureRenderSnapshot().Memory.Oam.ToArray();
        reboundTitle.BindTitleGraphics(editedPresentation);
        if (beforeRebind.AsSpan().SequenceEqual(reboundTitle.CaptureRenderSnapshot().Memory.Oam))
            throw new InvalidDataException("Rebinding a live title failed to apply current sprite composition.");
        reboundTitle.BindTitleGraphics(presentation);
        if (!beforeRebind.AsSpan().SequenceEqual(reboundTitle.CaptureRenderSnapshot().Memory.Oam))
            throw new InvalidDataException("Rebinding stock title composition failed to restore the frame.");
        year.Parts[0] = originalPart with { TileRow = TitleGraphicsFormat.ObjectTileRows };
        byte[] malformedMap = JsonSerializer.SerializeToUtf8Bytes(spriteDocument, MapPresentationFormat.JsonOptions);
        AssertThrows<InvalidDataException>(() => TitleGraphicsPresentation.Load(
            new MemoryStream(files[TitleGraphicsFormat.Mode7TilesFile]),
            new MemoryStream(malformedMap),
            new MemoryStream(files[TitleGraphicsFormat.ObjectTilesFile]),
            new MemoryStream(files[TitleGraphicsFormat.BabyTilesFile])),
            "title graphics reject an out-of-range OBJ region");

        IndexedPngImage mode7 = IndexedPng.Read(
            new MemoryStream(files[TitleGraphicsFormat.Mode7TilesFile]),
            TitleGraphicsFormat.Mode7Width,
            TitleGraphicsFormat.Mode7Height);
        using var wrong = new MemoryStream();
        IndexedPng.Write(wrong, 8, 8, new byte[64], mode7.Palette);
        wrong.Position = 0;
        AssertThrows<InvalidDataException>(() => TitleGraphicsPresentation.Load(
            wrong,
            new MemoryStream(files[TitleGraphicsFormat.Mode7MapFile]),
            new MemoryStream(files[TitleGraphicsFormat.ObjectTilesFile]),
            new MemoryStream(files[TitleGraphicsFormat.BabyTilesFile])),
            "title graphics reject wrong Mode 7 PNG dimensions");
        Console.WriteLine(
            $"  Title graphics presentation: four editable assets and all title OBJ frames match native OAM; " +
            $"production avoided {cartridgeReads.Count} cartridge-art bytes.");

        void AssertSource(int address, int count, ReadOnlySpan<byte> actual, string description)
        {
            byte[] source = RomDataReader.Decompress(bus, address);
            if (!source.AsSpan(0, count).SequenceEqual(actual))
                throw new InvalidDataException($"Extracted {description} differ from cartridge data.");
        }
    }

    private static void VerifyExtractedTitlePalette(ISnesAddressSpace bus)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.TitlePaletteExtractor.Extract(bus);
        TitlePalettePresentation presentation = TitlePalettePresentation.Load(
            new MemoryStream(extracted, writable: false));
        var expected = new SnesCgram();
        expected.LoadFromBus(bus, TitleSequenceRomData.Assets.PaletteAddress);
        if (!presentation.Colors.SequenceEqual(expected.Colors))
            throw new InvalidDataException("Extracted title palette differs from cartridge CGRAM data.");
        AssertEqual(TitleSequenceRomData.Palette.CopyrightWhite,
            presentation.SkipCopyrightWhite, "extracted title skip copyright white");
        AssertEqual(TitleSequenceRomData.Palette.CopyrightRed,
            presentation.SkipCopyrightRed, "extracted title skip copyright red");

        VerifyExtractedTitleAmbientPalette(bus, presentation);

        VerifyTitlePaletteValidation(extracted);
        var paletteAddresses = Enumerable.Range(
            TitleSequenceRomData.Assets.PaletteAddress,
            SnesCgram.ByteCount).ToHashSet();
        var guardedBus = new TitlePresentationReadBus(bus, paletteAddresses, forbidReads: true);
        var title = new TitleSequenceState(guardedBus, titlePalettePresentation: presentation);
        if (!title.PaletteColors.SequenceEqual(presentation.Colors) ||
            guardedBus.ForbiddenReadAttempts != 0)
        {
            throw new InvalidDataException(
                "Production title initialization did not use the installed palette exclusively.");
        }
        var nativeSkip = new TitleSequenceState(bus);
        var installedSkip = new TitleSequenceState(bus, titlePalettePresentation: presentation);
        nativeSkip.Step((ushort)SuperMetroid.Core.Input.SnesButton.Start);
        installedSkip.Step((ushort)SuperMetroid.Core.Input.SnesButton.Start);
        for (int frame = 0; frame < 18; frame++)
        {
            nativeSkip.Step(0);
            installedSkip.Step(0);
        }
        AssertEqual(nativeSkip.Phase, installedSkip.Phase, "title skip phase unaffected by installed colors");
        AssertEqual(nativeSkip.PaletteColors[TitleSequenceRomData.Palette.CopyrightWhiteIndex],
            installedSkip.PaletteColors[TitleSequenceRomData.Palette.CopyrightWhiteIndex],
            "installed title skip white matches native");
        AssertEqual(nativeSkip.PaletteColors[TitleSequenceRomData.Palette.CopyrightRedIndex],
            installedSkip.PaletteColors[TitleSequenceRomData.Palette.CopyrightRedIndex],
            "installed title skip red matches native");
        Console.WriteLine(
            $"  Title palette presentation: {SnesCgram.ColorCount} initial, 36 ambient, and two skip " +
            $"editable colors match ROM; production avoided {paletteAddresses.Count} " +
            "initial cartridge source bytes.");
    }

    private static void VerifyExtractedTitleAmbientPalette(
        ISnesAddressSpace bus,
        TitlePalettePresentation presentation)
    {
        var colorAddresses = new HashSet<int>();
        foreach (TitleScreenAmbientPaletteFxProgramDefinition definition in
                 TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.All)
        {
            for (int frame = 0; frame < definition.FrameCount; frame++)
            {
                for (int color = 0; color < definition.ColorsPerFrame; color++)
                {
                    ushort pointer = unchecked((ushort)(
                        definition.FramePointer(frame) + sizeof(ushort) +
                        color * sizeof(ushort)));
                    ushort expected = RomDataReader.ReadWordFixedBank(
                        bus,
                        RoomFxRomData.Banks.PaletteFx | pointer);
                    if (!presentation.TryReadColor(pointer, out ushort actual) ||
                        actual != expected)
                    {
                        throw new InvalidDataException(
                            $"Extracted title ambient color $8D:{pointer:X4} differs from cartridge data.");
                    }
                    colorAddresses.Add(RoomFxRomData.Banks.PaletteFx | pointer);
                    colorAddresses.Add(RoomFxRomData.Banks.PaletteFx |
                        unchecked((ushort)(pointer + 1)));
                }
            }
        }

        var guardedBus = new TitlePresentationReadBus(bus, colorAddresses, forbidReads: true);
        var native = new RoomPaletteFxSystem();
        var installed = new RoomPaletteFxSystem();
        installed.BindPresentationColors(presentation);
        foreach (TitleScreenAmbientPaletteFxProgramDefinition definition in
                 TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.All)
        {
            native.SpawnDefinition(bus, definition.DefinitionPointer, 0);
            installed.SpawnDefinition(guardedBus, definition.DefinitionPointer, 0);
        }

        var nativeCgram = new SnesCgram();
        var installedCgram = new SnesCgram();
        int frames = TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.All.Max(
            definition => definition.CycleFrames) * 2;
        for (int frame = 0; frame < frames; frame++)
        {
            native.Step(bus, nativeCgram, 0, 0, false, false);
            installed.Step(guardedBus, installedCgram, 0, 0, false, false);
            if (!nativeCgram.Colors.SequenceEqual(installedCgram.Colors))
            {
                throw new InvalidDataException(
                    $"Installed title ambient palette diverged from native output on frame {frame}.");
            }
        }
        AssertEqual(0, guardedBus.ForbiddenReadAttempts,
            "installed title ambient loops avoid cartridge color reads");
    }

    private static void VerifyTitlePaletteValidation(byte[] extracted)
    {
        TitlePaletteDocument document = JsonSerializer.Deserialize<TitlePaletteDocument>(
            extracted,
            MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Extracted title palette document is null.");
        byte[] Json() => JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        void Reject(string description) => AssertThrows<InvalidDataException>(
            () => TitlePalettePresentation.Load(new MemoryStream(Json(), writable: false)),
            description);

        int version = document.Version;
        document = document with { Version = version + 1 };
        Reject("title palette rejects unsupported schema version");
        document = document with { Version = version };

        PaletteRgb5[] colors = document.Colors;
        document = document with { Colors = colors[..^1] };
        Reject("title palette rejects incomplete color set");
        document = document with { Colors = colors };

        PaletteRgb5 first = colors[0];
        colors[0] = first with { Blue = 32 };
        Reject("title palette rejects non-RGB5 color");
        colors[0] = first;

        PaletteRgb5[][] tube = document.BabyMetroidTubeLight;
        document = document with { BabyMetroidTubeLight = tube[..^1] };
        Reject("title palette rejects incomplete tube-light animation");
        document = document with { BabyMetroidTubeLight = tube };

        PaletteRgb5[] firstDisplayFrame = document.FlickeringDisplays[0];
        document.FlickeringDisplays[0] = firstDisplayFrame[..^1];
        Reject("title palette rejects incomplete display animation frame");
        document.FlickeringDisplays[0] = firstDisplayFrame;

        PaletteRgb5 firstAmbient = tube[0][0];
        tube[0][0] = firstAmbient with { Green = 32 };
        Reject("title palette rejects non-RGB5 ambient color");
        tube[0][0] = firstAmbient;

        PaletteRgb5 skipWhite = document.SkipCopyrightWhite;
        document = document with { SkipCopyrightWhite = skipWhite with { Red = 32 } };
        Reject("title palette rejects out-of-range skip copyright white");
        document = document with { SkipCopyrightWhite = skipWhite };
        PaletteRgb5 skipRed = document.SkipCopyrightRed;
        document = document with { SkipCopyrightRed = skipRed with { Blue = 32 } };
        Reject("title palette rejects out-of-range skip copyright red");
        document = document with { SkipCopyrightRed = skipRed };

        string unknownField = System.Text.Encoding.UTF8.GetString(extracted)
            .Replace("\"version\": 3", "\"version\": 3,\n  \"nativeAddress\": 9232873", StringComparison.Ordinal);
        AssertThrows<InvalidDataException>(
            () => TitlePalettePresentation.Load(new MemoryStream(
                System.Text.Encoding.UTF8.GetBytes(unknownField), writable: false)),
            "title palette rejects native-address escape hatch");
    }

    private static void VerifyExtractedTitleGradient(ISnesAddressSpace bus)
    {
        var cartridgeReads = new HashSet<int>();
        var tracingBus = new TitlePresentationReadBus(bus, cartridgeReads, forbidReads: false);
        for (ushort variant = 0; variant < TitleGradientFormat.VariantCount; variant++)
            _ = TitleGradient.Decode(tracingBus, checked((ushort)(variant << 4)));

        byte[] extracted = SuperMetroid.AssetExtraction.TitleGradientExtractor.Extract(bus);
        TitleGradientPresentation presentation = TitleGradientPresentation.Load(
            new MemoryStream(extracted, writable: false));
        for (ushort zoom = 0; zoom < 256; zoom++)
        {
            if (!presentation.Resolve(zoom).SequenceEqual(TitleGradient.Decode(bus, zoom)))
                throw new InvalidDataException($"Extracted title gradient differs at zoom ${zoom:X2}.");
        }
        VerifyTitleGradientValidation(extracted);

        // The rest of title setup remains cartridge-backed presentation for later #549
        // slices. Forbid only the complete gradient source closure discovered above, then
        // drive the real title owner through Start's skip fade into a gradient frame.
        var guardedBus = new TitlePresentationReadBus(bus, cartridgeReads, forbidReads: true);
        (TitleGradientLine[] rendered, ushort selectedZoom) = CaptureInstalledTitleGradient(guardedBus, presentation);
        if (!rendered.AsSpan().SequenceEqual(presentation.Resolve(selectedZoom)) ||
            guardedBus.ForbiddenReadAttempts != 0)
        {
            throw new InvalidDataException(
                "Production title rendering did not use the installed gradient presentation exclusively.");
        }
        Console.WriteLine(
            $"  Title gradient presentation: {TitleGradientFormat.VariantCount} editable variants match ROM; " +
            $"production avoided {cartridgeReads.Count} cartridge source bytes.");
    }

    private static void VerifyTitleGradientValidation(byte[] extracted)
    {
        TitleGradientDocument document = JsonSerializer.Deserialize<TitleGradientDocument>(
            extracted,
            MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Extracted title gradient document is null.");
        byte[] Json() => JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        void Reject(string description) => AssertThrows<InvalidDataException>(
            () => TitleGradientPresentation.Load(new MemoryStream(Json(), writable: false)),
            description);

        int version = document.Version;
        document = document with { Version = version + 1 };
        Reject("title gradient rejects unsupported schema version");
        document = document with { Version = version };

        TitleGradientVariant first = document.Variants[0];
        document.Variants[0] = first with { ZoomHighNibble = 1 };
        Reject("title gradient rejects mismatched variant identity");
        document.Variants[0] = first with { Lines = first.Lines[..^1] };
        Reject("title gradient rejects incomplete scanline set");
        document.Variants[0] = first;

        TitleGradientScanline line = first.Lines[0];
        first.Lines[0] = line with { Red = 32 };
        Reject("title gradient rejects non-RGB5 color");
        first.Lines[0] = line with { ColorMathControl = 256 };
        Reject("title gradient rejects non-byte color math control");
        first.Lines[0] = line;

        string unknownField = System.Text.Encoding.UTF8.GetString(extracted)
            .Replace("\"version\": 1", "\"version\": 1,\n  \"nativeAddress\": 9192685", StringComparison.Ordinal);
        AssertThrows<InvalidDataException>(
            () => TitleGradientPresentation.Load(new MemoryStream(
                System.Text.Encoding.UTF8.GetBytes(unknownField), writable: false)),
            "title gradient rejects native-address escape hatch");
    }

    private static (TitleGradientLine[] Lines, ushort Zoom) CaptureInstalledTitleGradient(
        ISnesAddressSpace bus,
        TitleGradientPresentation presentation)
    {
        var title = new TitleSequenceState(bus, titleGradientPresentation: presentation);
        title.Step((ushort)SuperMetroid.Core.Input.SnesButton.Start);
        for (int frame = 0; frame < 16; frame++)
        {
            title.Step(0);
            Mode7ObjRenderSnapshot snapshot = title.CaptureRenderSnapshot();
            if (!snapshot.Gradient.IsEmpty)
                return (snapshot.Gradient.ToArray(), title.Mode7MatrixScale);
        }
        throw new InvalidDataException("Production title rendering never reached a gradient frame.");
    }

    private static void VerifyTitleGradientObjectEligibility()
    {
        var oam = new OamBuffer();
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        byte[] tile = new byte[32];
        for (int row = 0; row < 8; row++) tile[row * 2] = 255;
        vram.LoadBytes(0, tile);
        var pixels = new Rgba32[256 * 224];
        var priorities = new byte[pixels.Length];
        var palettes = new byte[pixels.Length];
        var color = new Rgba32(0, 0, 0, 255);
        var additive = new TitleGradientLine(0, 1, 1, 0x31);
        for (byte palette = 0; palette < 8; palette++)
        {
            oam.BeginFrame();
            oam.AddRawSmallSprite(0, 0, (ushort)(palette << 9));
            oam.AddRawSmallSprite(0, 0, (ushort)((7 - palette) << 9));
            oam.FinalizeFrame();
            SnesObjRenderer.RenderResolved(oam, vram, cgram, 0, pixels, priorities, palettes: palettes);
            AssertEqual(palette, palettes[0], "winning OBJ palette survives equal-color overlap");
            AssertEqual(byte.MaxValue, palettes[8], "transparent pixel has no palette owner");
            AssertEqual(palette < 4 ? color : new Rgba32(0, 8, 8, 255),
                TitleGradientColorMath.Apply(color, additive, palettes[0]), "title OBJ palette eligibility");
            AssertEqual(color, TitleGradientColorMath.Apply(color, new(15, 15, 15, 0xa1), palette),
                "upper title subtraction excludes every OBJ palette");
        }
        AssertEqual(new Rgba32(247, 247, 247, 255),
            TitleGradientColorMath.Apply(new(255, 255, 255, 255), new(1, 1, 1, 0xa1)), "native five-bit subtraction");
    }

    private sealed class TitlePresentationReadBus(
        ISnesAddressSpace source,
        HashSet<int> addresses,
        bool forbidReads) : ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (forbidReads && addresses.Contains(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production title gradient reread cartridge byte ${address >> 16:X2}:{address & 0xffff:X4}.");
            }
            if (!forbidReads)
                addresses.Add(address);
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
