namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for the save station's twenty-cycle electricity animation. The eight
/// spritemap operands resolve through extracted presentation art.
/// </summary>
internal abstract class SaveStationElectricityInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_SaveStationElectricity_0</c> at $86:E683.</summary>
    internal const ushort Initial = 0xe683;

    /// <summary><c>InstList_EnemyProjectile_SaveStationElectricity_1</c> at $86:E687.</summary>
    internal const ushort Loop = 0xe687;

    /// <summary>Eight one-frame drawing records between timer setup and loop/delete commands.</summary>
    private const int FrameCount = 8;

    /// <summary>Number of compiled words covering timer setup, animation frames, and terminal control.</summary>
    public static int MechanicsWordCount => FrameCount + 5;

    /// <summary>Number of sprite-map operands supplied by extracted save-station presentation art.</summary>
    public static int PresentationWordCount => FrameCount;

    /// <summary>Resolves an instruction slot to its address and mechanics value in the initial or looping program.</summary>
    /// <param name="index">Zero-based index among the compiled mechanics words.</param>
    /// <returns>The bank-local instruction address and value stored at that slot.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="MechanicsWordCount"/>.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index < 2)
            return new((ushort)(Initial + 2 * index), index == 0
                ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY : (ushort)20);
        if (index < FrameCount + 2)
            return new((ushort)(Loop + 4 * (index - 2)), 1);
        int terminal = index - FrameCount - 2;
        return new((ushort)(Loop + 4 * FrameCount + 2 * terminal), terminal switch
        {
            0 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero,
            1 => Loop,
            _ => EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete,
        });
    }

    /// <summary>Returns the instruction address containing one of the eight animation sprite-map operands.</summary>
    /// <param name="index">Zero-based index of the presentation operand.</param>
    /// <returns>The bank-local address of the operand's instruction slot.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(Loop + 2 + 4 * index);
    }
    /// <summary>Finds the compiled mechanics value associated with a native instruction address.</summary>
    /// <param name="address">Bank-local address to resolve.</param>
    /// <returns>The mechanics word stored at that address.</returns>
    /// <exception cref="InvalidDataException">The address is not part of the compiled save-station electricity program.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            var word = MechanicsWord(index);
            if (word.Address == address) return word.Value;
        }
        throw new InvalidDataException(
            $"Save-station electricity mechanics pointer $86:{address:X4} is not compiled.");
    }
}
