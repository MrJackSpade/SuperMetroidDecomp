using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;
using System.Text.Json;

internal static partial class Program
{
    private static void VerifyTitleGradientTables(SuperMetroidAddressSpace bus)
    {
        VerifyTitleGradientObjectEligibility();
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

    private static void VerifyExtractedTitlePalette(ISnesAddressSpace bus)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.TitlePaletteExtractor.Extract(bus);
        TitlePalettePresentation presentation = TitlePalettePresentation.Load(
            new MemoryStream(extracted, writable: false));
        var expected = new SnesCgram();
        expected.LoadFromBus(bus, TitleSequenceRomData.Assets.PaletteAddress);
        if (!presentation.Colors.SequenceEqual(expected.Colors))
            throw new InvalidDataException("Extracted title palette differs from cartridge CGRAM data.");

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
        Console.WriteLine(
            $"  Title palette presentation: {SnesCgram.ColorCount} editable colors match ROM; " +
            $"production avoided {paletteAddresses.Count} cartridge source bytes.");
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

        string unknownField = System.Text.Encoding.UTF8.GetString(extracted)
            .Replace("\"version\": 1", "\"version\": 1,\n  \"nativeAddress\": 9232873", StringComparison.Ordinal);
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
