namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The encoding of a bank-$84 list word: with bit 15 set it names an instruction routine, otherwise a draw timer.</summary>
internal static class PlmListWordDefinitions
{
    /// <summary>The bit that marks an instruction word.</summary>
    private const ushort InstructionBit = 0x8000;

    /// <summary>Whether <paramref name="word"/> names an instruction rather than a draw timer.</summary>
    internal static bool IsInstruction(ushort word) => (word & InstructionBit) != 0;
}
