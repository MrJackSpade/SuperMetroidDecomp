namespace SuperMetroid.Core.Game;

internal readonly record struct MochtroidInstructionMechanicsWord(ushort Address, ushort Value);

/// <summary>
/// Compiled engine-control words for Mochtroid's free-flight and attached animation loops.
/// Their eight spritemap operands select independently supplied presentation data.
/// </summary>
internal static class MochtroidInstructionProgramDefinitions
{
    /// <summary><c>InstList_Mochtroid_NotTouchingSamus</c> at $A3:A745.</summary>
    internal const ushort FreeFlight = 0xa745;
    /// <summary><c>InstList_Mochtroid_TouchingSamus</c> at $A3:A759.</summary>
    internal const ushort Attached = 0xa759;
    /// <summary>The shake-velocity table immediately after the programs, at $A3:A76D.</summary>
    internal const ushort FirstAdjacentMechanicsData = 0xa76d;

    internal static int MechanicsWordCount => 12;
    internal static int PresentationWordCount => 8;

    /// <summary>Each loop displays four timed records followed by Goto and its target; all records calculate on demand.</summary>
    internal static MochtroidInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        int program = index / 6;
        int record = index % 6;
        ushort address = (ushort)(FreeFlight + 20 * program + (record < 4 ? 4 * record : 16 + 2 * (record - 4)));
        return new(address, ReadMechanicsWord(address));
    }

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(FreeFlight + 20 * (index / 4) + 4 * (index % 4) + 2);
    }

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - FreeFlight;
        if (offset >= 0 && offset < 40)
        {
            int program = offset / 20;
            int local = offset % 20;
            if (local < 16 && (local & 3) == 0)
                return (ushort)(program == 0 ? 14 : 5);
            if (local == 16)
                return CommonEnemyInstructionCodes.Goto;
            if (local == 18)
                return (ushort)(FreeFlight + 20 * program);
        }
        throw new InvalidDataException(
            $"Mochtroid instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
            return false;
        int offset = unchecked((ushort)address) - FreeFlight;
        if (offset < 0 || offset >= 40)
            return false;
        int local = offset % 20;
        return local >= 16 || (local & 3) < 2;
    }
}