namespace SuperMetroid.Core.Game;

internal readonly record struct GoldenTorizoInitialMechanicsWord(ushort Address, ushort Value);

/// <summary>
/// Golden Torizo's initial bank-$AA entry through its first sleep. The initial
/// tile upload uses the shared compiled $814B descriptor/artwork path; later
/// falling, awakening, and combat programs retain separate ownership.
/// </summary>
internal static class GoldenTorizoInitialInstructionProgramDefinitions
{
    /// <summary><c>InstList_GoldenTorizo_Initial_0</c> at $AA:C9CB.</summary>
    internal const ushort Initial = 0xc9cb;

    /// <summary>The initial extended-spritemap operand at $AA:C9DE.</summary>
    internal const ushort InitialFrameOperand = 0xc9de;

    /// <summary>The first bank-$AA sleep instruction at $AA:C9E0.</summary>
    internal const ushort Sleep = 0xc9e0;

    /// <summary>Golden Torizo's Samus-position wake function at $AA:D5C2.</summary>
    internal const ushort WakeWhenSamusApproaches = 0xd5c2;

    private static readonly GoldenTorizoInitialMechanicsWord[] Words =
    [
        new(Initial, CommonEnemyInstructionCodes.CopyToVram),
        new(0xc9d4, TorizoInstructionCodes.Instruction_Torizo_SetSteppedLeftWithRightFootState),
        new(0xc9d6, TorizoInstructionCodes.Instruction_Torizo_SetAnimationLock),
        new(0xc9d8, TorizoInstructionCodes.Instruction_Torizo_FunctionInY),
        new(0xc9da, WakeWhenSamusApproaches),
        new(0xc9dc, 1),
        new(Sleep, CommonEnemyInstructionCodes.Sleep),
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static GoldenTorizoInitialMechanicsWord MechanicsWord(int index) => Words[index];
    internal static int PresentationWordCount => 1;
    internal static ushort PresentationWordAddress(int index) => index == 0
        ? InitialFrameOperand
        : throw new ArgumentOutOfRangeException(nameof(index));

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        foreach (GoldenTorizoInitialMechanicsWord word in Words)
        {
            if (word.Address == address)
            {
                value = word.Value;
                return true;
            }
        }
        value = 0;
        return false;
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xaa0000)
            return false;
        ushort offset = unchecked((ushort)address);
        foreach (GoldenTorizoInitialMechanicsWord word in Words)
        {
            if (offset == word.Address || offset == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
