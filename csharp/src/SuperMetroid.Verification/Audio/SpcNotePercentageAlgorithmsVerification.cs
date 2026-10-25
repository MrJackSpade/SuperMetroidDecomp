using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.Verification.Audio;

/// <summary>Checks compiled SPC operand and note-percentage tables against retail bytes.</summary>
internal static class SpcNotePercentageAlgorithmsVerification
{
    /// <summary>Compares operand counts for opcodes $E0-$FE and rejects indices outside the table.</summary>
    internal static void VerifySpcEffectOperandSelection(SuperMetroidAddressSpace rom)
    {
        for (int opcode = 0xe0; opcode <= 0xfe; opcode++)
        {
            int index = opcode - 0xe0;
            AssertEqual(rom.ReadCartridgeByte(SpcMusicTables.EffectLengthReferenceAddress + index),
                SpcMusicTables.EffectByteLength(index), $"Original SPC opcode {opcode:X2} operand count");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => SpcMusicTables.EffectByteLength(-1), "Effect negative index");
        AssertThrows<ArgumentOutOfRangeException>(() => SpcMusicTables.EffectByteLength(31), "Effect upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => SpcMusicTables.EffectByteLength(int.MaxValue), "Effect invalid maximum");
    }

    /// <summary>Checks that all timing and volume commands use the low nibble to select volume.</summary>
    internal static void VerifySpcNoteVolumeAlgorithm(SuperMetroidAddressSpace rom)
    {
        // Every possible timing/volume command byte: the decoder uses its low nibble.
        for (int command = 0; command < 128; command++)
        {
            int index = command & 15;
            AssertEqual(rom.ReadCartridgeByte(SpcMusicTables.NoteVolumeReferenceAddress + index),
                SpcMusicTables.NoteVolume(index), $"Original SPC volume selector {index}");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => SpcMusicTables.NoteVolume(-1), "Volume negative index");
        AssertThrows<ArgumentOutOfRangeException>(() => SpcMusicTables.NoteVolume(16), "Volume upper bound");
    }

    /// <summary>Checks that bits 4 through 6 select the gate-off percentage for note commands.</summary>
    internal static void VerifySpcNoteGateAlgorithm(SuperMetroidAddressSpace rom)
    {
        // The high command bit is clear; bits4..6 select the gate fraction.
        for (int command = 0; command < 128; command++)
        {
            int index = (command >> 4) & 7;
            AssertEqual(rom.ReadCartridgeByte(SpcMusicTables.NoteGateReferenceAddress + index),
                SpcMusicTables.NoteGateOffPercentage(index), $"Original SPC gate selector {index}");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => SpcMusicTables.NoteGateOffPercentage(-1), "Gate negative index");
        AssertThrows<ArgumentOutOfRangeException>(() => SpcMusicTables.NoteGateOffPercentage(8), "Gate upper bound");
    }
}
