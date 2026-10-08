namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Bull's ordinary and immune-shot animation programs.
/// Interleaved spritemap operands select installed presentation frames.
/// </summary>
internal abstract class BullInstructionProgramDefinitions
{
    /// <summary><c>InstList_Bull_Normal</c> at $A8:D841.</summary>
    internal const ushort Normal = 0xd841;
    /// <summary><c>InstList_Bull_Shot_0</c> at $A8:D855.</summary>
    internal const ushort Shot = 0xd855;
    /// <summary><c>InstList_Bull_Shot_1</c> at $A8:D859.</summary>
    internal const ushort ShotLoop = 0xd859;

    public static int MechanicsWordCount => 16;

    /// <summary>Normal loops four ten-frame drawings; shot loops four three-frame drawings five times and returns to normal.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index < 4) return new((ushort)(Normal + 4 * index), 10);
        if (index is >= 8 and < 12) return new((ushort)(ShotLoop + 4 * (index - 8)), 3);
        return index switch
        {
            4 => new(Normal + 16, CommonEnemyInstructionCodes.Goto),
            5 => new(Normal + 18, Normal),
            6 => new(Shot, CommonEnemyInstructionCodes.SetTimer),
            7 => new(Shot + 2, 5),
            12 => new(ShotLoop + 16, CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
            13 => new(ShotLoop + 18, ShotLoop),
            14 => new(ShotLoop + 20, CommonEnemyInstructionCodes.Goto),
            _ => new(ShotLoop + 22, Normal),
        };
    }

    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - (address < ShotLoop ? Normal + 2 : ShotLoop + 2);
        return (uint)offset < 16 && offset % 4 == 0;
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            if (MechanicsWord(index).Address == address)
                return MechanicsWord(index).Value;
        }

        throw new InvalidDataException(
            $"Bull instruction mechanics pointer $A8:{address:X4} is not compiled.");
    }
}
