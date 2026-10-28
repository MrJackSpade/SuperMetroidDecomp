using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Areas.Escape.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Checks editable Ceres and Zebes escape typewriter text through installed VRAM writes and state rebinding.</summary>
internal static class EscapeTypewriterPresentationVerification
{
    /// <summary>Runs extracted programs to completion and checks live edits, debugger restore, and invalid glyph rejection.</summary>
    internal static void VerifyEscapeTypewriterPresentation(string romPath)
    {
        ISnesAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        byte[] extracted = SuperMetroid.AssetExtraction.Areas.Escape.EscapeTypewriterExtractor.Extract(bus);
        EscapeTypewriterPresentation presentation = EscapeTypewriterPresentation.Load(
            new MemoryStream(extracted, writable: false));

        int comparedFrames = 0;
        foreach (EscapeTypewriterProgramId id in new[]
            { EscapeTypewriterProgramId.Ceres, EscapeTypewriterProgramId.Zebes })
        {
            EscapeTypewriterProgram program = presentation.Get(id);
            ushort tileBase = id == EscapeTypewriterProgramId.Ceres ? (ushort)0x3582 :
                EscapeTypewriterDefinitions.ZebesTileBase.PackedWord;
            // The installed program runs to completion without any cartridge text read;
            // extraction copied its stream from the ROM at program.SourceAddress.
            var installed = new EscapeTypewriterState(program, BackgroundTileWordBytes.Tile(tileBase));
            var installedVram = new SnesVram();
            for (int frame = 0; ; frame++)
            {
                if (frame == 2048) throw new InvalidOperationException($"{id} typewriter did not terminate.");
                comparedFrames++;
                if (installed.StepInto(installedVram)) break;
            }
            AssertTrue(installed.GlyphsWritten > 0, $"{id} installed typewriter writes its glyphs");
        }

        JsonObject editedDocument = RequiredJson.Parse(extracted).AsObject();
        JsonObject zebes = editedDocument.Required("programs").Required(EscapeTypewriterProgramId.Zebes.ToString()).AsObject();
        zebes.Required("lines").Required(0)["text"] = "TEST!";
        byte[] editedBytes = Encoding.UTF8.GetBytes(editedDocument.ToJsonString(ArtworkJson.Options));
        EscapeTypewriterPresentation edited = EscapeTypewriterPresentation.Load(
            new MemoryStream(editedBytes, writable: false));
        var editedState = new EscapeTypewriterState(
            edited.Get(EscapeTypewriterProgramId.Zebes), EscapeTypewriterDefinitions.ZebesTileBase);
        var editedVram = new SnesVram();
        editedState.StepInto(editedVram);
        editedState.StepInto(editedVram);
        AssertEqual((ushort)(EscapeTypewriterDefinitions.ZebesTileBase.PackedWord + 'E' - 'A'),
            editedVram.ReadWord(0x106), "edited UTF-8 escape text reaches live VRAM");

        var rebound = new EscapeTypewriterState(
            presentation.Get(EscapeTypewriterProgramId.Zebes), EscapeTypewriterDefinitions.ZebesTileBase);
        var reboundVram = new SnesVram();
        rebound.StepInto(reboundVram);
        rebound.BindProgram(edited.Get(EscapeTypewriterProgramId.Zebes));
        rebound.StepInto(reboundVram);
        AssertEqual((ushort)(EscapeTypewriterDefinitions.ZebesTileBase.PackedWord + 'E' - 'A'),
            reboundVram.ReadWord(0x106), "active escape typewriter accepts current replacement content");

        var saved = new EscapeTypewriterState(
            presentation.Get(EscapeTypewriterProgramId.Zebes), EscapeTypewriterDefinitions.ZebesTileBase);
        var savedVram = new SnesVram();
        saved.StepInto(savedVram);
        using var snapshot = new MemoryStream();
        SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Serialize(snapshot, saved);
        snapshot.Position = 0;
        var restored = SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer
            .Deserialize<EscapeTypewriterState>(snapshot);
        AssertEqual(EscapeTypewriterProgramId.Zebes, restored.ProgramId,
            "debugger state retains compiled escape program identity");
        AssertThrows<InvalidOperationException>(() =>
            restored.StepInto(new SnesVram()),
            "restored escape text fails loudly until host content is rebound");
        restored.BindProgram(presentation.Get(EscapeTypewriterProgramId.Zebes));
        restored.StepInto(savedVram);
        AssertEqual((ushort)(EscapeTypewriterDefinitions.ZebesTileBase.PackedWord + 'I' - 'A'),
            savedVram.ReadWord(0x106), "restored escape text resumes at its saved character");

        byte[] malformed = Encoding.UTF8.GetBytes(
            editedDocument.ToJsonString(ArtworkJson.Options).Replace("TEST!", "test!"));
        AssertThrows<InvalidDataException>(() => EscapeTypewriterPresentation.Load(
            new MemoryStream(malformed, writable: false)),
            "escape typewriter rejects glyphs absent from the documented font mapping");

        Console.WriteLine(
            $"Escape typewriter: Ceres/Zebes extraction and {comparedFrames} stock frames match with installed ROM reads forbidden; UTF-8 edits, active rebind and strict glyph validation pass.");
    }

    /// <summary>Checks deterministic stock extraction and that a catalog override changes rendered text and content identity.</summary>
    internal static void VerifyEscapeTypewriterAssets(
        ISnesAddressSpace bus,
        string stockDirectory,
        string overrideDirectory,
        AreaMapPresentation stockCatalog)
    {
        byte[] deterministic = SuperMetroid.AssetExtraction.Areas.Escape.EscapeTypewriterExtractor.Extract(bus);
        AssertTrue(deterministic.SequenceEqual(File.ReadAllBytes(
            Path.Combine(stockDirectory, EscapeTypewriterDefinitions.FileName))),
            "installed escape typewriter is the deterministic cartridge extraction");

        Directory.CreateDirectory(overrideDirectory);
        JsonObject document = RequiredJson.Parse(deterministic).AsObject();
        document.Required("programs").Required(EscapeTypewriterProgramId.Zebes.ToString()).Required("lines").Required(0)["text"] =
            "TEST!";
        string replacement = Path.Combine(overrideDirectory, EscapeTypewriterDefinitions.FileName);
        File.WriteAllText(replacement, document.ToJsonString(ArtworkJson.Options));
        AreaMapPresentation edited = MapPresentationFiles.Load(stockDirectory, overrideDirectory);
        var state = new EscapeTypewriterState(
            edited.EscapeTypewriter.Get(EscapeTypewriterProgramId.Zebes),
            EscapeTypewriterDefinitions.ZebesTileBase);
        var vram = new SnesVram();
        state.StepInto(vram);
        state.StepInto(vram);
        AssertEqual((ushort)(EscapeTypewriterDefinitions.ZebesTileBase.PackedWord + 'E' - 'A'),
            vram.ReadWord(0x106), "catalog override changes installed escape text");
        AssertTrue(stockCatalog.ContentIdentity != edited.ContentIdentity,
            "escape text override changes catalog content identity");

        File.WriteAllText(replacement, "{ broken escape text");
        AssertThrows<InvalidDataException>(() =>
            MapPresentationFiles.Load(stockDirectory, overrideDirectory),
            "corrupt escape text override fails loudly");
        Console.WriteLine(
            "Escape typewriter catalog: deterministic stock, override selection/identity and corruption failure pass.");
    }

    /// <summary>Runs the focused escape typewriter comparison against the repository ROM.</summary>
    [VerificationCommand("--escape-typewriter-presentation")]
    internal static int EscapeTypewriterPresentationCommand()
    {
        string escapeTextRom = RepositoryRomPath;
        Suite(nameof(VerifyEscapeTypewriterPresentation), () => VerifyEscapeTypewriterPresentation(escapeTextRom));
        return 0;
    }
}
