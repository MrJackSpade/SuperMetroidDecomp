using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares the effect-byte operand counts for all SPC effect opcodes with their original table and checks lookup bounds.</summary>
    /// <param name="rom">The address space containing the original SPC reference table.</param>
    private static void VerifySpcEffectOperandSelection(SuperMetroidAddressSpace rom)
    {
        for (int opcode = 0xe0; opcode <= 0xfe; opcode++)
        {
            int index = opcode - 0xe0;
            AssertEqual(rom.ReadByte(SpcMusicTables.EffectLengthReferenceAddress + index),
                SpcMusicTables.EffectByteLength(index), $"Original SPC opcode {opcode:X2} operand count");
        }
        AssertThrows<IndexOutOfRangeException>(() => SpcMusicTables.EffectByteLength(-1), "Effect negative index");
        AssertThrows<IndexOutOfRangeException>(() => SpcMusicTables.EffectByteLength(31), "Effect upper bound");
        AssertThrows<IndexOutOfRangeException>(() => SpcMusicTables.EffectByteLength(int.MaxValue), "Effect invalid maximum");
    }

    /// <summary>Verifies that every note command selects its volume percentage from the low nibble.</summary>
    /// <param name="rom">The address space containing the original SPC note-volume table.</param>
    private static void VerifySpcNoteVolumeAlgorithm(SuperMetroidAddressSpace rom)
    {
        // Every possible timing/volume command byte: the decoder uses its low nibble.
        for (int command = 0; command < 128; command++)
        {
            int index = command & 15;
            AssertEqual(rom.ReadByte(SpcMusicTables.NoteVolumeReferenceAddress + index),
                SpcMusicTables.NoteVolume(index), $"Original SPC volume selector {index}");
        }
        AssertThrows<IndexOutOfRangeException>(() => SpcMusicTables.NoteVolume(-1), "Volume negative index");
        AssertThrows<IndexOutOfRangeException>(() => SpcMusicTables.NoteVolume(16), "Volume upper bound");
    }

    /// <summary>Verifies that note-command bits 4 through 6 select the native gate-off percentage.</summary>
    /// <param name="rom">The address space containing the original SPC note-gate table.</param>
    private static void VerifySpcNoteGateAlgorithm(SuperMetroidAddressSpace rom)
    {
        // The high command bit is clear; bits4..6 select the gate fraction.
        for (int command = 0; command < 128; command++)
        {
            int index = (command >> 4) & 7;
            AssertEqual(rom.ReadByte(SpcMusicTables.NoteGateReferenceAddress + index),
                SpcMusicTables.NoteGateOffPercentage(index), $"Original SPC gate selector {index}");
        }
        AssertThrows<IndexOutOfRangeException>(() => SpcMusicTables.NoteGateOffPercentage(-1), "Gate negative index");
        AssertThrows<IndexOutOfRangeException>(() => SpcMusicTables.NoteGateOffPercentage(8), "Gate upper bound");
    }
}
