namespace SuperMetroid.Core.Game;

internal readonly record struct GoldenTorizoLeftTurnMechanicsWord(
    ushort Address, ushort Value);

/// <summary>
/// Golden Torizo's callable dodge-turn and ordinary left-turn lists at
/// $AA:D1F1-D20C. Twelve control words stay compiled; both visual operands
/// select the already-editable facing-screen turning frame at $AA:A4F0.
/// </summary>
internal static class GoldenTorizoLeftTurnInstructionProgramDefinitions
{
    /// <summary><c>InstList_GoldenTorizo_Dodge_TurningLeft</c> at $AA:D1F1.</summary>
    internal const ushort Dodge = 0xd1f1;
    /// <summary><c>InstList_GoldenTorizo_TurningLeft</c> at $AA:D203.</summary>
    internal const ushort Turn = 0xd203;
    /// <summary>First byte after the left-turn lists, $AA:D20D.</summary>
    internal const ushort End = GoldenTorizoCombatInstructionPointers.WalkingLeftRightLeg;
    /// <summary>
    /// <c>ExtendedSpritemaps_Torizo_FacingScreen_Turning_Dodging</c> at
    /// $AA:A4F0, also used by both turning-right lists.
    /// </summary>
    internal const ushort FacingScreenFrame = 0xa4f0;

    /// <summary><c>Function_Torizo_SimpleMovement</c> at $AA:C6BF.</summary>
    private const ushort SimpleMovement = 0xc6bf;

    private static readonly GoldenTorizoLeftTurnMechanicsWord[] Words =
    [
        new(0xd1f1, TorizoInstructionCodes.Instruction_Torizo_FunctionInY),
        new(0xd1f3, SimpleMovement),
        new(0xd1f5, TorizoInstructionCodes.Instruction_Torizo_SetAnimationLock),
        new(0xd1f7, TorizoInstructionCodes.Instruction_Torizo_SetTorizoTurningAroundFlag),
        new(0xd1f9, 0x0018),
        new(0xd1fd, TorizoInstructionCodes.Instruction_Torizo_ClearAnimationLock),
        new(0xd1ff, CommonEnemyInstructionCodes.Goto),
        new(0xd201, GoldenTorizoCombatInstructionPointers.WalkingLeftRightLeg),
        new(0xd203, TorizoInstructionCodes.Instruction_Torizo_FunctionInY),
        new(0xd205, SimpleMovement),
        new(0xd207, TorizoInstructionCodes.Instruction_Torizo_SetTorizoTurningAroundFlag),
        new(0xd209, 0x0008),
    ];

    private static readonly ushort[] PresentationWords = [0xd1fb, 0xd20b];

    internal static int MechanicsWordCount => Words.Length;
    internal static GoldenTorizoLeftTurnMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        foreach (GoldenTorizoLeftTurnMechanicsWord word in Words)
        {
            if (word.Address != address) continue;
            value = word.Value;
            return true;
        }
        value = 0;
        return false;
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xaa0000)
            return false;
        ushort offset = unchecked((ushort)address);
        foreach (GoldenTorizoLeftTurnMechanicsWord word in Words)
            if (offset == word.Address ||
                offset == unchecked((ushort)(word.Address + 1)))
                return true;
        return false;
    }
}
