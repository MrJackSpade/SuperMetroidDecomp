using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The shared Torizo falling-left list at $AA:BC78-BC95. Its one visual
/// operand selects the separately installed $AA:B014 extended frame.
/// </summary>
internal abstract class TorizoFallingLeftInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary><c>InstList_Torizo_FacingLeft_Falling_0</c> at $AA:BC78.</summary>
    internal const ushort Start = 0xbc78;
    /// <summary><c>InstList_Torizo_FacingLeft_Falling_1</c> at $AA:BC80.</summary>
    internal const ushort FallingLoop = 0xbc80;
    /// <summary><c>InstList_Torizo_FacingLeft_Falling_2</c> at $AA:BC88.</summary>
    internal const ushort Landing = 0xbc88;
    /// <summary><c>ExtendedSpritemaps_Torizo_Jumping_Falling_FacingLeft_1</c> at $AA:B014.</summary>
    internal const ushort FallingFrame = 0xb014;
    /// <summary><c>Function_Torizo_Movement_Jumping_Falling</c> at $AA:C82C.</summary>
    private const ushort JumpingFallingMovement = 0xc82c;
    /// <summary><c>InstList_Torizo_FacingLeft_Faceless_Walking_LeftLegMoving</c> at $AA:BD52.</summary>
    private const ushort FacelessWalkingLeftLeg = 0xbd52;
    /// <summary><c>InstList_Torizo_FacingLeft_Walking_LeftLegMoving</c> at $AA:B9B6.</summary>
    private const ushort BombTorizoWalkingLeftLeg = 0xb9b6;

    /// <summary><c>InstList_GoldenTorizo_WalkingLeft_LeftLegMoving</c> at $AA:D259.</summary>
    private const ushort GoldenTorizoWalkingLeftLeftLegMoving = 0xd259;

    /// <summary>Native program bank $AA.</summary>
    internal const byte Bank = 0xaa;
    static int IDeclaredProgramBank.Bank => Bank;

    private static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xbc78),
        Entry(Start),
        Op(TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY, JumpingFallingMovement),
        Op(TorizoInstructionCodes.Instruction_Torizo_LinkInstructionInY, Landing),
        Entry(FallingLoop),
        Frame(5),
        Op(CommonEnemyInstructionCodes.Goto, FallingLoop),
        Entry(Landing),
        Op(TorizoInstructionCodes.Instruction_Torizo_PlayTorizoFootstepsSFX),
        Op(TorizoInstructionCodes.Instruction_Torizo_SpawnTorizoLandingDustClouds),
        Op(TorizoInstructionCodes.Instruction_Torizo_GotoY_IfFaceBlownUp_ElseGotoY2_IfGolden, FacelessWalkingLeftLeg, GoldenTorizoWalkingLeftLeftLegMoving),
        Op(CommonEnemyInstructionCodes.Goto, BombTorizoWalkingLeftLeg));

    public static int MechanicsWordCount => Layout.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static int PresentationWordCount => Layout.PresentationSlotCount;
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);

    public static bool IsCompiledMechanicsByte(int address) => Layout.IsCompiledMechanicsByte(address);
}
