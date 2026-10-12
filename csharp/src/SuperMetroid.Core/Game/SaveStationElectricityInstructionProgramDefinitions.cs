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
    /// <summary>Timer setup, eight frame durations, loop control, and deletion words.</summary>
    public static int MechanicsWordCount => FrameCount + 5;
    /// <summary>One separately installed spritemap operand for each frame duration.</summary>
    public static int PresentationWordCount => FrameCount;

    /// <summary>Enumerates compiled timer, frame-duration, loop, and delete control words in native order.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index < 2)
            return new((ushort)(Initial + 2 * index), index == 0
                ? (ushort)EnemyProjectileInstruction.TimerInY : (ushort)20);
        if (index < FrameCount + 2)
            return new((ushort)(Loop + 4 * (index - 2)), 1);
        int terminal = index - FrameCount - 2;
        return new((ushort)(Loop + 4 * FrameCount + 2 * terminal), terminal switch
        {
            0 => (ushort)EnemyProjectileInstruction.DecrementTimer_GotoYIfNonZero,
            1 => Loop,
            _ => (ushort)EnemyProjectileInstruction.Delete,
        });
    }

    /// <summary>Locates a separately installed sprite operand for a bounded frame index.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(Loop + 2 + 4 * index);
    }
    /// <summary>Looks up a compiled electricity control word while rejecting presentation operands.</summary>
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
