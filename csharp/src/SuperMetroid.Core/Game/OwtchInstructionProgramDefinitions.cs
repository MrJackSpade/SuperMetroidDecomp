namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Owtch's left/right animation programs.
/// Interleaved spritemap operands are selected by the installed visual catalog.
/// </summary>
internal abstract class OwtchInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstList_Owtch_MovingLeft_0</c> at $A2:A3AB.</summary>
    internal const ushort MovingLeft = 0xa3ab;

    /// <summary><c>InstList_Owtch_MovingRight_0</c> at $A2:A3BD.</summary>
    internal const ushort MovingRight = 0xa3bd;

    /// <summary>$A2:A3AD/A3B1/A3B5 and A3BF/A3C3/A3C7: selected eight-tick hold for the three cyclic shell poses.</summary>
    /// <remarks>Issue1165 narrow nonsense retention: the same spiked shell has shifted lower
    /// pink/purple pixels, played in opposite orders by the two directions. The selected
    /// playback cadence has no timing equation from the independently selected movement
    /// velocity. Native A0:D03F fixes collision radii8/8; all three maps retain identical
    /// geometry, and A2:A579 gates shots by behavior state, not pose or animation timer.
    /// Direction callbacks precede the loops; no pose triggers a collision/callback event.
    /// Replacing this chosen visual rate changes the animation. This exception covers
    /// only hold8, not instruction-reset1, motion, radii, sprite selection or pixel art.</remarks>
    internal const ushort CyclicVisualHold = 8;

    private const int ProgramBytes = 18;
    public static int MechanicsWordCount => 12;
    public static int PresentationWordCount => 6;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int word = index % 6;
        int offset = word == 0 ? 0 : word <= 4 ? 2 + (word - 1) * 4 : 16;
        ushort address = (ushort)(MovingLeft + index / 6 * ProgramBytes + offset);
        return new(address, ReadMechanicsWord(address));
    }
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(MovingLeft + index / 3 * ProgramBytes + 4 + index % 3 * 4);
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - MovingLeft;
        if ((uint)offset < 2 * ProgramBytes)
        {
            bool right = offset >= ProgramBytes;
            switch (offset % ProgramBytes)
            {
                case 0: return right ? EnemyInstructionCodePointers.Instruction_Owtch_1 : EnemyInstructionCodePointers.Instruction_Owtch_0;
                case 2: case 6: case 10: return CyclicVisualHold;
                case 14: return CommonEnemyInstructionCodes.Goto;
                case 16: return (ushort)((right ? MovingRight : MovingLeft) + 2);
            }
        }
        throw new InvalidDataException($"Owtch instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000) return false;
        int offset = (ushort)address - MovingLeft;
        return (uint)offset < 2 * ProgramBytes &&
            ((offset % ProgramBytes & ~1) is 0 or 2 or 6 or 10 or 14 or 16);
    }
}
