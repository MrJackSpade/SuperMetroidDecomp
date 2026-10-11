using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks every registered narration page's native begin callback, then its extraction and compilation.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the intro narration pages' bank-$8C begin callbacks")]
    private static void VerifyIntroNarrationNativePages(CartridgeImportAddressSpace rom)
    {
        var narrationSources = IntroNarrationDefinitions.Pages.ToArray();
        AssertEqual(6, narrationSources.Length, "narration native registry has six pages");
        byte[] narrationJson = IntroNarrationExtractor.Extract(rom);
        var narration = IntroNarrationPresentation.Load(new MemoryStream(narrationJson, writable: false));
        for (int page = 0; page < narrationSources.Length; page++)
        {
            IntroNarrationNativePage source = narrationSources[page];
            AssertEqual((IntroNarrationPageId)(page + 1), source.Id, "narration native registry order");
            int address = 0x8c0000 | source.InstructionPointer;
            AssertEqual(source.BeginOpcode, (ushort)(rom.ReadCartridgeByte(address) | rom.ReadCartridgeByte(address + 1) << 8), "narration native begin callback");
            AssertTrue(narration.Compile(source.Id).Length > 0, "native narration extracts and compiles every page through finish callback");
        }
    }

    /// <summary>Compares the compiled Mochtroid shake offsets with the native $A3:A76D/$A3:A775 tables for masked timers.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Mochtroid shake offset tables")]
    private static void VerifyMochtroidShakeOffsets(CartridgeImportAddressSpace rom)
    {
        foreach (ushort timer in new ushort[] { 0, 1, 2, 3, 4, 5, 6, 7, 0xfff8, 0xfff9, 0xfffa, 0xfffb, 0xfffc, 0xfffd, 0xfffe, 0xffff })
        {
            int offset = timer & 6;
            short x = unchecked((short)(rom.ReadCartridgeByte(0xa3a76d + offset) | rom.ReadCartridgeByte(0xa3a76e + offset) << 8));
            short y = unchecked((short)(rom.ReadCartridgeByte(0xa3a775 + offset) | rom.ReadCartridgeByte(0xa3a776 + offset) << 8));
            AssertEqual(((int)x, (int)y), MochtroidShakeDefinitions.Offset(timer), "Native cardinal shake and timer-bit mask");
        }
    }

    /// <summary>Compares the registered intro caret's part count with its native $8C:8D68 spritemap.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the intro caret spritemap part count")]
    private static void VerifyIntroCaretNativePartCount(CartridgeImportAddressSpace rom) =>
        AssertEqual((int)(rom.ReadCartridgeByte(0x8c8d68) | rom.ReadCartridgeByte(0x8c8d69) << 8), IntroCaretSpriteDefinitions.Visible.StockPartCount, "Native caret part count");

    /// <summary>Reads one little-endian retail word for the phase-algorithm lookups.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle words for the combo power-bomb cost and origin-angle algorithms")]
    private static ushort ReadPhaseAlgorithmWord(CartridgeImportAddressSpace rom, int address) =>
        (ushort)(rom.ReadCartridgeByte(address) | rom.ReadCartridgeByte(address + 1) << 8);
}
