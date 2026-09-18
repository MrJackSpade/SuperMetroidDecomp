using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyIntroNarrationPresentation(string romPath)
    {
        ISnesAddressSpace nativeBus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        byte[] extracted = SuperMetroid.AssetExtraction.IntroNarrationExtractor.Extract(nativeBus);
        IntroNarrationPresentation presentation = IntroNarrationPresentation.Load(
            new MemoryStream(extracted, writable: false));
        ISnesAddressSpace installedBus = new IntroNarrationReadGuard(nativeBus);

        byte[] fontPng = SuperMetroid.AssetExtraction.IntroFontAtlasExtractor.Extract(nativeBus);
        IntroFontAtlas font = IntroFontAtlas.Load(new MemoryStream(fontPng, writable: false));
        byte[] nativeFont = RomDataReader.Decompress(
            nativeBus,
            IntroCinematicRomData.Assets.FontOne,
            maximumOutputBytes: IntroCinematicRomData.Vram.FontOneBytes);
        AssertTrue(font.Transfer.Span.SequenceEqual(
            nativeFont.AsSpan(0, IntroFontAtlasFormat.ByteCount)),
            "installed opening-font PNG compiles to exact native planar bytes");
        var installedIntro = new IntroCinematicState(
            new IntroFontReadGuard(nativeBus), introFont: font);
        AssertTrue(installedIntro.CaptureTranslatedRenderSnapshot().Memory.Vram.Slice(
            IntroCinematicRomData.Vram.FontOneDestinationByte,
            IntroFontAtlasFormat.ByteCount).SequenceEqual(font.Transfer.Span),
            "opening cinematic uploads the installed font without reading its ROM stream");

        int comparedFrames = 0;
        foreach (IntroNarrationPageId page in Enum.GetValues<IntroNarrationPageId>())
        {
            ushort[] nativeTilemap = CreateBlankIntroTilemap();
            ushort[] installedTilemap = CreateBlankIntroTilemap();
            var native = new IntroCinematicObjectSystem(
                nativeBus, new SnesVram(), nativeTilemap);
            var installed = new IntroCinematicObjectSystem(
                installedBus, new SnesVram(), installedTilemap,
                narrationPresentation: presentation);
            StartPage(native, page);
            StartPage(installed, page);

            for (int frame = 0; frame < 2048; frame++)
            {
                native.Step();
                installed.Step();
                AssertTrue(nativeTilemap.AsSpan().SequenceEqual(installedTilemap),
                    $"installed opening narration {page} tilemap frame {frame}");
                AssertEqual(native.CaretX, installed.CaretX,
                    $"installed opening narration {page} caret X frame {frame}");
                AssertEqual(native.CaretY, installed.CaretY,
                    $"installed opening narration {page} caret Y frame {frame}");
                AssertEqual(IsPageComplete(native, page), IsPageComplete(installed, page),
                    $"installed opening narration {page} completion frame {frame}");
                comparedFrames++;
                if (IsPageComplete(native, page))
                    break;
                if (frame == 2047)
                    throw new InvalidOperationException($"Opening narration {page} did not terminate.");
            }
        }

        JsonObject editedDocument = JsonNode.Parse(extracted)!.AsObject();
        editedDocument["pages"]![IntroNarrationPageId.Page6.ToString()]!["lines"]![0]!["text"] =
            "TEST";
        byte[] editedBytes = Encoding.UTF8.GetBytes(
            editedDocument.ToJsonString(MapPresentationFormat.JsonOptions));
        IntroNarrationPresentation edited = IntroNarrationPresentation.Load(
            new MemoryStream(editedBytes, writable: false));
        ushort[] editedTilemap = CreateBlankIntroTilemap();
        var editedState = new IntroCinematicObjectSystem(
            installedBus, new SnesVram(), editedTilemap,
            narrationPresentation: edited);
        editedState.StartEnglishPageSix();
        editedState.Step();
        editedState.Step();
        AssertEqual(IntroNarrationDefinitions.CompileGlyph('T'),
            editedTilemap[4 * IntroCinematicRomData.Layers.TilemapWidth + 1],
            "edited UTF-8 opening narration reaches the live tilemap");

        ushort[] reboundTilemap = CreateBlankIntroTilemap();
        var rebound = new IntroCinematicObjectSystem(
            installedBus, new SnesVram(), reboundTilemap,
            narrationPresentation: presentation);
        rebound.StartEnglishPageSix();
        rebound.Step();
        rebound.BindNarration(edited);
        rebound.Step();
        AssertEqual(IntroNarrationDefinitions.CompileGlyph('T'),
            reboundTilemap[4 * IntroCinematicRomData.Layers.TilemapWidth + 1],
            "active opening narration accepts current replacement content");

        var saved = new IntroCinematicObjectSystem(
            nativeBus, new SnesVram(), CreateBlankIntroTilemap(),
            narrationPresentation: presentation);
        saved.StartEnglishPageSix();
        saved.Step();
        using var snapshot = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(snapshot, saved);
        snapshot.Position = 0;
        var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer
            .Deserialize<IntroCinematicObjectSystem>(snapshot);
        AssertThrows<InvalidOperationException>(() => restored.Step(),
            "restored opening narration fails loudly until host content is rebound");
        restored.BindNarration(presentation);
        restored.Step();
        AssertEqual((ushort)16, restored.CaretX,
            "restored opening narration resumes at its saved character");

        editedDocument["pages"]![IntroNarrationPageId.Page6.ToString()]!["lines"]![0]!["text"] =
            "lowercase";
        byte[] malformed = Encoding.UTF8.GetBytes(
            editedDocument.ToJsonString(MapPresentationFormat.JsonOptions));
        AssertThrows<InvalidDataException>(() => IntroNarrationPresentation.Load(
            new MemoryStream(malformed, writable: false)),
            "opening narration rejects glyphs absent from the documented font mapping");

        IndexedPngImage editableFont = IndexedPng.Read(
            new MemoryStream(fontPng, writable: false),
            IntroFontAtlasFormat.Width, IntroFontAtlasFormat.Height);
        editableFont.Pixels[0] ^= 1;
        using var editedFontPng = new MemoryStream();
        IndexedPng.Write(editedFontPng, editableFont.Width, editableFont.Height,
            editableFont.Pixels, editableFont.Palette);
        editedFontPng.Position = 0;
        IntroFontAtlas editedFont = IntroFontAtlas.Load(editedFontPng);
        installedIntro.BindIntroFont(editedFont);
        AssertTrue(installedIntro.CaptureTranslatedRenderSnapshot().Memory.Vram.Slice(
            IntroCinematicRomData.Vram.FontOneDestinationByte,
            IntroFontAtlasFormat.ByteCount).SequenceEqual(editedFont.Transfer.Span),
            "active opening cinematic rebinds edited font pixels");

        Console.WriteLine(
            $"Opening narration: six extracted UTF-8 pages and {comparedFrames} stock frames match with narration ROM reads forbidden; caret timing, page completion, edits and glyph validation pass.");
        Console.WriteLine(
            "Opening font: 144 PNG tiles compile to exact native 2-bpp bytes; installed upload, active pixel edit and ROM-read guard pass.");
    }

    private static ushort[] CreateBlankIntroTilemap()
    {
        var result = new ushort[IntroCinematicRomData.Layers.TextTilemapWordCount];
        Array.Fill(result, IntroCinematicRomData.Text.Blank.Raw);
        return result;
    }

    private static void StartPage(IntroCinematicObjectSystem state, IntroNarrationPageId page)
    {
        switch (page)
        {
            case IntroNarrationPageId.Page1: state.StartEnglishPageOne(); break;
            case IntroNarrationPageId.Page2: state.StartEnglishPageTwo(); break;
            case IntroNarrationPageId.Page3: state.StartEnglishPageThree(); break;
            case IntroNarrationPageId.Page4: state.StartEnglishPageFour(); break;
            case IntroNarrationPageId.Page5: state.StartEnglishPageFive(); break;
            case IntroNarrationPageId.Page6: state.StartEnglishPageSix(); break;
            default: throw new ArgumentOutOfRangeException(nameof(page), page, null);
        }
    }

    private static bool IsPageComplete(
        IntroCinematicObjectSystem state,
        IntroNarrationPageId page) => page switch
    {
        IntroNarrationPageId.Page1 => state.PageOneAwaitingInput,
        IntroNarrationPageId.Page2 => state.PageTwoAwaitingInput,
        IntroNarrationPageId.Page3 => state.PageThreeAwaitingInput,
        IntroNarrationPageId.Page4 => state.PageFourAwaitingInput,
        IntroNarrationPageId.Page5 => state.PageFiveAwaitingInput,
        IntroNarrationPageId.Page6 => state.IntroFinishRequested,
        _ => throw new ArgumentOutOfRangeException(nameof(page), page, null),
    };

    private static void VerifyIntroNarrationAssets(
        ISnesAddressSpace bus,
        string stockDirectory,
        string overrideDirectory,
        AreaMapPresentationCatalog stockCatalog)
    {
        byte[] deterministic = SuperMetroid.AssetExtraction.IntroNarrationExtractor.Extract(bus);
        AssertTrue(deterministic.AsSpan().SequenceEqual(File.ReadAllBytes(
            Path.Combine(stockDirectory, IntroNarrationDefinitions.FileName))),
            "installed opening narration is the deterministic cartridge extraction");

        Directory.CreateDirectory(overrideDirectory);
        JsonObject document = JsonNode.Parse(deterministic)!.AsObject();
        document["pages"]![IntroNarrationPageId.Page1.ToString()]!["lines"]![0]!["text"] =
            "TEST";
        string replacement = Path.Combine(
            overrideDirectory, IntroNarrationDefinitions.FileName);
        File.WriteAllText(replacement,
            document.ToJsonString(MapPresentationFormat.JsonOptions));
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(
            stockDirectory, overrideDirectory);
        AssertEqual(IntroNarrationDefinitions.CompileGlyph('T'),
            edited.IntroNarration.Compile(IntroNarrationPageId.Page1)[0].TilemapWord,
            "catalog override changes installed opening narration");
        AssertTrue(stockCatalog.ContentIdentity != edited.ContentIdentity,
            "opening-narration override changes catalog content identity");

        File.WriteAllText(replacement, "{ broken opening narration");
        AssertThrows<InvalidDataException>(() =>
            AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory),
            "corrupt opening-narration override fails loudly");
        Console.WriteLine(
            "Opening-narration catalog: deterministic stock, override selection/identity and corruption failure pass.");
    }

    private static void VerifyIntroFontAssets(
        ISnesAddressSpace bus,
        string stockDirectory,
        string overrideDirectory,
        AreaMapPresentationCatalog stockCatalog)
    {
        byte[] deterministic = SuperMetroid.AssetExtraction.IntroFontAtlasExtractor.Extract(bus);
        AssertTrue(deterministic.AsSpan().SequenceEqual(File.ReadAllBytes(
            Path.Combine(stockDirectory, IntroFontAtlasFormat.FileName))),
            "installed opening font is the deterministic cartridge extraction");
        IndexedPngImage image = IndexedPng.Read(
            new MemoryStream(deterministic, writable: false),
            IntroFontAtlasFormat.Width, IntroFontAtlasFormat.Height);
        image.Pixels[0] ^= 1;
        Directory.CreateDirectory(overrideDirectory);
        string replacement = Path.Combine(overrideDirectory, IntroFontAtlasFormat.FileName);
        using (var output = File.Create(replacement))
            IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(
            stockDirectory, overrideDirectory);
        AssertTrue(!edited.IntroFont.Transfer.Span.SequenceEqual(
            stockCatalog.IntroFont.Transfer.Span),
            "opening-font override changes installed VRAM bytes");
        AssertTrue(stockCatalog.ContentIdentity != edited.ContentIdentity,
            "opening-font override changes catalog content identity");
        File.WriteAllText(replacement, "not a PNG");
        AssertThrows<InvalidDataException>(() =>
            AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory),
            "corrupt opening-font override fails loudly");
        Console.WriteLine(
            "Opening-font catalog: deterministic stock, PNG override identity and corruption failure pass.");
    }

    private sealed class IntroNarrationReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            int bank = address >> 16;
            int offset = address & 0xffff;
            if (bank == 0x8c &&
                (offset is >= 0xc383 and <= 0xd5dd or >= 0xd67d and <= 0xd780))
            {
                throw new InvalidOperationException(
                    $"Installed opening narration read cartridge text address ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private sealed class IntroFontReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address is >= 0x95d089 and < 0x95d713)
            {
                throw new InvalidOperationException(
                    $"Installed opening font read cartridge address ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
