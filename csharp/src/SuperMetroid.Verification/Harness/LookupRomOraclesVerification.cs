using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

namespace SuperMetroid.Verification.Harness;

/// <summary>Holds focused retail-ROM oracles for narration and phase data.</summary>
internal static class LookupRomOraclesVerification
{
    /// <summary>Checks every registered narration page's native begin callback, then its extraction and compilation.</summary>
    internal static void VerifyIntroNarrationNativePages(CartridgeImportAddressSpace rom)
    {
        var narrationSources = IntroNarrationScriptSources.Pages.ToArray();
        AssertEqual(6, narrationSources.Length, "narration native registry has six pages");
        byte[] narrationJson = IntroNarrationExtractor.Extract(rom);
        var narration = IntroNarrationFile.Read(new MemoryStream(narrationJson, writable: false));
        IntroNarrationPageId[] storyOrder =
        [
            IntroNarrationPageId.Page1, IntroNarrationPageId.Page2, IntroNarrationPageId.Page3,
            IntroNarrationPageId.Page4, IntroNarrationPageId.Page5, IntroNarrationPageId.Page6,
        ];
        for (int page = 0; page < narrationSources.Length; page++)
        {
            IntroNarrationScriptPage source = narrationSources[page];
            AssertEqual(storyOrder[page], source.Id, "narration native registry order");
            int address = 0x8c0000 | source.InstructionPointer;
            AssertEqual(source.BeginOpcode, (int)rom.ReadCartridgeWord(address), "narration native begin callback");
            AssertTrue(narration.Compile(source.Id).Length > 0, "native narration extracts and compiles every page through finish callback");
        }
    }

    /// <summary>Compares the registered intro caret's part count with its native $8C:8D68 spritemap.</summary>
    internal static void VerifyIntroCaretNativePartCount(CartridgeImportAddressSpace rom) =>
        AssertEqual(rom.ReadCartridgeWord(0x8c8d68), IntroCaretSpriteDefinitions.Visible.StockPartCount, "Native caret part count");

    /// <summary>Reads one little-endian retail word for the phase-algorithm lookups.</summary>
    internal static ushort ReadPhaseAlgorithmWord(CartridgeImportAddressSpace rom, int address) =>
        rom.ReadCartridgeWord(address);

    /// <summary>Checks native narration page callbacks, extracted compilation, order, and page bounds.</summary>
    [VerificationCommand("--lookup-intro-narration-registry")]
    internal static int LookupIntroNarrationRegistryCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            VerifyIntroNarrationNativePages(rom);
            // A value no page identity holds stands for an unknown page.
            AssertThrows<ArgumentOutOfRangeException>(() => IntroNarrationScriptSources.Page((IntroNarrationPageId)(-1)), "narration registry rejects an unknown page");
        Console.WriteLine("Narration registry: six native page boundaries, extraction, compilation, order and unknown-page rejection pass.");
        return 0;
    }
}
