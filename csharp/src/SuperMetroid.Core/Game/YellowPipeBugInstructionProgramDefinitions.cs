namespace SuperMetroid.Core.Game;

internal readonly record struct YellowPipeBugInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>Compiled control words for Yellow Brinstar Pipe Bug straight and arc loops.</summary>
internal static class YellowPipeBugInstructionProgramDefinitions
{
    /// <summary><c>$B3:8EFC</c>, straight flight facing left.</summary>
    internal const ushort FlyingLeft = 0x8efc;
    /// <summary><c>$B3:8F10</c>, arcing flight facing left.</summary>
    internal const ushort ArcingLeft = 0x8f10;
    /// <summary><c>$B3:8F24</c>, straight flight facing right.</summary>
    internal const ushort FlyingRight = 0x8f24;
    /// <summary><c>$B3:8F38</c>, arcing flight facing right.</summary>
    internal const ushort ArcingRight = 0x8f38;

    internal static int MechanicsWordCount => 24;
    internal static int PresentationWordCount => 16;

    /// <summary>Each loop displays four timed records followed by Goto and its target; all records calculate on demand.</summary>
    internal static YellowPipeBugInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        int program = index / 6;
        int record = index % 6;
        ushort address = (ushort)(FlyingLeft + 20 * program + (record < 4 ? 4 * record : 16 + 2 * (record - 4)));
        return new(address, ReadMechanicsWord(address));
    }

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(FlyingLeft + 20 * (index / 4) + 4 * (index % 4) + 2);
    }

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - FlyingLeft;
        if (offset >= 0 && offset < 80)
        {
            int program = offset / 20;
            int local = offset % 20;
            if (local < 16 && (local & 3) == 0)
                return (ushort)((program & 1) == 0 ? 4 : 1);
            if (local == 16)
                return CommonEnemyInstructionCodes.Goto;
            if (local == 18)
                return (ushort)(FlyingLeft + 20 * program);
        }
        throw new InvalidDataException(
            $"YellowPipeBug instruction mechanics pointer $B3:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xb30000)
            return false;
        int offset = unchecked((ushort)address) - FlyingLeft;
        if (offset < 0 || offset >= 80)
            return false;
        int local = offset % 20;
        return local >= 16 || (local & 3) < 2;
    }
}