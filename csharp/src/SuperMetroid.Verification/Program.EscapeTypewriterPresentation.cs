using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyEscapeTypewriterPresentation(string romPath)
    {
        ISnesAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        byte[] extracted = SuperMetroid.AssetExtraction.EscapeTypewriterExtractor.Extract(bus);
        EscapeTypewriterPresentation presentation = EscapeTypewriterPresentation.Load(
            new MemoryStream(extracted, writable: false));

        int comparedFrames = 0;
        foreach (EscapeTypewriterProgramId id in new[]
            { EscapeTypewriterProgramId.Ceres, EscapeTypewriterProgramId.Zebes })
        {
            EscapeTypewriterProgram program = presentation.Get(id);
            ushort tileBase = id == EscapeTypewriterProgramId.Ceres ? (ushort)0x3582 :
                EscapeTypewriterRomData.ZebesTileBase;
            var native = new EscapeTypewriterState(program.SourceAddress, tileBase);
            var installed = new EscapeTypewriterState(program, tileBase);
            var nativeVram = new SnesVram();
            var installedVram = new SnesVram();
            for (int frame = 0; frame < 2048; frame++)
            {
                bool nativeDone = native.Step(bus, nativeVram);
                bool installedDone = installed.Step(new ForbiddenEscapeTextBus(), installedVram);
                AssertEqual(nativeDone, installedDone, $"{id} installed completion frame {frame}");
                AssertEqual(native.Destination, installed.Destination, $"{id} installed destination frame {frame}");
                AssertEqual(native.Delay, installed.Delay, $"{id} installed delay frame {frame}");
                AssertEqual(native.DelayTimer, installed.DelayTimer, $"{id} installed delay timer frame {frame}");
                AssertEqual(native.GlyphsWritten, installed.GlyphsWritten, $"{id} installed glyph count frame {frame}");
                AssertEqual(native.ClickRequested, installed.ClickRequested, $"{id} installed click frame {frame}");
                AssertTrue(nativeVram.Bytes.SequenceEqual(installedVram.Bytes),
                    $"{id} installed VRAM frame {frame}");
                comparedFrames++;
                if (nativeDone) break;
                if (frame == 2047) throw new InvalidOperationException($"{id} typewriter did not terminate.");
            }
        }

        JsonObject editedDocument = JsonNode.Parse(extracted)!.AsObject();
        JsonObject zebes = editedDocument["programs"]![EscapeTypewriterProgramId.Zebes.ToString()]!.AsObject();
        zebes["lines"]![0]!["text"] = "TEST!";
        byte[] editedBytes = Encoding.UTF8.GetBytes(editedDocument.ToJsonString(MapPresentationFormat.JsonOptions));
        EscapeTypewriterPresentation edited = EscapeTypewriterPresentation.Load(
            new MemoryStream(editedBytes, writable: false));
        var editedState = new EscapeTypewriterState(
            edited.Get(EscapeTypewriterProgramId.Zebes), EscapeTypewriterRomData.ZebesTileBase);
        var editedVram = new SnesVram();
        editedState.Step(new ForbiddenEscapeTextBus(), editedVram);
        editedState.Step(new ForbiddenEscapeTextBus(), editedVram);
        AssertEqual((ushort)(EscapeTypewriterRomData.ZebesTileBase + 'E' - 'A'),
            editedVram.ReadWord(0x4906), "edited UTF-8 escape text reaches live VRAM");

        var rebound = new EscapeTypewriterState(
            presentation.Get(EscapeTypewriterProgramId.Zebes), EscapeTypewriterRomData.ZebesTileBase);
        var reboundVram = new SnesVram();
        rebound.Step(new ForbiddenEscapeTextBus(), reboundVram);
        rebound.BindProgram(edited.Get(EscapeTypewriterProgramId.Zebes));
        rebound.Step(new ForbiddenEscapeTextBus(), reboundVram);
        AssertEqual((ushort)(EscapeTypewriterRomData.ZebesTileBase + 'E' - 'A'),
            reboundVram.ReadWord(0x4906), "active escape typewriter accepts current replacement content");

        var saved = new EscapeTypewriterState(
            presentation.Get(EscapeTypewriterProgramId.Zebes), EscapeTypewriterRomData.ZebesTileBase);
        var savedVram = new SnesVram();
        saved.Step(new ForbiddenEscapeTextBus(), savedVram);
        using var snapshot = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(snapshot, saved);
        snapshot.Position = 0;
        var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer
            .Deserialize<EscapeTypewriterState>(snapshot);
        AssertEqual(EscapeTypewriterProgramId.Zebes, restored.ProgramId,
            "debugger state retains compiled escape program identity");
        AssertThrows<InvalidOperationException>(() =>
            restored.Step(new ForbiddenEscapeTextBus(), new SnesVram()),
            "restored escape text fails loudly until host content is rebound");
        restored.BindProgram(presentation.Get(EscapeTypewriterProgramId.Zebes));
        restored.Step(new ForbiddenEscapeTextBus(), savedVram);
        AssertEqual((ushort)(EscapeTypewriterRomData.ZebesTileBase + 'I' - 'A'),
            savedVram.ReadWord(0x4906), "restored escape text resumes at its saved character");

        byte[] malformed = Encoding.UTF8.GetBytes(
            editedDocument.ToJsonString(MapPresentationFormat.JsonOptions).Replace("TEST!", "test!"));
        AssertThrows<InvalidDataException>(() => EscapeTypewriterPresentation.Load(
            new MemoryStream(malformed, writable: false)),
            "escape typewriter rejects glyphs absent from the documented font mapping");

        Console.WriteLine(
            $"Escape typewriter: Ceres/Zebes extraction and {comparedFrames} stock frames match with installed ROM reads forbidden; UTF-8 edits, active rebind and strict glyph validation pass.");
    }

    private static void VerifyEscapeTypewriterAssets(
        ISnesAddressSpace bus,
        string stockDirectory,
        string overrideDirectory,
        AreaMapPresentationCatalog stockCatalog)
    {
        byte[] deterministic = SuperMetroid.AssetExtraction.EscapeTypewriterExtractor.Extract(bus);
        AssertTrue(deterministic.AsSpan().SequenceEqual(File.ReadAllBytes(
            Path.Combine(stockDirectory, EscapeTypewriterDefinitions.FileName))),
            "installed escape typewriter is the deterministic cartridge extraction");

        Directory.CreateDirectory(overrideDirectory);
        JsonObject document = JsonNode.Parse(deterministic)!.AsObject();
        document["programs"]![EscapeTypewriterProgramId.Zebes.ToString()]!["lines"]![0]!["text"] =
            "TEST!";
        string replacement = Path.Combine(overrideDirectory, EscapeTypewriterDefinitions.FileName);
        File.WriteAllText(replacement, document.ToJsonString(MapPresentationFormat.JsonOptions));
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory);
        var state = new EscapeTypewriterState(
            edited.EscapeTypewriter.Get(EscapeTypewriterProgramId.Zebes),
            EscapeTypewriterRomData.ZebesTileBase);
        var vram = new SnesVram();
        state.Step(new ForbiddenEscapeTextBus(), vram);
        state.Step(new ForbiddenEscapeTextBus(), vram);
        AssertEqual((ushort)(EscapeTypewriterRomData.ZebesTileBase + 'E' - 'A'),
            vram.ReadWord(0x4906), "catalog override changes installed escape text");
        AssertTrue(stockCatalog.ContentIdentity != edited.ContentIdentity,
            "escape text override changes catalog content identity");

        File.WriteAllText(replacement, "{ broken escape text");
        AssertThrows<InvalidDataException>(() =>
            AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory),
            "corrupt escape text override fails loudly");
        Console.WriteLine(
            "Escape typewriter catalog: deterministic stock, override selection/identity and corruption failure pass.");
    }

    private sealed class ForbiddenEscapeTextBus : ISnesAddressSpace
    {
        public byte ReadByte(int address) => throw new InvalidOperationException(
            $"Installed escape typewriter read cartridge address ${address:X6}.");
        public void WriteByte(int address, byte value) => throw new InvalidOperationException(
            $"Installed escape typewriter wrote cartridge address ${address:X6}.");
    }
}
