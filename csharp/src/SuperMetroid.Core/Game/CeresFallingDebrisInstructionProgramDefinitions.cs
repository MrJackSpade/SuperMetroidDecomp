namespace SuperMetroid.Core.Game;

/// <summary>One compiled Ceres falling-debris mechanics word at its bank-$86 address.</summary>
internal readonly record struct CeresFallingDebrisInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for the light and dark Ceres falling-debris poses. Their spritemap
/// operands resolve through extracted presentation art.
/// </summary>
internal static class CeresFallingDebrisInstructionProgramDefinitions
{
    /// <summary>
    /// <c>InstList_EnemyProjectile_CeresFallingTile_Light</c> at $86:9750,
    /// selected by definition $9734. Its pinned-ROM mechanics are one tick
    /// at $9750, then terminal $8159 sleep at $9754. The production interpreter
    /// reaches only those two private pointers before a separate shot reaction
    /// can enter the shared delete program. The $9752 spritemap is presentation.
    /// </summary>
    internal const ushort Light = 0x9750;

    /// <summary>
    /// <c>InstList_EnemyProjectile_CeresFallingTile_Dark</c> at $86:9756,
    /// selected by definition $9742. Both pinned-ROM mechanics words are
    /// the light program's words relocated by six bytes: one tick at $9756,
    /// then terminal $8159 sleep at $975A. Only those two private pointers
    /// are reachable before shared shot deletion. The $9758 spritemap is
    /// independent presentation data.
    /// </summary>
    internal const ushort Dark = 0x9756;

    internal static int MechanicsWordCount => 4;
    internal static int PresentationWordCount => 2;

    /// <summary>Two six-byte programs each display one frame for one tick, then sleep.</summary>
    internal static CeresFallingDebrisInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        bool sleep = (index & 1) != 0;
        return new((ushort)(Light + 6 * (index / 2) + (sleep ? 4 : 0)),
            sleep ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep : (ushort)1);
    }

    /// <summary>
    /// Visual operands at $86:9752/$9758 follow their one-tick duration words.
    /// Original $8ABF/$8AC6 selectors reference two seven-byte one-component poses;
    /// their artwork remains independently required under issue1165.
    /// </summary>
    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(Light + 6 * index + 2);
    }
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.CeresFallingDebrisLight or
        RoomEnemyProjectileKind.CeresFallingDebrisDark;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            CeresFallingDebrisInstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Ceres falling-debris mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
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
