namespace SuperMetroid.Core.Game;

/// <summary>One compiled Lower Norfair Rio mechanics word at its bank-$A2 address.</summary>
internal readonly record struct LowerNorfairRioInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled timing, private callbacks, and loop control for Lower Norfair Rio parent and
/// flame programs. Interleaved spritemap operands select extracted presentation frames.
/// </summary>
internal static class LowerNorfairRioInstructionProgramDefinitions
{
    /// <summary><c>InstList_Holtz_Idle_0</c> at $A2:C61A.</summary>
    internal const ushort Idle = 0xc61a;
    /// <summary><c>InstList_Holtz_PrepareToSwoop</c> at $A2:C630.</summary>
    internal const ushort PrepareToSwoop = 0xc630;
    /// <summary><c>InstList_Holtz_Swoop_Descending</c> at $A2:C65A.</summary>
    internal const ushort Descending = 0xc65a;
    /// <summary><c>InstList_Holtz_Swoop_Ascending_Part1</c> at $A2:C662.</summary>
    internal const ushort AscendingPart1 = 0xc662;
    /// <summary><c>InstList_Holtz_Swoop_Part2_0</c> at $A2:C674.</summary>
    internal const ushort AscendingPart2 = 0xc674;
    /// <summary><c>InstList_Holtz_SwoopCooldown</c> at $A2:C686.</summary>
    internal const ushort Cooldown = 0xc686;
    /// <summary><c>InstList_Holtz_Flames</c> at $A2:C6B0.</summary>
    internal const ushort Flames = 0xc6b0;
    /// <summary><c>UNUSED_HoltzConstants_A2C6C0</c>, adjacent data at $A2:C6C0.</summary>
    internal const ushort AdjacentMovementDefinitions = 0xc6c0;

    /// <summary>A2:C630 preparation holds; required timing inputs until a functional derivation is established.</summary>
    private static readonly ushort[] PreparationDurations = [3, 3, 3, 3, 2, 1, 2, 3, 3];
    /// <summary>A2:C686 cooldown holds, including the distinct final three poses; required issue-1165 timing inputs.</summary>
    private static readonly ushort[] CooldownDurations = [3, 3, 2, 1, 2, 3, 1, 1, 1];
    /// <summary>A2:C6B0 flame display holds; the three values alone do not establish a halving process. Required input.</summary>
    private static readonly ushort[] FlameDurations = [6, 4, 3];
    internal static int MechanicsWordCount => 51;
    internal static int PresentationWordCount => 32;

    internal static LowerNorfairRioInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        for (int address = Idle; address < AdjacentMovementDefinitions; address += 2)
        {
            if (!IsPresentationWord((ushort)address) && index-- == 0)
                return new((ushort)address, ReadMechanicsWord((ushort)address));
        }
        throw new IndexOutOfRangeException();
    }

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        for (int address = Idle; address < AdjacentMovementDefinitions; address += 2)
        {
            if (IsPresentationWord((ushort)address) && index-- == 0)
                return (ushort)address;
        }
        throw new IndexOutOfRangeException();
    }

    private static ushort ProgramAt(ushort address) => address switch
    {
        >= Idle and < PrepareToSwoop => Idle,
        >= PrepareToSwoop and < Descending => PrepareToSwoop,
        >= Descending and < AscendingPart1 => Descending,
        >= AscendingPart1 and < AscendingPart2 => AscendingPart1,
        >= AscendingPart2 and < Cooldown => AscendingPart2,
        >= Cooldown and < Flames => Cooldown,
        >= Flames and < AdjacentMovementDefinitions => Flames,
        _ => 0,
    };

    private static int FrameCount(ushort program) => program switch
    {
        Idle => 4,
        PrepareToSwoop or Cooldown => 9,
        Descending => 1,
        _ => 3,
    };

    internal static bool IsPresentationWord(ushort address)
    {
        ushort program = ProgramAt(address);
        if (program == 0)
            return false;
        int offset = address - program - (program == Flames ? 0 : 2);
        return offset >= 2 && offset < 4 * FrameCount(program) && offset % 4 == 2;
    }

    /// <summary>Returns Lower Norfair Rio control or rejects non-mechanics pointers.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        ushort program = ProgramAt(address);
        if (program != 0 && (address & 1) == 0 && !IsPresentationWord(address))
        {
            int offset = address - program;
            if (program != Flames)
            {
                if (offset == 0)
                    return program is AscendingPart2 or Cooldown
                        ? LowerNorfairRioInstructionCodes.ShowFlames
                        : LowerNorfairRioInstructionCodes.HideFlames;
                offset -= 2;
            }
            int frames = FrameCount(program);
            if (offset < frames * 4)
                return Duration(program, offset / 4);
            bool loops = program is Idle or AscendingPart2 or Flames;
            if (offset == frames * 4)
                return loops ? CommonEnemyInstructionCodes.Goto
                    : program == Descending ? CommonEnemyInstructionCodes.Sleep
                    : LowerNorfairRioInstructionCodes.SetAnimationFinishedFlag;
            return loops ? (ushort)(program + (program == Flames ? 0 : 2))
                : CommonEnemyInstructionCodes.Sleep;
        }
        throw new InvalidDataException(
            $"Lower Norfair Rio instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }

    private static ushort Duration(ushort program, int frame) => program switch
    {
        Idle => 11,
        Descending => 1,
        AscendingPart1 => 3,
        AscendingPart2 => 2,
        PrepareToSwoop => PreparationDurations[frame],
        Cooldown => CooldownDurations[frame],
        Flames => FlameDurations[frame],
        _ => throw new InvalidDataException(),
    };

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;
        ushort word = (ushort)(address & ~1);
        return ProgramAt(word) != 0 && !IsPresentationWord(word);
    }
}
