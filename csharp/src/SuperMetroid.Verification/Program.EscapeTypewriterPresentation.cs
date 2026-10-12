using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static Program;
using static RepositoryInputsVerification;
using static SuiteTimingVerification;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Represents escape typewriter presentation verification.</summary>
internal static class EscapeTypewriterPresentationVerification
{
    /// <summary>Verifies escape typewriter presentation.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyEscapeTypewriterPresentation(string romPath)
    {
        ISnesAddressSpace bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
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
            // The installed program runs to completion without any cartridge text read;
            // extraction copied its stream from the ROM at program.SourceAddress.
            var installed = new EscapeTypewriterState(program, tileBase);
            var installedVram = new SnesVram();
            for (int frame = 0; ; frame++)
            {
                if (frame == 2048) throw new InvalidOperationException($"{id} typewriter did not terminate.");
                comparedFrames++;
                if (installed.Step(installedVram)) break;
            }
            AssertTrue(installed.GlyphsWritten > 0, $"{id} installed typewriter writes its glyphs");
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
        editedState.Step(editedVram);
        editedState.Step(editedVram);
        AssertEqual((ushort)(EscapeTypewriterRomData.ZebesTileBase + 'E' - 'A'),
            editedVram.ReadWord(0x4906), "edited UTF-8 escape text reaches live VRAM");

        var rebound = new EscapeTypewriterState(
            presentation.Get(EscapeTypewriterProgramId.Zebes), EscapeTypewriterRomData.ZebesTileBase);
        var reboundVram = new SnesVram();
        rebound.Step(reboundVram);
        rebound.BindProgram(edited.Get(EscapeTypewriterProgramId.Zebes));
        rebound.Step(reboundVram);
        AssertEqual((ushort)(EscapeTypewriterRomData.ZebesTileBase + 'E' - 'A'),
            reboundVram.ReadWord(0x4906), "active escape typewriter accepts current replacement content");

        var saved = new EscapeTypewriterState(
            presentation.Get(EscapeTypewriterProgramId.Zebes), EscapeTypewriterRomData.ZebesTileBase);
        var savedVram = new SnesVram();
        saved.Step(savedVram);
        using var snapshot = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(snapshot, saved);
        snapshot.Position = 0;
        var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer
            .Deserialize<EscapeTypewriterState>(snapshot);
        AssertEqual(EscapeTypewriterProgramId.Zebes, restored.ProgramId,
            "debugger state retains compiled escape program identity");
        AssertThrows<InvalidOperationException>(() =>
            restored.Step(new SnesVram()),
            "restored escape text fails loudly until host content is rebound");
        restored.BindProgram(presentation.Get(EscapeTypewriterProgramId.Zebes));
        restored.Step(savedVram);
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

    /// <summary>Verifies escape typewriter assets.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyEscapeTypewriterAssets(
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
        state.Step(vram);
        state.Step(vram);
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

    /// <summary>Escape typewriter presentation command.</summary>
    [VerificationCommand("--escape-typewriter-presentation")]
    internal static int EscapeTypewriterPresentationCommand()
    {
        string escapeTextRom = RepositoryRomPath;
        Suite(nameof(VerifyEscapeTypewriterPresentation), () => VerifyEscapeTypewriterPresentation(escapeTextRom));
        return 0;
    }
}
