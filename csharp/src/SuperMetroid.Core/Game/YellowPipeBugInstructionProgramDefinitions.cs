namespace SuperMetroid.Core.Game;

/// <summary>Compiled control words for Yellow Brinstar Pipe Bug straight and arc loops.</summary>
internal abstract class YellowPipeBugInstructionProgramDefinitions
{
    /// <summary><c>$B3:8EFC</c>, straight flight facing left.</summary>
    internal const ushort FlyingLeft = 0x8efc;
    /// <summary><c>$B3:8F10</c>, arcing flight facing left.</summary>
    internal const ushort ArcingLeft = 0x8f10;
    /// <summary><c>$B3:8F24</c>, straight flight facing right.</summary>
    internal const ushort FlyingRight = 0x8f24;
    /// <summary><c>$B3:8F38</c>, arcing flight facing right.</summary>
    internal const ushort ArcingRight = 0x8f38;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - FlyingLeft;
        if (offset is >= 0 and < 80)
        {
            int program = offset / 20;
            int local = offset % 20;
            if (local < 16 && (local & 3) == 0)
                return (ushort)((program & 1) == 0 ? 4 : 1);
            if (local == 16)
                return (ushort)CommonEnemyInstruction.Goto;
            if (local == 18)
                return (ushort)(FlyingLeft + 20 * program);
        }
        throw new InvalidDataException(
            $"YellowPipeBug instruction mechanics pointer $B3:{address:X4} is not compiled.");
    }
}