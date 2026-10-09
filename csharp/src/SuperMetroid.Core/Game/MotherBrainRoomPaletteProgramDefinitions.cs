namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control words for Mother Brain's fake-death room-palette flash program.
/// Color rows are installed presentation data, indexed by the compiled timed-entry identity.
/// </summary>
internal static class MotherBrainRoomPaletteProgramDefinitions
{
    /// <summary><c>MotherBrain_FakeDeath_RoomPalette_InstructionList</c> at $A9:D046.</summary>
    public const ushort FlashStart = 0xd046;

    /// <summary><c>Instruction_Goto</c> at $A9:9B0F.</summary>
    public const ushort GotoInstruction = 0x9b0f;

    /// <summary>The final grey room palette at $A9:D082.</summary>
    public const ushort FinalPalette = 0xd082;

    /// <summary>$A9:D046-$D07C: fourteen two-tick frames, each with a palette operand.</summary>
    internal const int PresentationWordCount = 14;

    /// <summary>$A9:D046-D07C, reviewed two-tick visual cadence for the fourteen-image flash performance; only the private palette timer consumes it.</summary>
    private const ushort FlashImageTicks = 2;

    /// <summary>$A9:D07E: closing Goto opcode following the timed frames.</summary>
    private const ushort LoopInstruction = FlashStart + PresentationWordCount * 4;

    /// <summary>Returns the native address of a palette operand in one of the timed flash frames.</summary>
    /// <param name="index">Zero-based frame index among the fourteen palette events.</param>
    /// <returns>The bank-$A9 address of that frame's palette selector word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the fourteen-frame program.</exception>
    internal static ushort PresentationWordAddress(int index) =>
        (uint)index < PresentationWordCount ? (ushort)(FlashStart + index * 4 + 2) :
            throw new IndexOutOfRangeException();

    /// <summary>Reviewed visual strength sequence in thirds for the fourteen events at $A9:D046-$D07C; each returns a paint image without gameplay callbacks.</summary>
    internal static int FlashStrength(int frame) => frame switch
    {
        1 or 5 or 8 => 1,
        2 or 11 or 13 => 2,
        3 or 7 or 10 => 3,
        0 or 4 or 6 or 9 or 12 => 0,
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>Reads a compiled frame duration or the closing loop instruction and target.</summary>
    /// <param name="address">Bank-$A9 address of a mechanics word in the palette flash program.</param>
    /// <returns>The fixed frame duration, Goto opcode, or flash-program start pointer stored at that address.</returns>
    /// <exception cref="InvalidDataException">The address does not identify a compiled mechanics word.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - FlashStart;
        if ((uint)offset < PresentationWordCount * 4 && offset % 4 == 0) return FlashImageTicks;
        if (address == LoopInstruction) return GotoInstruction;
        if (address == LoopInstruction + 2) return FlashStart;
        throw new InvalidDataException(
            $"Mother Brain room-palette mechanics pointer $A9:{address:X4} is not compiled.");
    }
}
