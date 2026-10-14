using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>Compares compiled Ceres backdrop instruction bytes and playback with native bank-$8B lists.</summary>
internal static class CeresBackdropProgramsVerification
{
    /// <summary>Checks the seven backdrop lists, including the longer title sequence.</summary>
    internal static void VerifyCeresBackdropPrograms(ISnesAddressSpace rom)
    {
        Suite(nameof(VerifyCeresBackdropProgram), () => VerifyCeresBackdropProgram(rom, 0xcc3f, 0xcc47, false));
        Suite(nameof(VerifyCeresBackdropProgram), () => VerifyCeresBackdropProgram(rom, 0xccab, 0xccb3, false));
        Suite(nameof(VerifyCeresBackdropProgram), () => VerifyCeresBackdropProgram(rom, 0xccbb, 0xccd5, true));
        Suite(nameof(VerifyCeresBackdropProgram), () => VerifyCeresBackdropProgram(rom, 0xcd83, 0xcd8b, false));
        Suite(nameof(VerifyCeresBackdropProgram), () => VerifyCeresBackdropProgram(rom, 0xcd8b, 0xcd93, false));
        Suite(nameof(VerifyCeresBackdropProgram), () => VerifyCeresBackdropProgram(rom, 0xcd93, 0xcd9b, false));
        Suite(nameof(VerifyCeresBackdropProgram), () => VerifyCeresBackdropProgram(rom, 0xcd9b, 0xcda3, false));
    }

    /// <summary>Checks byte and word lookup boundaries plus frame-by-frame cursor, visibility, lifetime, and callback behavior.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres backdrop program")]
    private static void VerifyCeresBackdropProgram(ISnesAddressSpace rom, ushort start, ushort end, bool title)
    {
        ushort OriginalWord(ushort p) => CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x8b0000 | p);
        for (int p = start; p < end; p++)
        {
            AssertEqual(rom.ReadCartridgeByte(0x8b0000 | p), CeresDestructionSpriteInstructionDefinitions.ReadByte((ushort)p), "original backdrop program byte");
            if (p + 1 < end)
                AssertEqual(OriginalWord((ushort)p), CeresDestructionSpriteInstructionDefinitions.ReadWord((ushort)p), "original aligned or unaligned backdrop word");
        }
        AssertThrows<InvalidDataException>(() => CeresDestructionSpriteInstructionDefinitions.ReadWord((ushort)(end - 1)), "word cannot cross backdrop list boundary");
        var native = new IntroDiscoverySprite(new(0, 0), 0, start);
        var compiled = new IntroDiscoverySprite(new(0, 0), 0, start);
        var nativeCalls = new List<(ushort Opcode, ushort Cursor)>();
        var compiledCalls = new List<(ushort Opcode, ushort Cursor)>();
        for (int frame = 0; frame < (title ? 400 : 32); frame++)
        {
            nativeCalls.Clear();
            compiledCalls.Clear();
            native.Step((ZebesTitleInstruction opcode, ushort cursor) => { nativeCalls.Add(((ushort)opcode, cursor)); return cursor; }, OriginalWord);
            compiled.Step((ZebesTitleInstruction opcode, ushort cursor) => { compiledCalls.Add(((ushort)opcode, cursor)); return cursor; }, CeresDestructionSpriteInstructionDefinitions.ReadWord);
            AssertEqual(native.InstructionPointer, compiled.InstructionPointer, "backdrop cursor each frame");
            AssertEqual(native.SpriteMapPointer, compiled.SpriteMapPointer, "backdrop visibility each frame");
            AssertEqual(native.IsActive, compiled.IsActive, "backdrop lifetime each frame");
            AssertEqual(nativeCalls.Count, compiledCalls.Count, "title callback count each frame");
            for (int i = 0; i < nativeCalls.Count; i++)
                AssertEqual(nativeCalls[i], compiledCalls[i], "title callback identity and continuation each frame");
        }
        AssertEqual(!title, compiled.IsActive, "static loops persist and title deletes");
    }
}
