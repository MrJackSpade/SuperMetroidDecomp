namespace SuperMetroid.Core.Game;

internal readonly record struct TorizoFallingLeftMechanicsWord(
    ushort Address, ushort Value);

/// <summary>
/// The shared Torizo falling-left list at $AA:BC78-BC95. Its one visual
/// operand selects the separately installed $AA:B014 extended frame.
/// </summary>
internal static class TorizoFallingLeftInstructionProgramDefinitions
{
    /// <summary><c>InstList_Torizo_FacingLeft_Falling_0</c> at $AA:BC78.</summary>
    internal const ushort Start = 0xbc78;
    /// <summary><c>InstList_Torizo_FacingLeft_Falling_1</c> at $AA:BC80.</summary>
    internal const ushort FallingLoop = 0xbc80;
    /// <summary><c>InstList_Torizo_FacingLeft_Falling_2</c> at $AA:BC88.</summary>
    internal const ushort Landing = 0xbc88;
    /// <summary>First byte after the falling-left list, $AA:BC96.</summary>
    internal const ushort End = 0xbc96;
    /// <summary><c>ExtendedSpritemaps_Torizo_Jumping_Falling_FacingLeft_1</c> at $AA:B014.</summary>
    internal const ushort FallingFrame = 0xb014;
    /// <summary><c>Function_Torizo_Movement_Jumping_Falling</c> at $AA:C82C.</summary>
    private const ushort JumpingFallingMovement = 0xc82c;
    /// <summary><c>InstList_Torizo_FacingLeft_Faceless_Walking_LeftLegMoving</c> at $AA:BD52.</summary>
    private const ushort FacelessWalkingLeftLeg = 0xbd52;
    /// <summary><c>InstList_Torizo_FacingLeft_Walking_LeftLegMoving</c> at $AA:B9B6.</summary>
    private const ushort BombTorizoWalkingLeftLeg = 0xb9b6;

    private static readonly TorizoFallingLeftMechanicsWord[] Words =
    [
        new(0xbc78, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xbc7a, JumpingFallingMovement),
        new(0xbc7c, TorizoInstructionCodes.Instruction_Torizo_LinkInstructionInY),
        new(0xbc7e, Landing),
        new(0xbc80, 0x0005),
        new(0xbc84, CommonEnemyInstructionCodes.Goto),
        new(0xbc86, FallingLoop),
        new(0xbc88, TorizoInstructionCodes.Instruction_Torizo_PlayTorizoFootstepsSFX),
        new(0xbc8a, TorizoInstructionCodes.Instruction_Torizo_SpawnTorizoLandingDustClouds),
        new(0xbc8c, TorizoInstructionCodes.Instruction_Torizo_GotoY_IfFaceBlownUp_ElseGotoY2_IfGolden),
        new(0xbc8e, FacelessWalkingLeftLeg),
        new(0xbc90, GoldenTorizoCombatInstructionPointers.WalkingLeftLeftLeg),
        new(0xbc92, CommonEnemyInstructionCodes.Goto),
        new(0xbc94, BombTorizoWalkingLeftLeg),
    ];

    private static readonly ushort[] PresentationWords = [0xbc82];

    internal static int MechanicsWordCount => Words.Length;
    internal static TorizoFallingLeftMechanicsWord MechanicsWord(int index) => Words[index];
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        foreach (TorizoFallingLeftMechanicsWord word in Words)
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
        foreach (TorizoFallingLeftMechanicsWord word in Words)
            if (offset == word.Address ||
                offset == unchecked((ushort)(word.Address + 1)))
                return true;
        return false;
    }
}
