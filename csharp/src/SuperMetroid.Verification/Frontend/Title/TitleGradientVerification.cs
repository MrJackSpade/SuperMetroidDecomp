using SuperMetroid.Verification.Fx.PaletteFx;
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
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.AssetExtraction.Video;
using System.Collections.Immutable;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Frontend.Title;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

/// <summary>Checks extracted title graphics, palettes, and HDMA gradient through installed presentation and retail oracles.</summary>
internal static class TitleGradientVerification
{
    /// <summary>The title's upper bands darken the floor and the backdrop (#1514).</summary>
    private const GradientBlend SubtractFromBackground = GradientBlend.DarkensFloorAndBackdrop;
    /// <summary>The title's lower bands brighten the floor, the backdrop and the sprites (#1514).</summary>
    private const GradientBlend AddToBackgroundAndObjects = GradientBlend.BrightensFloorBackdropAndSprites;

    /// <summary>Creates the production title owner with selected installed graphics, palette, and gradient overrides.</summary>
    private static TitleSequenceState CreateTitlePresentationFixture(TitleGradientPresentation? titleGradientPresentation = null,
        RgbColor? titleBackdrop = null,
        TitleGraphicsCharacters? titleGraphicsPresentation = null)
 =>
        InstalledFixtures.CreateTitle(titleGradientPresentation: titleGradientPresentation,
            titleBackdrop: titleBackdrop, titleGraphicsPresentation: titleGraphicsPresentation);
    /// <summary>Checks native gradient band boundaries and all 256 zoom selections after artwork and schema suites.</summary>
    internal static void VerifyTitleGradientTables(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {
        Suite(nameof(VerifyExtractedTitleGraphics), () => VerifyExtractedTitleGraphics(bus));
        Suite(nameof(VerifyExtractedTitlePalette), () => VerifyExtractedTitlePalette(bus));
        Suite(nameof(VerifyExtractedTitleGradient), () => VerifyExtractedTitleGradient(bus));
        // Independently transcribed boundaries from $8C:BC7D and $88:EB95, now as the stops their bands begin on (#1468).
        ScreenGradient gradient = TitleGradientDecoder.Decode(bus, 0);
        AssertEqual(0, gradient.Top, "title gradient's band starts on the top line");
        AssertEqual(ScreenGeometry.ScreenHeightPixels, gradient.Bottom, "title gradient's band runs to the bottom of the screen");
        AssertEqual(new GradientStop(0, new RgbColor(123, 123, 123), SubtractFromBackground), gradient.Stops[0], "title gradient starts subtracting fifteen");
        AssertStop(gradient, new GradientStop(4, new RgbColor(115, 115, 115), SubtractFromBackground), "title gradient steps down after four lines");
        GradientStop lastSubtractive = StopCovering(gradient, 121);
        AssertEqual(SubtractFromBackground, lastSubtractive.Blend, "last subtractive title line subtracts");
        AssertEqual(RgbColor.Black, lastSubtractive.Colour, "last subtractive title line's colour is black");
        AssertStop(gradient, new GradientStop(122, RgbColor.Black, AddToBackgroundAndObjects), "first additive title band begins");
        AssertStop(gradient, new GradientStop(132, new RgbColor(0, 8, 8), AddToBackgroundAndObjects), "lower title cyan band begins");
        AssertStop(gradient, new GradientStop(144, new RgbColor(0, 16, 16), AddToBackgroundAndObjects), "second lower cyan band begins");
        for (int stop = 1; stop < gradient.Stops.Length; stop++)
        {
            GradientStop above = gradient.Stops[stop - 1], below = gradient.Stops[stop];
            AssertTrue(above.Colour != below.Colour || above.Blend != below.Blend, $"title stop on line {below.Line} changes the colour or the blend");
        }
        for (ushort zoom = 0; zoom < 256; zoom++)
        {
            ScreenGradient actual = TitleGradientDecoder.Decode(bus, zoom);
            ScreenGradient nibble = TitleGradientDecoder.Decode(bus, (ushort)(zoom & 0xf0));
            if (!actual.Equals(nibble))
                throw new InvalidDataException("Title gradient index used low zoom bits.");
            AssertEqual(SubtractFromBackground, StopCovering(actual, 121).Blend, "zoom preserves subtractive boundary");
            AssertEqual(AddToBackgroundAndObjects, StopCovering(actual, 122).Blend, "zoom preserves additive boundary");
            AssertEqual(122, StopCovering(actual, 122).Line, "zoom keeps the additive band beginning on its line");
        }
        Console.WriteLine($"  Title gradient: {gradient.Stops.Length} stops; 256 zoom selections, native cyan bands and subtract/add boundary agree.");
    }

    /// <summary>The stop whose band covers <paramref name="line"/>: the last beginning on or above it.</summary>
    private static GradientStop StopCovering(ScreenGradient gradient, int line) =>
        gradient.Stops.Last(stop => stop.Line <= line);

    /// <summary>Asserts that <paramref name="gradient"/> has a stop beginning exactly on <paramref name="expected"/>'s line, as <paramref name="expected"/> describes it.</summary>
    private static void AssertStop(ScreenGradient gradient, GradientStop expected, string description) =>
        AssertEqual(expected, StopCovering(gradient, expected.Line), description);

    /// <summary>Compares extracted title character and map transfers and the title's OBJ frames with their retail sources.</summary>
    private static void VerifyExtractedTitleGraphics(ISnesAddressSpace bus)
    {
        _ = TitleCardOracle.CompareAll(bus);
        IReadOnlyDictionary<string, byte[]> files =
            SuperMetroid.AssetExtraction.Frontend.Title.TitleGraphicsExtractor.Extract(bus);
        TitleGraphicsCharacters presentation = TitleGraphicsCharacters.Read(
            new MemoryStream(files[TitleGraphicsFormat.FloorTilesFile]),
            new MemoryStream(files[TitleGraphicsFormat.FloorMapFile]),
            new MemoryStream(files[TitleGraphicsFormat.ObjectTilesFile]),
            new MemoryStream(files[TitleGraphicsFormat.BabyTilesFile]));
        AssertSource(TitleSourceDefinitions.FloorCharacters,
            TitleFloorTileDefinitions.FloorCharacterByteCount, presentation.FloorCharacters,
            "title Mode 7 characters");
        AssertSource(TitleSourceDefinitions.FloorMap,
            TitleFloorTileDefinitions.FloorMapByteCount, presentation.FloorMap,
            "title Mode 7 map");
        AssertSource(TitleSourceDefinitions.ObjectCharacters,
            TitleGraphicsFormat.ObjectCharacterByteCount, presentation.ObjectCharacters,
            "title OBJ characters");
        AssertSource(TitleSourceDefinitions.BabyMetroidCharacters,
            TitleFloorTileDefinitions.BabyCharacterByteCount, presentation.BabyCharacters,
            "title Baby characters");

        TitleFloorMapDocument spriteDocument = JsonSerializer.Deserialize<TitleFloorMapDocument>(
            files[TitleGraphicsFormat.FloorMapFile], ArtworkJson.Options)
            ?? throw new InvalidDataException("Extracted title map document is null.");
        AssertEqual(TitleGraphicsFormat.SpriteFrameCount, spriteDocument.Sprites.Length,
            "title extraction covers all cartridge-selected OBJ frames");
        ushort yearPointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
            TitleSourceDefinitions.TextLists[TitleSequencePhase.YearText] + 6);
        IReadOnlyDictionary<TitleSprite, int> spritemaps =
            SuperMetroid.AssetExtraction.Frontend.Title.TitleSpriteExtractor.MapSprites(bus);
        AssertEqual(yearPointer, spritemaps[TitleSprite.YearReveal1], "the year's first letter is the year card's first listed spritemap");
        // Independent native OBJ records are the cartridge reference for each extracted composition's draw.
        // Both draws show the title cards' text colours: the native records take their sprite set as palette bits.
        ushort textPaletteBits = SnesObjAttributeWord.Create(0, SuperMetroid.AssetExtraction.Frontend.Title.Artwork.TitleSheetInks.SpriteSet(TitleSpriteColouring.Text), 0).PaletteBits;
        foreach (TitleSpriteFrame frame in spriteDocument.Sprites)
        {
            var nativeOam = new OamBuffer(); var installedOam = ScreenSpritesFixtures.Blank();
            int address = ((int)SnesAddress.InBank(FrontendSourceDefinitions.TitleSpritemapBank, spritemaps[frame.Sprite]));
            ushort Word(int at) => LittleEndianBytes.Word(CpuBusRegionRead.Read(bus, at), CpuBusRegionRead.Read(bus, at + 1));
            int count = Word(address);
            for (int part = 0; part < count; part++)
            {
                int record = address + 2 + part * 5;
                nativeOam.AddOnScreenSpritePart(new SnesSpritemapXWord(Word(record)),
                    new SnesSpritemapYByte(CpuBusRegionRead.Read(bus, record + 2)), new SnesObjAttributeWord(Word(record + 3)).WithPaletteBits(textPaletteBits),
                    new Position16<SpritePixels>(128, 112));
            }
            TitleSpriteDrawing.DrawSprite(MenuSheetFixtures.WithTitle(presentation).Get(SpriteSheetId.Title), frame.Sprite, installedOam, new(128, 112), TitleSpriteColouring.Text);
            nativeOam.FinalizeFrame(); installedOam.FinishFrame();
            // The glyphs of a typed card never overlap, so the order the objects are listed in shows nothing: the native
            // spritemap lists the newest glyph first and the installed draw places glyph 0 first. Compare the objects as a set.
            AssertEqual(string.Join(',', ObjectRecords(nativeOam)), string.Join(',', ObjectRecords(ScreenSpritesFixtures.ToOam(installedOam))),
                $"Title composition {frame.Sprite} holds the independent native OBJ records");
        }
        // Every object's record and size bits, in a fixed order, so two frames listing the same objects compare equal.
        static string[] ObjectRecords(OamBuffer oam)
        {
            var records = new string[oam.LowTable.Length / 4];
            for (int index = 0; index < records.Length; index++)
                records[index] = $"{Convert.ToHexString(oam.LowTable.Slice(index * 4, 4))}/{oam.HighTable[index / 4] >> (index % 4 * 2) & 3}";
            Array.Sort(records, StringComparer.Ordinal);
            return records;
        }
        TitleSpriteFrame year = spriteDocument.Sprites.Single(frame => frame.Sprite == TitleSprite.YearReveal1);
        SpriteVisualPart originalPart = year.Parts[0];
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
        Console.WriteLine("  Title graphics presentation: four extracted assets match the cartridge and all title OBJ frames match native OAM.");

        void AssertSource(int address, int count, ReadOnlySpan<byte> actual, string description)
        {
            byte[] source = RomDataReader.Decompress(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus), address);
            if (!source.AsSpan(0, count).SequenceEqual(actual))
                throw new InvalidDataException($"Extracted {description} differ from cartridge data.");
        }
    }

    /// <summary>Checks extracted title colors, skip colors, ambient loops, overrides, and installed CGRAM reads.</summary>
    private static void VerifyExtractedTitlePalette(ISnesAddressSpace bus)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.Frontend.Title.TitlePaletteExtractor.Extract(bus);
        TitlePalettePresentation presentation = TitlePaletteFile.Read(
            new MemoryStream(extracted, writable: false));
        var expected = new SnesCgram();
        SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(expected, bus, TitleSourceDefinitions.Palette);
        if (!presentation.Colours.AsSpan().SequenceEqual(expected.Colors))
            throw new InvalidDataException("Extracted title palette differs from cartridge CGRAM data.");
        AssertEqual(SuperMetroid.AssetExtraction.Frontend.Title.TitleSkipCopyrightColours.White,
            presentation.SkipCopyrightWhite, "extracted title skip copyright white");
        AssertEqual(SuperMetroid.AssetExtraction.Frontend.Title.TitleSkipCopyrightColours.Red,
            presentation.SkipCopyrightRed, "extracted title skip copyright red");
        AssertEqual(expected.Colors[0], presentation.Backdrop, "extracted title backdrop is the cartridge's first colour");

        Suite(nameof(VerifyExtractedTitleAmbientPalette), () => VerifyExtractedTitleAmbientPalette(bus, presentation));

        Suite(nameof(VerifyTitlePaletteValidation), () => VerifyTitlePaletteValidation(extracted));
        var paletteAddresses = Enumerable.Range(
            TitleSourceDefinitions.Palette,
            CgramPalettes.ByteCount).ToHashSet();
        var guardedBus = NewTitlePresentationReadBus(bus, paletteAddresses, forbidReads: true);
        var title = CreateTitlePresentationFixture(titleBackdrop: presentation.Backdrop);
        if (title.CaptureRenderSnapshot().Paint.Backdrop != presentation.Backdrop ||
            guardedBus.ForbiddenReadAttempts != 0)
        {
            throw new InvalidDataException(
                "Production title initialization did not use the installed palette exclusively.");
        }
        var skip = CreateTitlePresentationFixture();
        skip.Step(ControllerButton.Start);
        for (int frame = 0; frame < 18; frame++)
            skip.Step(0);
        AssertTrue(skip.CopyrightRestored, "the skip restores the copyright's glyph colours");
        Console.WriteLine(
            $"  Title palette presentation: {SnesCgram.ColorCount} initial, 36 ambient, and two skip " +
            $"installed colors match ROM; production avoided {paletteAddresses.Count} " +
            "initial cartridge source bytes.");
    }

    /// <summary>Matches all ambient palette-FX color frames to cartridge words and guards installed loops against rereads.</summary>
    private static void VerifyExtractedTitleAmbientPalette(
        ISnesAddressSpace bus,
        TitlePalettePresentation presentation)
    {
        var colorAddresses = new HashSet<int>();
        foreach (TitleScreenAmbientPaletteFxColorDefinition definition in
                 TitleScreenAmbientPaletteOwners.All)
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
        var nativeSession = new ColourEffectSessionState();
        var native = new ColourEffectSystem(nativeSession);
        var installedSession = new ColourEffectSessionState();
        var installed = new ColourEffectSystem(installedSession);
        foreach (TitleScreenAmbientPaletteFxColorDefinition definition in
                 TitleScreenAmbientPaletteOwners.All)
        {
            native.Start(definition.DefinitionPointer, 0);
            installed.Start(definition.DefinitionPointer, 0);
        }

        var nativeCgram = new SnesCgram();
        var installedCgram = new SnesCgram();
        int frames = TitleScreenAmbientPaletteOwners.All.Max(
            definition => definition.CycleFrames) * 2;
        for (int frame = 0; frame < frames; frame++)
        {
            native.StepScreen(new ColourEffectInputs { SamusY = 0, EquippedItems = 0, AreaMiniBossDefeated = false });
            PaletteFxRoomLook.LayShownShades(native, nativeCgram, new ReferencePaletteFxColorSource(bus));
            installed.StepScreen(new ColourEffectInputs { SamusY = 0, EquippedItems = 0, AreaMiniBossDefeated = false });
            PaletteFxRoomLook.LayShownShades(installed, installedCgram, presentation);
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
        TitleGradientPresentation presentation = TitleGradientFile.Read(
            new MemoryStream(extracted, writable: false));
        // The gradient the game loads from the installed file, every stop's line, colour and blend, is the import's
        // decoding of the cartridge at every zoom (#1709).
        TitleGradientPresentation installed = InstalledFixtures.Maps.TitleGradient;
        for (ushort zoom = 0; zoom < 256; zoom++)
        {
            ScreenGradient decoded = TitleGradientDecoder.Decode(CartridgeImportSource.Require(bus), zoom);
            if (!presentation.Resolve(FloorZoom.FromTwoHundredFiftySixths(zoom)).Equals(decoded))
                throw new InvalidDataException($"Extracted title gradient differs at zoom ${zoom:X2}.");
            if (!installed.Resolve(FloorZoom.FromTwoHundredFiftySixths(zoom)).Equals(decoded))
                throw new InvalidDataException($"Installed title gradient differs at zoom ${zoom:X2}.");
        }
        Suite(nameof(VerifyTitleGradientValidation), () => VerifyTitleGradientValidation(extracted));

        // The rest of title setup remains cartridge-backed presentation for later #549
        // slices. Forbid only the complete gradient source closure discovered above, then
        // drive the real title owner through Start's skip fade into a gradient frame.
        var guardedBus = NewTitlePresentationReadBus(bus, cartridgeReads, forbidReads: true);
        TitlePalettePresentation palette = TitlePaletteFile.Read(new MemoryStream(
            TitlePaletteExtractor.Extract(bus)));
        IReadOnlyDictionary<string, byte[]> graphicsFiles = TitleGraphicsExtractor.Extract(bus);
        TitleGraphicsCharacters graphics = TitleGraphicsCharacters.Read(
            new MemoryStream(graphicsFiles[TitleGraphicsFormat.FloorTilesFile]),
            new MemoryStream(graphicsFiles[TitleGraphicsFormat.FloorMapFile]),
            new MemoryStream(graphicsFiles[TitleGraphicsFormat.ObjectTilesFile]),
            new MemoryStream(graphicsFiles[TitleGraphicsFormat.BabyTilesFile]));
        CaptureInstalledTitleGradientResult capture = CaptureInstalledTitleGradient(presentation, palette.Backdrop, graphics);
        if (!capture.Gradient.Equals(presentation.Resolve(capture.Zoom)) ||
            guardedBus.ForbiddenReadAttempts != 0)
        {
            throw new InvalidDataException(
                "Production title rendering did not use the installed gradient presentation exclusively.");
        }
        Suite(nameof(VerifyRenderedTitleGradient), () => VerifyRenderedTitleGradient(capture));
        Console.WriteLine(
            $"  Title gradient presentation: {TitleGradientFormat.VariantCount} installed variants match ROM; " +
            $"production avoided {cartridgeReads.Count} cartridge source bytes.");
    }

    /// <summary>
    /// Draws the installed title's first gradient frame at full brightness from its stops and checks the backend steps
    /// exactly where each band begins (#1468): the frame equals the same gradient described with a stop on every line, the gradient
    /// changes the frame, and a band that begins below the top and ends above the bottom draws its lines as the whole
    /// gradient does while leaving the lines outside it as the frame without a gradient shows them.
    /// </summary>
    private static void VerifyRenderedTitleGradient(CaptureInstalledTitleGradientResult capture)
    {
        FloorScene scene = capture.Scene;
        ScreenGradient gradient = capture.Gradient;
        RgbaPixel[] Draw(ScreenGradient? shown) => DisplayRendering.Render(new DisplayPacket(0,
            new FloorScene(scene.Paint, scene.Background, DisplayBrightness.Full, shown is null ? null : new ScreenGradientLayer(shown), scene.Floor)));
        ImmutableArray<GradientStop> everyLine = [.. Enumerable.Range(gradient.Top, gradient.Bottom - gradient.Top)
            .Select(line => StopCovering(gradient, line) with { Line = line })];
        RgbaPixel[] stepped = Draw(gradient);
        AssertTrue(stepped.SequenceEqual(Draw(new ScreenGradient(everyLine, gradient.Bottom))),
            "the title gradient drawn from its stops matches it described line by line");
        RgbaPixel[] plain = Draw(null);
        AssertTrue(!stepped.SequenceEqual(plain), "the title gradient changes the frame");
        AssertTrue(gradient.Stops.Length > 2, "the title gradient has stops inside the screen to band");
        var inner = new ScreenGradient([.. gradient.Stops.Skip(1).Take(gradient.Stops.Length - 2)], gradient.Stops[^1].Line);
        RgbaPixel[] banded = Draw(inner);
        int width = ScreenGeometry.ScreenWidthPixels;
        for (int line = 0; line < ScreenGeometry.ScreenHeightPixels; line++)
        {
            bool inside = line >= inner.Top && line < inner.Bottom;
            ReadOnlySpan<RgbaPixel> expected = (inside ? stepped : plain).AsSpan(line * width, width);
            AssertTrue(banded.AsSpan(line * width, width).SequenceEqual(expected),
                inside ? $"line {line} inside the band is drawn as the whole gradient draws it" : $"line {line} outside the band is left as it is");
        }
    }

    /// <summary>Mutates extracted gradient JSON to check schema, stop structure, blend fields, and native-address rejection.</summary>
    private static void VerifyTitleGradientValidation(byte[] extracted)
    {
        TitleGradientDocument document = JsonSerializer.Deserialize<TitleGradientDocument>(
            extracted,
            ArtworkJson.Options)
            ?? throw new InvalidDataException("Extracted title gradient document is null.");
        byte[] Json() => JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        void Reject(string description) => AssertThrows<InvalidDataException>(
            () => TitleGradientFile.Read(new MemoryStream(Json(), writable: false)),
            description);

        int version = document.Version;
        document = document with { Version = version + 1 };
        Reject("title gradient rejects unsupported schema version");
        document = document with { Version = version };

        TitleGradientVariant first = document.Variants[0];
        TitleGradientVariant[] variants = document.Variants;
        document = document with { Variants = variants[1..] };
        Reject("title gradient rejects a missing zoom bucket");
        document = document with { Variants = variants };
        document.Variants[0] = first with { Stops = [] };
        Reject("title gradient rejects a variant without stops");
        document.Variants[0] = first;

        TitleGradientStop top = first.Stops[0], second = first.Stops[1];
        first.Stops[0] = top with { Red = 32 };
        Reject("title gradient rejects non-RGB5 color");
        first.Stops[0] = top with { Line = 1 };
        Reject("title gradient rejects a first stop below the top line");
        first.Stops[0] = top;
        first.Stops[1] = second with { Line = top.Line };
        Reject("title gradient rejects a stop on its predecessor's line");
        first.Stops[1] = second with { Line = ScreenGeometry.ScreenHeightPixels };
        Reject("title gradient rejects a stop below the screen");
        first.Stops[1] = second;
        first.Stops[0] = top with { Blend = "Subtracts" };
        Reject("title gradient rejects a blend it does not name");
        first.Stops[0] = top with { Blend = "0" };
        Reject("title gradient rejects a blend by number");
        first.Stops[0] = top;

        string extractedText = System.Text.Encoding.UTF8.GetString(extracted);
        void RejectText(string text, string description) => AssertThrows<InvalidDataException>(
            () => TitleGradientFile.Read(new MemoryStream(
                System.Text.Encoding.UTF8.GetBytes(text), writable: false)),
            description);
        string versionField = $"\"version\": {TitleGradientFile.Version}";
        AssertTrue(extractedText.Contains(versionField, StringComparison.Ordinal), "extracted title gradient states its schema version");
        RejectText(extractedText.Replace(versionField, versionField + ",\n  \"nativeAddress\": 9192685", StringComparison.Ordinal),
            "title gradient rejects native-address escape hatch");
        // A stop's blend is named; neither the revision-1 control byte nor the revision-3 layers and direction are accepted
        // beside the name, and no variant carries the zoom bucket it is listed at.
        AssertTrue(extractedText.Contains("\"blend\": \"DarkensFloorAndBackdrop\"", StringComparison.Ordinal) &&
            extractedText.Contains("\"blend\": \"BrightensFloorBackdropAndSprites\"", StringComparison.Ordinal),
            "extracted title gradient names each stop's blend");
        RejectText(extractedText.Replace("\"blend\": ", "\"colorMathControl\": 161,\n          \"blend\": ", StringComparison.Ordinal),
            "title gradient rejects a color-math control byte");
        RejectText(extractedText.Replace("\"blend\": ", "\"subtracts\": true,\n          \"blend\": ", StringComparison.Ordinal),
            "title gradient rejects a blend direction beside the blend's name");
        string withoutBlend = System.Text.RegularExpressions.Regex.Replace(
            extractedText, ",\\s*\"blend\": \"[A-Za-z]+\"", string.Empty);
        AssertTrue(withoutBlend != extractedText, "extracted title gradient states each stop's blend");
        RejectText(withoutBlend, "title gradient rejects a stop without its blend");
        AssertTrue(!extractedText.Contains("zoomHighNibble", StringComparison.Ordinal), "extracted title gradient keys no variant by its zoom bucket");
        RejectText(extractedText.Replace("\"stops\": ", "\"zoomHighNibble\": 0,\n      \"stops\": ", StringComparison.Ordinal),
            "title gradient rejects a variant keyed by its zoom bucket");
        // The revision-2 per-line arrays are not accepted in place of stops.
        AssertTrue(extractedText.Contains("\"stops\": ", StringComparison.Ordinal), "extracted title gradient describes each variant by its stops");
        RejectText(extractedText.Replace("\"stops\": ", "\"lines\": ", StringComparison.Ordinal),
            "title gradient rejects per-line arrays in place of stops");
    }

    /// <summary>Steps the installed title through Start's skip fade and captures the first visible gradient and Mode 7 zoom.</summary>
    internal static CaptureInstalledTitleGradientResult CaptureInstalledTitleGradient(
        TitleGradientPresentation presentation,
        RgbColor backdrop,
        TitleGraphicsCharacters graphics)
    {
        var title = CreateTitlePresentationFixture(titleGradientPresentation: presentation,
            titleBackdrop: backdrop, titleGraphicsPresentation: graphics);
        title.Step(ControllerButton.Start);
        for (int frame = 0; frame < 16; frame++)
        {
            title.Step(0);
            FloorScene snapshot = title.CaptureRenderSnapshot();
            if (snapshot.Gradient is { } shown)
                return new CaptureInstalledTitleGradientResult(shown.Gradient, title.FloorZoom, snapshot);
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
