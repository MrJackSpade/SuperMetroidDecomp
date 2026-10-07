namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Kago's slow and post-hit animation loops.
/// Interleaved spritemap operands select separately installed presentation data.
/// </summary>
internal abstract class KagoInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstList_Kago_Initial_SlowAnimation</c> at $A8:AB1E.</summary>
    internal const ushort Slow = 0xab1e;
    /// <summary><c>InstList_Kago_TakenHit_FastAnimation</c> at $A8:AB32.</summary>
    internal const ushort Fast = 0xab32;

    public static int MechanicsWordCount => 12;
    public static int PresentationWordCount => 8;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort start = index < 6 ? Slow : Fast;
        int local = index % 6;
        if (local < 4) return new((ushort)(start + 4 * local), index < 6 ? (ushort)10 : (ushort)3);
        return local == 4 ? new((ushort)(start + 16), CommonEnemyInstructionCodes.Goto)
            : new((ushort)(start + 18), start);
    }

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(Slow + 20 * (index / 4) + 2 + 4 * (index % 4));
    }

    /// <summary>Four interleaved visual operands per20-byte slow/fast loop.</summary>
    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - Slow;
        return (uint)offset < 40 && offset % 20 < 16 && offset % 4 == 2;
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            if (MechanicsWord(index).Address == address)
                return MechanicsWord(index).Value;
        }

        throw new InvalidDataException(
            $"Kago instruction mechanics pointer $A8:{address:X4} is not compiled.");
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
