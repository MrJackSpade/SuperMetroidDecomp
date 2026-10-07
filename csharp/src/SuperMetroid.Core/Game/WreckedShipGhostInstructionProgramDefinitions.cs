namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for the Wrecked Ship ghost (native Coven) animation
/// loop. Its three spritemap selections resolve compiled identities to installed artwork.
/// </summary>
internal abstract class WreckedShipGhostInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstList_Coven</c> at $A8:9A8C.</summary>
    internal const ushort Floating = 0x9a8c;

    /// <summary>The terminal <c>Instruction_Common_GotoY</c> word at $A8:9A98.</summary>
    internal const ushort LoopOpcode = 0x9a98;

    /// <summary>The first non-program word after <c>InstList_Coven</c>, at $A8:9A9C.</summary>
    internal const ushort FirstAdjacentConstant = 0x9a9c;

    public static int MechanicsWordCount => 5;
    public static int PresentationWordCount => 3;

    /// <summary>Three sixteen-tick poses followed by the native goto and loop target.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index < PresentationWordCount) return new((ushort)(Floating + 4 * index), 16);
        return index == PresentationWordCount
            ? new(LoopOpcode, CommonEnemyInstructionCodes.Goto)
            : new((ushort)(LoopOpcode + 2), Floating);
    }

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(Floating + 2 + 4 * index);
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            if (MechanicsWord(index).Address == address)
                return MechanicsWord(index).Value;
        }

        throw new InvalidDataException(
            $"Wrecked Ship ghost instruction mechanics pointer $A8:{address:X4} is not compiled.");
    }

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
