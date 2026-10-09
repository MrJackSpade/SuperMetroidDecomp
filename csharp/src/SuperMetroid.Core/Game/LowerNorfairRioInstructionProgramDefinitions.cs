namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled timing, private callbacks, and loop control for Lower Norfair Rio parent and
/// flame programs. Interleaved spritemap operands select extracted presentation frames.
/// </summary>
internal abstract class LowerNorfairRioInstructionProgramDefinitions
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

    /// <summary>$A2:C630 preparation holds. Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.</summary>
    private static readonly ushort[] PreparationDurations = [3, 3, 3, 3, 2, 1, 2, 3, 3];
    /// <summary>$A2:C686 cooldown holds, including the distinct final three poses. Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.</summary>
    private static readonly ushort[] CooldownDurations = [3, 3, 2, 1, 2, 3, 1, 1, 1];
    /// <summary>$A2:C6B0 flame display holds; three values do not establish a halving process. Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.</summary>
    private static readonly ushort[] FlameDurations = [6, 4, 3];

    /// <summary>Finds the compiled instruction-list start whose address interval contains a bank-$A2 pointer.</summary>
    /// <param name="address">Address within the parent or flame instruction data.</param>
    /// <returns>The owning program start, or zero when the address is outside the compiled lists.</returns>
    internal static ushort ProgramAt(ushort address) => address switch
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

    /// <summary>Returns the number of displayed frames in a program's looping or finite animation sequence.</summary>
    /// <param name="program">Start address of a compiled Lower Norfair Rio animation.</param>
    /// <returns>The frame count used to locate that program's following control words.</returns>
    private static int FrameCount(ushort program) => program switch
    {
        Idle => 4,
        PrepareToSwoop or Cooldown => 9,
        Descending => 1,
        _ => 3,
    };

    /// <summary>Identifies interleaved spritemap operands that select extracted presentation frames.</summary>
    /// <param name="address">Bank-$A2 instruction-data address to classify.</param>
    /// <returns><see langword="true"/> when the address is a compiled presentation operand.</returns>
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

    /// <summary>Gets the authored instruction hold for a frame in one of the compiled Rio programs.</summary>
    /// <param name="program">Start address of the animation whose timing is requested.</param>
    /// <param name="frame">Zero-based frame index within that program's duration sequence.</param>
    /// <returns>The number of updates for which the instruction holds the frame.</returns>
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
}
