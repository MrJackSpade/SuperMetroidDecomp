using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.Title.TitleSequenceRomDataVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;
using System.Text.Json;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationOracles;
namespace SuperMetroid.Verification.Frontend.Title;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

/// <summary>Checks extracted title graphics, palettes, and HDMA gradient through installed presentation and retail oracles.</summary>
internal static class TitleGradientVerification
{
    /// <summary>The title's upper CGADSUB setting: subtract the fixed color from BG1 and the backdrop.</summary>
    private const ColorBlend SubtractFromBackground = ColorBlend.Subtract | ColorBlend.Backdrop | ColorBlend.Bg1;
    /// <summary>The title's lower CGADSUB setting: add the fixed color to BG1, the backdrop and the OBJs.</summary>
    private const ColorBlend AddToBackgroundAndObjects = ColorBlend.Backdrop | ColorBlend.Sprites | ColorBlend.Bg1;

    /// <summary>Creates the production title owner with selected installed graphics, palette, and gradient overrides.</summary>
    private static TitleSequenceState CreateTitlePresentationFixture(TitleGradientPresentation? titleGradientPresentation = null,
        TitlePalettePresentation? titlePalettePresentation = null,
        TitleGraphicsPresentation? titleGraphicsPresentation = null)
 =>
        InstalledFixtures.CreateTitle(titleGradientPresentation: titleGradientPresentation,
            titlePalettePresentation: titlePalettePresentation, titleGraphicsPresentation: titleGraphicsPresentation);
    /// <summary>Checks native gradient band boundaries and all 256 zoom selections after artwork and schema suites.</summary>
    internal static void VerifyTitleGradientTables(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {
        Suite(nameof(VerifyExtractedTitleGraphics), () => VerifyExtractedTitleGraphics(bus));
        Suite(nameof(VerifyExtractedTitlePalette), () => VerifyExtractedTitlePalette(bus));
        Suite(nameof(VerifyExtractedTitleGradient), () => VerifyExtractedTitleGradient(bus));
        // Independently transcribed boundaries from $8C:BC7D and $88:EB95.
        var lines = TitleGradientDecoder.Decode(bus, 0);
        AssertEqual(new TitleGradientLine(15, 15, 15, SubtractFromBackground), lines[0], "title gradient starts subtracting fifteen");
        AssertEqual(new TitleGradientLine(14, 14, 14, SubtractFromBackground), lines[4], "title gradient advances after four scanlines");
        AssertEqual(new TitleGradientLine(0, 0, 0, SubtractFromBackground), lines[121], "last subtractive title line");
        AssertEqual(new TitleGradientLine(0, 0, 0, AddToBackgroundAndObjects), lines[122], "first additive title line");
        AssertEqual(new TitleGradientLine(0, 1, 1, AddToBackgroundAndObjects), lines[132], "lower title cyan band begins");
        AssertEqual(new TitleGradientLine(0, 2, 2, AddToBackgroundAndObjects), lines[144], "second lower cyan band begins");
        for (ushort zoom = 0; zoom < 256; zoom++)
        {
            var actual = TitleGradientDecoder.Decode(bus, zoom);
            var nibble = TitleGradientDecoder.Decode(bus, (ushort)(zoom & 0xf0));
            if (!actual.SequenceEqual(nibble))
                throw new InvalidDataException("Title gradient index used low zoom bits.");
            AssertEqual(SubtractFromBackground, actual[121].Control, "zoom preserves subtractive boundary");
            AssertEqual(AddToBackgroundAndObjects, actual[122].Control, "zoom preserves additive boundary");
        }
        Console.WriteLine("  Title gradient: 256 zoom selections, native cyan bands and subtract/add boundary agree.");
    }

    /// <summary>Compares extracted title character and map transfers with their retail source streams and live title rendering.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for title graphics streams and the year text pointer")]
    private static void VerifyExtractedTitleGraphics(ISnesAddressSpace bus)
    {
        _ = TitleCardOracle.CompareAll(bus);
        var cartridgeReads = new HashSet<int>();
        var tracingBus = NewTitlePresentationReadBus(bus, cartridgeReads, forbidReads: false);
        IReadOnlyDictionary<string, byte[]> files =
            SuperMetroid.AssetExtraction.Frontend.Title.TitleGraphicsExtractor.Extract(tracingBus);
        TitleGraphicsPresentation presentation = TitleGraphicsCharacters.Read(
            new MemoryStream(files[TitleGraphicsFormat.FloorTilesFile]),
            new MemoryStream(files[TitleGraphicsFormat.FloorMapFile]),
            new MemoryStream(files[TitleGraphicsFormat.ObjectTilesFile]),
            new MemoryStream(files[TitleGraphicsFormat.BabyTilesFile]));
        AssertSource(TitleSourceDefinitions.FloorCharacters,
            TitleSequenceDefinitions.Graphics.FloorCharacterByteCount, presentation.FloorCharacters,
            "title Mode 7 characters");
        AssertSource(TitleSourceDefinitions.FloorMap,
            TitleSequenceDefinitions.Graphics.FloorMapByteCount, presentation.FloorMap,
            "title Mode 7 map");
        AssertSource(TitleSourceDefinitions.ObjectCharacters,
            TitleSequenceDefinitions.Graphics.ObjectCharacterByteCount, presentation.ObjectCharacters,
            "title OBJ characters");
        AssertSource(TitleSourceDefinitions.BabyMetroidCharacters,
            TitleSequenceDefinitions.Graphics.BabyCharacterByteCount, presentation.BabyCharacters,
            "title Baby characters");

        // The installed title uses compiled card timing and sprite selectors alongside
        // editable bank-$8C OAM compositions. The original source addresses are all
        // blocked while the complete natural scene is compared with independently installed stock.
        var guardedBus = NewTitlePresentationReadBus(bus, cartridgeReads, forbidReads: true);
        TitleGradientPresentation gradient = TitleGradientPresentation.Load(
            new MemoryStream(TitleGradientExtractor.Extract(bus), writable: false));
        var stock = CreateTitlePresentationFixture(titleGradientPresentation: gradient);
        var installed = CreateTitlePresentationFixture(titleGradientPresentation: gradient, titleGraphicsPresentation: presentation);
        for (int frame = 0; frame < 140; frame++)
        {
            stock.Step(0);
            installed.Step(0);
            if (!stock.Render().SequenceEqual(installed.Render()))
                throw new InvalidDataException($"Installed title graphics differ from stock at frame {frame}.");
        }
        bool reachedTitle = false;
        for (int frame = 140; frame < 1800; frame++)
        {
            stock.Step(0);
            installed.Step(0);
            FloorScene native = stock.CaptureRenderSnapshot();
            FloorScene extracted = installed.CaptureRenderSnapshot();
            if (stock.Phase != installed.Phase ||
                !native.Paint.SpriteParts().SequenceEqual(extracted.Paint.SpriteParts()))
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

        TitleFloorMapDocument spriteDocument = JsonSerializer.Deserialize<TitleFloorMapDocument>(
            files[TitleGraphicsFormat.FloorMapFile], ArtworkJson.Options)
            ?? throw new InvalidDataException("Extracted title map document is null.");
        AssertEqual(TitleGraphicsFormat.SpriteFrameCount, spriteDocument.Sprites.Length,
            "title extraction covers all cartridge-selected OBJ frames");
        ushort yearPointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
            TitleSourceDefinitions.TextLists[TitleSequencePhase.YearText] + 6);
        IReadOnlyDictionary<TitleSprite, ushort> spritemaps =
            SuperMetroid.AssetExtraction.Frontend.Title.TitleSpriteExtractor.MapSprites(bus);
        AssertEqual(yearPointer, spritemaps[TitleSprite.YearReveal1], "the year's first letter is the year card's first listed spritemap");
        // Independent native OBJ records retain the cartridge reference now that both
        // scene owners require installed graphics rather than a ROM-backed draw path.
        foreach (TitleSpriteFrame frame in spriteDocument.Sprites)
        {
            var nativeOam = new OamBuffer(); var installedOam = ScreenSpritesFixtures.Blank();
            int address = ((int)new SnesAddress(FrontendSourceDefinitions.TitleSpritemapBank, spritemaps[frame.Sprite]));
            ushort Word(int at) => SnesLittleEndian.Word(CpuBusRegionRead.Read(bus, at), CpuBusRegionRead.Read(bus, at + 1));
            int count = Word(address);
            for (int part = 0; part < count; part++)
            {
                int record = address + 2 + part * 5;
                nativeOam.AddOnScreenSpritePart(new SnesSpritemapXWord(Word(record)),
                    new SnesSpritemapYByte(CpuBusRegionRead.Read(bus, record + 2)), new SnesObjAttributeWord(Word(record + 3)).WithPaletteBits(0),
                    new Position16<OamPixels>(128, 112));
            }
            TitleGraphicsPresentation.DrawSprite(MenuSheetFixtures.WithTitle(presentation).Get(SpriteSheetId.Title), frame.Sprite, installedOam, new(128, 112), 0);
            nativeOam.FinalizeFrame(); installedOam.FinishFrame();
            AssertTrue(nativeOam.ImageEquals(installedOam), "Title composition matches independent native OBJ records");
        }
        TitleSpriteFrame year = spriteDocument.Sprites.Single(frame => frame.Sprite == TitleSprite.YearReveal1);
        SpriteVisualPart originalPart = year.Parts[0];
        year.Parts[0] = originalPart with { OffsetX = originalPart.OffsetX + 8 };
        byte[] editedMap = JsonSerializer.SerializeToUtf8Bytes(spriteDocument, ArtworkJson.Options);
        TitleGraphicsPresentation editedPresentation = TitleGraphicsCharacters.Read(
            new MemoryStream(files[TitleGraphicsFormat.FloorTilesFile]),
            new MemoryStream(editedMap),
            new MemoryStream(files[TitleGraphicsFormat.ObjectTilesFile]),
            new MemoryStream(files[TitleGraphicsFormat.BabyTilesFile]));
        var originalTitle = CreateTitlePresentationFixture(titleGradientPresentation: gradient, titleGraphicsPresentation: presentation);
        var editedTitle = CreateTitlePresentationFixture(titleGradientPresentation: gradient, titleGraphicsPresentation: editedPresentation);
        bool editVisible = false;
        bool pixelChanged = false;
        for (int frame = 0; frame < 100; frame++)
        {
            originalTitle.Step(0);
            editedTitle.Step(0);
            if (originalTitle.Phase != editedTitle.Phase)
                throw new InvalidDataException("Title sprite edit altered the native title timing.");
            if (!originalTitle.CaptureRenderSnapshot().Paint.SpriteParts().SequenceEqual(
                    editedTitle.CaptureRenderSnapshot().Paint.SpriteParts()))
            {
                editVisible = true;
                pixelChanged |= !originalTitle.Render().SequenceEqual(editedTitle.Render());
            }
        }
        if (!editVisible)
            throw new InvalidDataException("Editing the Year composition did not alter production OAM.");
        if (!pixelChanged)
            throw new InvalidDataException("Editing the Year composition did not alter visible title pixels.");
        var reboundTitle = CreateTitlePresentationFixture(titleGradientPresentation: gradient, titleGraphicsPresentation: presentation);
        for (int frame = 0; frame < 64; frame++) reboundTitle.Step(0);
        PlacedSpritePart[] beforeRebind = reboundTitle.CaptureRenderSnapshot().Paint.SpriteParts();
        reboundTitle.BindFixtureTitleGraphics(editedPresentation);
        if (beforeRebind.SequenceEqual(reboundTitle.CaptureRenderSnapshot().Paint.SpriteParts()))
            throw new InvalidDataException("Rebinding a live title failed to apply current sprite composition.");
        reboundTitle.BindFixtureTitleGraphics(presentation);
        if (!beforeRebind.SequenceEqual(reboundTitle.CaptureRenderSnapshot().Paint.SpriteParts()))
            throw new InvalidDataException("Rebinding stock title composition failed to restore the frame.");
        year.Parts[0] = originalPart with { TileRow = TitleGraphicsFormat.ObjectTileRows };
        byte[] malformedMap = JsonSerializer.SerializeToUtf8Bytes(spriteDocument, ArtworkJson.Options);
        AssertThrows<InvalidDataException>(() => TitleGraphicsCharacters.Read(
            new MemoryStream(files[TitleGraphicsFormat.FloorTilesFile]),
            new MemoryStream(malformedMap),
            new MemoryStream(files[TitleGraphicsFormat.ObjectTilesFile]),
            new MemoryStream(files[TitleGraphicsFormat.BabyTilesFile])),
            "title graphics reject an out-of-range OBJ region");

        IndexedPngImage mode7 = IndexedPng.Read(
            new MemoryStream(files[TitleGraphicsFormat.FloorTilesFile]),
            new(TitleGraphicsFormat.FloorWidth, TitleGraphicsFormat.FloorHeight));
        using var wrong = new MemoryStream();
        IndexedPng.Write(wrong, new(8, 8), new byte[64], mode7.Colours);
        wrong.Position = 0;
        AssertThrows<InvalidDataException>(() => TitleGraphicsCharacters.Read(
            wrong,
            new MemoryStream(files[TitleGraphicsFormat.FloorMapFile]),
            new MemoryStream(files[TitleGraphicsFormat.ObjectTilesFile]),
            new MemoryStream(files[TitleGraphicsFormat.BabyTilesFile])),
            "title graphics reject wrong Mode 7 PNG dimensions");
        Console.WriteLine(
            $"  Title graphics presentation: four editable assets and all title OBJ frames match native OAM; " +
            $"production avoided {cartridgeReads.Count} cartridge-art bytes.");

        void AssertSource(int address, int count, ReadOnlySpan<byte> actual, string description)
        {
            byte[] source = RomDataReader.Decompress(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus), address);
            if (!source.AsSpan(0, count).SequenceEqual(actual))
                throw new InvalidDataException($"Extracted {description} differ from cartridge data.");
        }
    }

    /// <summary>Checks extracted title colors, skip colors, ambient loops, overrides, and installed CGRAM reads.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the title palette")]
    private static void VerifyExtractedTitlePalette(ISnesAddressSpace bus)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.Frontend.Title.TitlePaletteExtractor.Extract(bus);
        TitlePalettePresentation presentation = TitlePaletteFile.Read(
            new MemoryStream(extracted, writable: false));
        var expected = new SnesCgram();
        SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(expected, bus, TitleSourceDefinitions.Palette);
        if (!presentation.Colors.SequenceEqual(expected.Colors))
            throw new InvalidDataException("Extracted title palette differs from cartridge CGRAM data.");
        AssertEqual(TitleSequenceDefinitions.Palette.CopyrightWhite,
            presentation.SkipCopyrightWhite, "extracted title skip copyright white");
        AssertEqual(TitleSequenceDefinitions.Palette.CopyrightRed,
            presentation.SkipCopyrightRed, "extracted title skip copyright red");

        Suite(nameof(VerifyExtractedTitleAmbientPalette), () => VerifyExtractedTitleAmbientPalette(bus, presentation));

        Suite(nameof(VerifyTitlePaletteValidation), () => VerifyTitlePaletteValidation(extracted));
        var paletteAddresses = Enumerable.Range(
            TitleSourceDefinitions.Palette,
            CgramPalettes.ByteCount).ToHashSet();
        var guardedBus = NewTitlePresentationReadBus(bus, paletteAddresses, forbidReads: true);
        var title = CreateTitlePresentationFixture(titlePalettePresentation: presentation);
        if (!title.PaletteColors.SequenceEqual(presentation.Colors) ||
            guardedBus.ForbiddenReadAttempts != 0)
        {
            throw new InvalidDataException(
                "Production title initialization did not use the installed palette exclusively.");
        }
        var nativeSkip = CreateTitlePresentationFixture();
        var installedSkip = CreateTitlePresentationFixture(titlePalettePresentation: presentation);
        nativeSkip.Step(ControllerButton.Start);
        installedSkip.Step(ControllerButton.Start);
        for (int frame = 0; frame < 18; frame++)
        {
            nativeSkip.Step(0);
            installedSkip.Step(0);
        }
        AssertEqual(nativeSkip.Phase, installedSkip.Phase, "title skip phase unaffected by installed colors");
        AssertEqual(nativeSkip.PaletteColors[ScreenColourPositions.Sprite(TitleCopyrightColourDefinitions.SpriteRow, TitleCopyrightColourDefinitions.WhiteColour)],
            installedSkip.PaletteColors[ScreenColourPositions.Sprite(TitleCopyrightColourDefinitions.SpriteRow, TitleCopyrightColourDefinitions.WhiteColour)],
            "installed title skip white matches native");
        AssertEqual(nativeSkip.PaletteColors[ScreenColourPositions.Sprite(TitleCopyrightColourDefinitions.SpriteRow, TitleCopyrightColourDefinitions.RedColour)],
            installedSkip.PaletteColors[ScreenColourPositions.Sprite(TitleCopyrightColourDefinitions.SpriteRow, TitleCopyrightColourDefinitions.RedColour)],
            "installed title skip red matches native");
        Console.WriteLine(
            $"  Title palette presentation: {SnesCgram.ColorCount} initial, 36 ambient, and two skip " +
            $"editable colors match ROM; production avoided {paletteAddresses.Count} " +
            "initial cartridge source bytes.");
    }

    /// <summary>Matches all ambient palette-FX color frames to cartridge words and guards installed loops against rereads.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for title ambient palette color words")]
    private static void VerifyExtractedTitleAmbientPalette(
        ISnesAddressSpace bus,
        TitlePalettePresentation presentation)
    {
        var colorAddresses = new HashSet<int>();
        foreach (TitleScreenAmbientPaletteFxColorDefinition definition in
                 TitleScreenAmbientPaletteFxColorDefinitions.All)
        {
            for (int frame = 0; frame < definition.FrameCount; frame++)
            {
                for (int color = 0; color < definition.ColorsPerFrame; color++)
                {
                    ushort pointer = unchecked((ushort)(
                        definition.FramePointer(frame) + sizeof(ushort) +
                        color * sizeof(ushort)));
                    ushort expected = RomDataReader.ReadWordFixedBank(
                        CartridgeImportSource.Require(bus),
                        RoomFxBankSourceDefinitions.PaletteFx | pointer);
                    if (!presentation.TryReadColor(pointer, out RgbColor actual) ||
                        actual.ToWord() != expected)
                    {
                        throw new InvalidDataException(
                            $"Extracted title ambient color $8D:{pointer:X4} differs from cartridge data.");
                    }
                    colorAddresses.Add(RoomFxBankSourceDefinitions.PaletteFx | pointer);
                    colorAddresses.Add(RoomFxBankSourceDefinitions.PaletteFx |
                        unchecked((ushort)(pointer + 1)));
                }
            }
        }

        var guardedBus = NewTitlePresentationReadBus(bus, colorAddresses, forbidReads: true);
        var nativeSession = new PaletteFxSessionState();
        var native = new RoomPaletteFxSystem(nativeSession);
        var installedSession = new PaletteFxSessionState();
        var installed = new RoomPaletteFxSystem(installedSession);
        foreach (TitleScreenAmbientPaletteFxColorDefinition definition in
                 TitleScreenAmbientPaletteFxColorDefinitions.All)
        {
            native.SpawnDefinition(definition.DefinitionPointer, 0);
            installed.SpawnDefinition(definition.DefinitionPointer, 0);
        }

        var nativeCgram = new SamusColourRig();
        var installedCgram = new SamusColourRig();
        int frames = TitleScreenAmbientPaletteFxColorDefinitions.All.Max(
            definition => definition.CycleFrames) * 2;
        for (int frame = 0; frame < frames; frame++)
        {
            native.Step(nativeCgram, new ReferencePaletteFxColorSource(bus), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
            installed.Step(installedCgram, presentation, new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
            if (!new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(installedCgram)))
            {
                throw new InvalidDataException(
                    $"Installed title ambient palette diverged from native output on frame {frame}.");
            }
        }
        AssertEqual(0, guardedBus.ForbiddenReadAttempts,
            "installed title ambient loops avoid cartridge color reads");
    }

    /// <summary>Mutates title-palette JSON to check version, shape, color bounds, and native-address rejection.</summary>
    private static void VerifyTitlePaletteValidation(byte[] extracted)
    {
        TitlePaletteDocument document = JsonSerializer.Deserialize<TitlePaletteDocument>(
            extracted,
            ArtworkJson.Options)
            ?? throw new InvalidDataException("Extracted title palette document is null.");
        byte[] Json() => JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        void Reject(string description) => AssertThrows<InvalidDataException>(
            () => TitlePaletteFile.Read(new MemoryStream(Json(), writable: false)),
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
            () => TitlePaletteFile.Read(new MemoryStream(
                System.Text.Encoding.UTF8.GetBytes(unknownField), writable: false)),
            "title palette rejects native-address escape hatch");
    }

    /// <summary>Compares installed gradient variants with retail HDMA output and checks a production title frame avoids its source reads.</summary>
    private static void VerifyExtractedTitleGradient(ISnesAddressSpace bus)
    {
        var cartridgeReads = new HashSet<int>();
        var tracingBus = NewTitlePresentationReadBus(bus, cartridgeReads, forbidReads: false);
        for (ushort variant = 0; variant < TitleGradientFormat.VariantCount; variant++)
            _ = TitleGradientDecoder.Decode(tracingBus, checked((ushort)(variant << 4)));

        byte[] extracted = SuperMetroid.AssetExtraction.Frontend.Title.TitleGradientExtractor.Extract(bus);
        TitleGradientPresentation presentation = TitleGradientPresentation.Load(
            new MemoryStream(extracted, writable: false));
        for (ushort zoom = 0; zoom < 256; zoom++)
        {
            if (!presentation.Resolve(zoom).SequenceEqual(
                    TitleGradientDecoder.Decode(CartridgeImportSource.Require(bus), zoom)))
                throw new InvalidDataException($"Extracted title gradient differs at zoom ${zoom:X2}.");
        }
        Suite(nameof(VerifyTitleGradientValidation), () => VerifyTitleGradientValidation(extracted));

        // The rest of title setup remains cartridge-backed presentation for later #549
        // slices. Forbid only the complete gradient source closure discovered above, then
        // drive the real title owner through Start's skip fade into a gradient frame.
        var guardedBus = NewTitlePresentationReadBus(bus, cartridgeReads, forbidReads: true);
        TitlePalettePresentation palette = TitlePaletteFile.Read(new MemoryStream(
            TitlePaletteExtractor.Extract(bus)));
        IReadOnlyDictionary<string, byte[]> graphicsFiles = TitleGraphicsExtractor.Extract(bus);
        TitleGraphicsPresentation graphics = TitleGraphicsCharacters.Read(
            new MemoryStream(graphicsFiles[TitleGraphicsFormat.FloorTilesFile]),
            new MemoryStream(graphicsFiles[TitleGraphicsFormat.FloorMapFile]),
            new MemoryStream(graphicsFiles[TitleGraphicsFormat.ObjectTilesFile]),
            new MemoryStream(graphicsFiles[TitleGraphicsFormat.BabyTilesFile]));
        (TitleGradientLine[] rendered, ushort selectedZoom) = CaptureInstalledTitleGradient(
            presentation, palette, graphics);
        if (!rendered.SequenceEqual(presentation.Resolve(selectedZoom)) ||
            guardedBus.ForbiddenReadAttempts != 0)
        {
            throw new InvalidDataException(
                "Production title rendering did not use the installed gradient presentation exclusively.");
        }
        Console.WriteLine(
            $"  Title gradient presentation: {TitleGradientFormat.VariantCount} editable variants match ROM; " +
            $"production avoided {cartridgeReads.Count} cartridge source bytes.");
    }

    /// <summary>Mutates extracted gradient JSON to check schema, line structure, blend fields, and native-address rejection.</summary>
    private static void VerifyTitleGradientValidation(byte[] extracted)
    {
        TitleGradientDocument document = JsonSerializer.Deserialize<TitleGradientDocument>(
            extracted,
            ArtworkJson.Options)
            ?? throw new InvalidDataException("Extracted title gradient document is null.");
        byte[] Json() => JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
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
        first.Lines[0] = line;

        string extractedText = System.Text.Encoding.UTF8.GetString(extracted);
        void RejectText(string text, string description) => AssertThrows<InvalidDataException>(
            () => TitleGradientPresentation.Load(new MemoryStream(
                System.Text.Encoding.UTF8.GetBytes(text), writable: false)),
            description);
        string versionField = $"\"version\": {TitleGradientFormat.Version}";
        AssertTrue(extractedText.Contains(versionField, StringComparison.Ordinal), "extracted title gradient states its schema version");
        RejectText(extractedText.Replace(versionField, versionField + ",\n  \"nativeAddress\": 9192685", StringComparison.Ordinal),
            "title gradient rejects native-address escape hatch");
        // A line's blend is named layers and a direction; the revision-1 control byte is not accepted beside or instead of them.
        AssertTrue(extractedText.Contains("\"blendsFloor\": ", StringComparison.Ordinal), "extracted title gradient names each line's blended layers");
        RejectText(extractedText.Replace("\"blendsFloor\": ", "\"colorMathControl\": 161,\n        \"blendsFloor\": ", StringComparison.Ordinal),
            "title gradient rejects a color-math control byte");
        string withoutDirection = System.Text.RegularExpressions.Regex.Replace(
            extractedText, ",\\s*\"subtracts\": (true|false)", string.Empty);
        AssertTrue(withoutDirection != extractedText, "extracted title gradient states each line's blend direction");
        RejectText(withoutDirection, "title gradient rejects a line without its blend direction");
    }

    /// <summary>Steps the installed title through Start's skip fade and captures the first visible gradient and Mode 7 zoom.</summary>
    internal static CaptureInstalledTitleGradientResult CaptureInstalledTitleGradient(
        TitleGradientPresentation presentation,
        TitlePalettePresentation palette,
        TitleGraphicsPresentation graphics)
    {
        var title = CreateTitlePresentationFixture(titleGradientPresentation: presentation,
            titlePalettePresentation: palette, titleGraphicsPresentation: graphics);
        title.Step(ControllerButton.Start);
        for (int frame = 0; frame < 16; frame++)
        {
            title.Step(0);
            FloorScene snapshot = title.CaptureRenderSnapshot();
            if (!snapshot.Gradient.IsEmpty)
                return new CaptureInstalledTitleGradientResult(snapshot.Gradient.ToArray(), title.Mode7MatrixScale);
        }
        throw new InvalidDataException("Production title rendering never reached a gradient frame.");
    }

    /// <summary>Records title source addresses during extraction or forbids those addresses during installed rendering.</summary>
    internal static CartridgeReadGuard NewTitlePresentationReadBus(ISnesAddressSpace source, HashSet<int> addresses, bool forbidReads) =>
        forbidReads
            ? new CartridgeReadGuard(source).Forbid(addresses.Contains,
                address => $"Production title gradient reread cartridge byte {SnesAddressMath.BankNumber(address):X2}:{SnesAddressMath.OffsetInBank(address):X4}.")
            : new CartridgeReadGuard(source).Observe(_ => true, address => addresses.Add(address));

    /// <summary>Runs the title gradient and instruction-definition retail suites from the command line.</summary>
    [VerificationCommand("--title-instruction-definitions")]
    internal static int TitleInstructionDefinitionsCommand()
    {
        Suite(nameof(VerifyTitleGradientTables), () => VerifyTitleGradientTables(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"))));
        Suite(nameof(VerifyTitleSequenceRomData), () => VerifyTitleSequenceRomData());
        return 0;
    }
}
