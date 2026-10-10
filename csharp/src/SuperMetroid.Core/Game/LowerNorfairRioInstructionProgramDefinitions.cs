namespace SuperMetroid.Core.Game;

/// <summary>The seven bank-$A2 Lower Norfair Rio (Holtz) parent and flame instruction lists.</summary>
internal enum LowerNorfairRioProgram : ushort
{
    /// <summary><c>InstList_Holtz_Idle_0</c> at $A2:C61A.</summary>
    Idle = 0xc61a,
    /// <summary><c>InstList_Holtz_PrepareToSwoop</c> at $A2:C630.</summary>
    PrepareToSwoop = 0xc630,
    /// <summary><c>InstList_Holtz_Swoop_Descending</c> at $A2:C65A.</summary>
    Descending = 0xc65a,
    /// <summary><c>InstList_Holtz_Swoop_Ascending_Part1</c> at $A2:C662.</summary>
    AscendingPart1 = 0xc662,
    /// <summary><c>InstList_Holtz_Swoop_Part2_0</c> at $A2:C674.</summary>
    AscendingPart2 = 0xc674,
    /// <summary><c>InstList_Holtz_SwoopCooldown</c> at $A2:C686.</summary>
    Cooldown = 0xc686,
    /// <summary><c>InstList_Holtz_Flames</c> at $A2:C6B0.</summary>
    Flames = 0xc6b0,
}

/// <summary>
/// Compiled timing, private callbacks, and loop control for Lower Norfair Rio parent and
/// flame programs. Interleaved spritemap operands select extracted presentation frames.
/// </summary>
internal abstract class LowerNorfairRioInstructionProgramDefinitions
{
    /// <summary><c>UNUSED_HoltzConstants_A2C6C0</c>, adjacent data at $A2:C6C0.</summary>
    internal const ushort AdjacentMovementDefinitions = 0xc6c0;

    /// <summary>$A2:C630 preparation holds. Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.</summary>
    private static readonly ushort[] PreparationDurations = [3, 3, 3, 3, 2, 1, 2, 3, 3];
    /// <summary>$A2:C686 cooldown holds, including the distinct final three poses. Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.</summary>
    private static readonly ushort[] CooldownDurations = [3, 3, 2, 1, 2, 3, 1, 1, 1];
    /// <summary>$A2:C6B0 flame display holds; three values do not establish a halving process. Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.</summary>
    private static readonly ushort[] FlameDurations = [6, 4, 3];

    /// <summary>Returns the list containing <paramref name="address"/>, or null outside $A2:C61A..C6BF.</summary>
    internal static LowerNorfairRioProgram? ProgramAt(ushort address) => address switch
    {
        >= (ushort)LowerNorfairRioProgram.Idle and < (ushort)LowerNorfairRioProgram.PrepareToSwoop => LowerNorfairRioProgram.Idle,
        >= (ushort)LowerNorfairRioProgram.PrepareToSwoop and < (ushort)LowerNorfairRioProgram.Descending => LowerNorfairRioProgram.PrepareToSwoop,
        >= (ushort)LowerNorfairRioProgram.Descending and < (ushort)LowerNorfairRioProgram.AscendingPart1 => LowerNorfairRioProgram.Descending,
        >= (ushort)LowerNorfairRioProgram.AscendingPart1 and < (ushort)LowerNorfairRioProgram.AscendingPart2 => LowerNorfairRioProgram.AscendingPart1,
        >= (ushort)LowerNorfairRioProgram.AscendingPart2 and < (ushort)LowerNorfairRioProgram.Cooldown => LowerNorfairRioProgram.AscendingPart2,
        >= (ushort)LowerNorfairRioProgram.Cooldown and < (ushort)LowerNorfairRioProgram.Flames => LowerNorfairRioProgram.Cooldown,
        >= (ushort)LowerNorfairRioProgram.Flames and < AdjacentMovementDefinitions => LowerNorfairRioProgram.Flames,
        _ => null,
    };

    private static int FrameCount(LowerNorfairRioProgram program) => program switch
    {
        LowerNorfairRioProgram.Idle => 4,
        LowerNorfairRioProgram.PrepareToSwoop or LowerNorfairRioProgram.Cooldown => 9,
        LowerNorfairRioProgram.Descending => 1,
        LowerNorfairRioProgram.AscendingPart1 or LowerNorfairRioProgram.AscendingPart2 or LowerNorfairRioProgram.Flames => 3,
        _ => throw new InvalidOperationException($"Undefined {nameof(LowerNorfairRioProgram)} {(int)program:X4}."),
    };

    internal static bool IsPresentationWord(ushort address)
    {
        if (ProgramAt(address) is not { } program)
            return false;
        int offset = address - (ushort)program - (program == LowerNorfairRioProgram.Flames ? 0 : 2);
        return offset >= 2 && offset < 4 * FrameCount(program) && offset % 4 == 2;
    }

    /// <summary>Returns Lower Norfair Rio control or rejects non-mechanics pointers.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (ProgramAt(address) is { } program && (address & 1) == 0 && !IsPresentationWord(address))
        {
            int offset = address - (ushort)program;
            if (program != LowerNorfairRioProgram.Flames)
            {
                if (offset == 0)
                    return program is LowerNorfairRioProgram.AscendingPart2 or LowerNorfairRioProgram.Cooldown
                        ? LowerNorfairRioInstructionCodes.ShowFlames
                        : LowerNorfairRioInstructionCodes.HideFlames;
                offset -= 2;
            }
            int frames = FrameCount(program);
            if (offset < frames * 4)
                return Duration(program, offset / 4);
            bool loops = program is LowerNorfairRioProgram.Idle or LowerNorfairRioProgram.AscendingPart2 or LowerNorfairRioProgram.Flames;
            if (offset == frames * 4)
                return loops ? CommonEnemyInstructionCodes.Goto
                    : program == LowerNorfairRioProgram.Descending ? CommonEnemyInstructionCodes.Sleep
                    : LowerNorfairRioInstructionCodes.SetAnimationFinishedFlag;
            return loops ? (ushort)((ushort)program + (program == LowerNorfairRioProgram.Flames ? 0 : 2))
                : CommonEnemyInstructionCodes.Sleep;
        }
        throw new InvalidDataException(
            $"Lower Norfair Rio instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }

    private static ushort Duration(LowerNorfairRioProgram program, int frame) => program switch
    {
        LowerNorfairRioProgram.Idle => 11,
        LowerNorfairRioProgram.Descending => 1,
        LowerNorfairRioProgram.AscendingPart1 => 3,
        LowerNorfairRioProgram.AscendingPart2 => 2,
        LowerNorfairRioProgram.PrepareToSwoop => PreparationDurations[frame],
        LowerNorfairRioProgram.Cooldown => CooldownDurations[frame],
        LowerNorfairRioProgram.Flames => FlameDurations[frame],
        _ => throw new InvalidOperationException($"Undefined {nameof(LowerNorfairRioProgram)} {(int)program:X4}."),
    };
}
