using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static MochtroidInstructionProgramDefinitionsVerification;
using static Program;
using static SuiteTimingVerification;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static class LookupRomOraclesVerification
{
    /// <summary>Checks every registered narration page's native begin callback, then its extraction and compilation.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the intro narration pages' bank-$8C begin callbacks")]
    internal static void VerifyIntroNarrationNativePages(CartridgeImportAddressSpace rom)
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
            AssertEqual(source.BeginOpcode, rom.ReadCartridgeWord(address), "narration native begin callback");
            AssertTrue(narration.Compile(source.Id).Length > 0, "native narration extracts and compiles every page through finish callback");
        }
    }

    /// <summary>Compares the compiled Mochtroid shake offsets with the native $A3:A76D/$A3:A775 tables for masked timers.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Mochtroid shake offset tables")]
    internal static void VerifyMochtroidShakeOffsets(CartridgeImportAddressSpace rom)
    {
        foreach (ushort timer in new ushort[] { 0, 1, 2, 3, 4, 5, 6, 7, 0xfff8, 0xfff9, 0xfffa, 0xfffb, 0xfffc, 0xfffd, 0xfffe, 0xffff })
        {
            int offset = timer & 6;
            short x = unchecked((short)rom.ReadCartridgeWord(0xa3a76d + offset));
            short y = unchecked((short)rom.ReadCartridgeWord(0xa3a775 + offset));
            AssertEqual(((int)x, (int)y), MochtroidShakeDefinitions.Offset(timer), "Native cardinal shake and timer-bit mask");
        }
    }

    /// <summary>Compares the registered intro caret's part count with its native $8C:8D68 spritemap.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the intro caret spritemap part count")]
    internal static void VerifyIntroCaretNativePartCount(CartridgeImportAddressSpace rom) =>
        AssertEqual((int)rom.ReadCartridgeWord(0x8c8d68), IntroCaretSpriteDefinitions.Visible.StockPartCount, "Native caret part count");

    /// <summary>Reads one little-endian retail word for the phase-algorithm lookups.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle words for the combo power-bomb cost and origin-angle algorithms")]
    internal static ushort ReadPhaseAlgorithmWord(CartridgeImportAddressSpace rom, int address) =>
        rom.ReadCartridgeWord(address);

    [VerificationCommand("--lookup-intro-narration-registry")]
    internal static int LookupIntroNarrationRegistryCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            VerifyIntroNarrationNativePages(rom);
            AssertThrows<ArgumentOutOfRangeException>(() => IntroNarrationDefinitions.NativePage((IntroNarrationPageId)0), "narration registry lower bound");
            AssertThrows<ArgumentOutOfRangeException>(() => IntroNarrationDefinitions.NativePage((IntroNarrationPageId)7), "narration registry upper bound");
        Console.WriteLine("Narration registry: six native page boundaries, extraction, compilation, order and bounds pass.");
        return 0;
    }

    [VerificationCommand("--lookup-mochtroid-shake")]
    internal static int LookupMochtroidShakeCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        VerifyMochtroidShakeOffsets(rom);
        Suite(nameof(VerifyMochtroidInstructionProgramDefinitions), () => VerifyMochtroidInstructionProgramDefinitions(rom));
        Console.WriteLine("Mochtroid shake: sixteen native masked-timer comparisons pass; amplitude remains required.");
        return 0;
    }
}
