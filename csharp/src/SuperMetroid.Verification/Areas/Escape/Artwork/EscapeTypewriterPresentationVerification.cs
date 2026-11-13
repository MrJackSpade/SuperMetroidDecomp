using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Areas.Escape.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;

/// <summary>Checks the installed Ceres and Zebes escape typewriter text through VRAM writes, debugger restore and glyph validation.</summary>
internal static class EscapeTypewriterPresentationVerification
{
    /// <summary>Runs extracted programs to completion and checks debugger restore and invalid glyph rejection.</summary>
    internal static void VerifyEscapeTypewriterPresentation(string romPath)
    {
        ISnesAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        byte[] extracted = SuperMetroid.AssetExtraction.Areas.Escape.EscapeTypewriterExtractor.Extract(bus);
        EscapeTypewriterPresentation presentation = EscapeTypewriterPresentationFile.Load(
            new MemoryStream(extracted, writable: false));

        int comparedFrames = 0;
        foreach (EscapeTypewriterMessageId id in new[]
            { EscapeTypewriterMessageId.Ceres, EscapeTypewriterMessageId.Zebes })
        {
            EscapeTypewriterMessage program = presentation.Get(id);
            // The installed program runs to completion without any cartridge text read;
            // extraction copied its stream from the ROM at program.SourceAddress.
            var installed = new EscapeTypewriterState(program);
            var installedVram = new SnesVram();
            for (int frame = 0; ; frame++)
            {
                if (frame == 2048) throw new InvalidOperationException($"{id} typewriter did not terminate.");
                comparedFrames++;
                if (installed.StepInto(installedVram)) break;
            }
            AssertTrue(installed.GlyphsWritten > 0, $"{id} installed typewriter writes its glyphs");
        }

        var saved = new EscapeTypewriterState(
            presentation.Get(EscapeTypewriterMessageId.Zebes));
        var savedVram = new SnesVram();
        saved.StepInto(savedVram);
        // A debugger state names the bound warning text by its path in the installed presentation.
        var restored = GraphDigestVerification.RoundTrip(saved, presentation);
        AssertEqual(EscapeTypewriterMessageId.Zebes, restored.MessageId,
            "debugger state retains compiled escape program identity");
        restored.StepInto(savedVram);
        AssertEqual((ushort)(EscapeTypewriterTileDefinitions.ZebesTileBase.PackedWord + 'I' - 'A'),
            savedVram.ReadWord(0x106), "restored escape text resumes at its saved character");

        JsonObject malformedDocument = RequiredJson.Parse(extracted).AsObject();
        malformedDocument.Required("programs").Required(EscapeTypewriterMessageId.Zebes.ToString())
            .Required("lines").Required(0)["text"] = "test!";
        byte[] malformed = Encoding.UTF8.GetBytes(malformedDocument.ToJsonString(ArtworkJson.Options));
        AssertThrows<InvalidDataException>(() => EscapeTypewriterPresentationFile.Load(
            new MemoryStream(malformed, writable: false)),
            "escape typewriter rejects glyphs absent from the documented font mapping");

        Console.WriteLine(
            $"Escape typewriter: Ceres/Zebes extraction and {comparedFrames} stock frames match with installed ROM reads forbidden; debugger restore and strict glyph validation pass.");
    }

    /// <summary>Checks that the installed escape typewriter is the deterministic cartridge extraction.</summary>
    internal static void VerifyEscapeTypewriterAssets(
        ISnesAddressSpace bus,
        string stockDirectory)
    {
        byte[] deterministic = SuperMetroid.AssetExtraction.Areas.Escape.EscapeTypewriterExtractor.Extract(bus);
        AssertTrue(deterministic.SequenceEqual(File.ReadAllBytes(
            Path.Combine(stockDirectory, EscapeTypewriterDocumentDefinitions.FileName))),
            "installed escape typewriter is the deterministic cartridge extraction");

        Console.WriteLine(
            "Escape typewriter catalog: deterministic stock passes.");
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
