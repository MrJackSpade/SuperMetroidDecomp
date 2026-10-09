using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Draygon's body, eye, tail, and arms programs.
/// Interleaved physical frame identities are compiled selectors; installed display
/// bindings may replace their OAM/BG2 art without changing mechanics or timing.
/// </summary>
internal abstract class DraygonInstructionProgramDefinitions
{
    /// <summary><c>InstList_Draygon_Sleep</c> at $A5:97B9.</summary>
    public const ushort Sleep = 0x97b9;
    /// <summary><c>InstList_DraygonBody_FacingLeft_Reset</c> at $A5:97BB.</summary>
    public const ushort BodyFacingLeftReset = 0x97bb;
    /// <summary><c>InstList_DraygonBody_FacingRight_Reset</c> at $A5:97D1.</summary>
    public const ushort BodyFacingRightReset = 0x97d1;
    /// <summary><c>InstList_DraygonArms_FacingLeft_Idle_0</c> at $A5:97E7.</summary>
    public const ushort ArmsFacingLeftIdle = 0x97e7;
    /// <summary><c>InstList_DraygonArms_FacingLeft_NearSwoopApex</c> at $A5:9813.</summary>
    public const ushort ArmsFacingLeftNearSwoopApex = 0x9813;
    /// <summary><c>InstList_DraygonArms_FacingLeft_Grab</c> at $A5:9845.</summary>
    public const ushort ArmsFacingLeftGrab = 0x9845;
    /// <summary><c>InstList_DraygonArms_FacingLeft_Dying</c> at $A5:9867.</summary>
    public const ushort ArmsFacingLeftDying = 0x9867;
    /// <summary><c>InstList_DraygonBody_FacingLeft_Idle</c> at $A5:9889.</summary>
    public const ushort BodyFacingLeftIdle = 0x9889;
    /// <summary><c>InstList_Draygon_Delete</c> at $A5:98ED.</summary>
    public const ushort Delete = 0x98ed;
    /// <summary><c>InstList_DraygonBody_FacingLeft_FireGoop</c> at $A5:98FE.</summary>
    public const ushort BodyFacingLeftFireGoop = 0x98fe;
    /// <summary><c>InstList_DraygonBody_FacingLeft_Roar</c> at $A5:9922.</summary>
    public const ushort BodyFacingLeftRoar = 0x9922;
    /// <summary><c>InstList_DraygonEye_FacingLeft_Idle</c> at $A5:9944.</summary>
    public const ushort EyeFacingLeftIdle = 0x9944;
    /// <summary><c>InstList_DraygonEye_FacingLeft_Dying_0</c> at $A5:997A.</summary>
    public const ushort EyeFacingLeftDying = 0x997a;
    /// <summary><c>InstList_DraygonEye_FacingLeft_Dead</c> at $A5:999C.</summary>
    public const ushort EyeFacingLeftDead = 0x999c;
    /// <summary><c>InstList_DraygonEye_FacingLeft_LookingLeft</c> at $A5:99AE.</summary>
    public const ushort EyeFacingLeftLookingLeft = 0x99ae;
    /// <summary><c>InstList_DraygonEye_FacingLeft_LookingRight</c> at $A5:99B4.</summary>
    public const ushort EyeFacingLeftLookingRight = 0x99b4;
    /// <summary><c>InstList_DraygonEye_FacingLeft_LookingUp</c> at $A5:99BA.</summary>
    public const ushort EyeFacingLeftLookingUp = 0x99ba;
    /// <summary><c>InstList_DraygonEye_FacingLeft_LookingDown</c> at $A5:99C0.</summary>
    public const ushort EyeFacingLeftLookingDown = 0x99c0;
    /// <summary><c>InstList_DraygonTail_FacingLeft_FakeTailWhip</c> at $A5:99FC.</summary>
    public const ushort TailFacingLeftInitialFakeWhip = 0x99fc;
    /// <summary><c>InstList_DraygonTail_FacingLeft_FinalTailWhips_0</c> at $A5:9A68.</summary>
    public const ushort TailFacingLeftFinalWhips = 0x9a68;
    /// <summary><c>InstList_DraygonTail_FacingLeft_TailWhip</c> at $A5:9AE8.</summary>
    public const ushort TailFacingLeftWhip = 0x9ae8;
    /// <summary><c>InstList_DraygonTail_FacingLeft_TailFlail</c> at $A5:9B5A.</summary>
    public const ushort TailFacingLeftFlail = 0x9b5a;
    /// <summary><c>InstList_DraygonArms_FacingRight_Idle_0</c> at $A5:9BDA.</summary>
    public const ushort ArmsFacingRightIdle = 0x9bda;
    /// <summary><c>InstList_DraygonArms_FacingRight_NearSwoopApex</c> at $A5:9C06.</summary>
    public const ushort ArmsFacingRightNearSwoopApex = 0x9c06;
    /// <summary><c>InstList_DraygonArms_FacingRight_Grab</c> at $A5:9C38.</summary>
    public const ushort ArmsFacingRightGrab = 0x9c38;
    /// <summary><c>InstList_DraygonArms_FacingRight_Dying_0</c> at $A5:9C5A.</summary>
    public const ushort ArmsFacingRightDying = 0x9c5a;
    /// <summary><c>InstList_DraygonBody_FacingRight_FireGoop</c> at $A5:9C90.</summary>
    public const ushort BodyFacingRightFireGoop = 0x9c90;
    /// <summary><c>InstList_DraygonBody_FacingRight_Roar</c> at $A5:9CB4.</summary>
    public const ushort BodyFacingRightRoar = 0x9cb4;
    /// <summary><c>InstList_DraygonEye_FacingRight_Dying_0</c> at $A5:9D1C.</summary>
    public const ushort EyeFacingRightDying = 0x9d1c;
    /// <summary><c>InstList_DraygonEye_FacingRight_Dead</c> at $A5:9D3E.</summary>
    public const ushort EyeFacingRightDead = 0x9d3e;
    /// <summary><c>InstList_DraygonEye_FacingRight_LookingRight</c> at $A5:9D50.</summary>
    public const ushort EyeFacingRightLookingRight = 0x9d50;
    /// <summary><c>InstList_DraygonEye_FacingRight_LookingLeft</c> at $A5:9D56.</summary>
    public const ushort EyeFacingRightLookingLeft = 0x9d56;
    /// <summary><c>InstList_DraygonEye_FacingRight_LookingUp</c> at $A5:9D5C.</summary>
    public const ushort EyeFacingRightLookingUp = 0x9d5c;
    /// <summary><c>InstList_DraygonEye_FacingRight_LookingDown</c> at $A5:9D62.</summary>
    public const ushort EyeFacingRightLookingDown = 0x9d62;
    /// <summary><c>InstList_DraygonTail_FacingRight_FinalTailWhips_0</c> at $A5:9E21.</summary>
    public const ushort TailFacingRightFinalWhips = 0x9e21;
    /// <summary><c>InstList_DraygonTail_FacingRight_TailWhip_0</c> at $A5:9EA1.</summary>
    public const ushort TailFacingRightWhip = 0x9ea1;
    /// <summary><c>InstList_DraygonTail_FacingRight_TailFlail_0</c> at $A5:9F15.</summary>
    public const ushort TailFacingRightFlail = 0x9f15;

    /// <summary><c>Function_DraygonBody_GrabbedSamus_FlailTail_FlyStraightUp</c> at $A5:9128.</summary>
    private const ushort BodyGrabbedSamusFlailTailFlyStraightUpFunction = 0x9128;
    /// <summary><c>Instruction_Draygon_SetInstList_Body_Eye_Tail_Arms</c> at $A5:94DD.</summary>
    private const ushort SetInstListBodyEyeTailArms = 0x94dd;
    /// <summary><c>Instruction_Draygon_FunctionInY</c> at $A5:9736.</summary>
    private const ushort FunctionInY = 0x9736;
    /// <summary><c>Inst_Draygon_SpawnDyingDraygonSpriteObject_BigDustCloud</c> at $A5:973F.</summary>
    private const ushort InstDraygonSpawnDyingDraygonSpriteObjectBigDustCloud = 0x973f;
    /// <summary><c>Inst_Draygon_SpawnDyingDraygonSpriteObject_SmallExplosion</c> at $A5:9752.</summary>
    private const ushort InstDraygonSpawnDyingDraygonSpriteObjectSmallExplosion = 0x9752;
    /// <summary><c>Inst_Draygon_SpawnDyingDraygonSpriteObject_BigExplosion</c> at $A5:9765.</summary>
    private const ushort InstDraygonSpawnDyingDraygonSpriteObjectBigExplosion = 0x9765;
    /// <summary><c>Inst_Draygon_SpawnDyingDraygonSpriteObject_BreathBubbles</c> at $A5:9778.</summary>
    private const ushort InstDraygonSpawnDyingDraygonSpriteObjectBreathBubbles = 0x9778;
    /// <summary><c>Instruction_Draygon_RoomLoadingInterruptCmd_BeginHUDDraw</c> at $A5:9895.</summary>
    private const ushort RoomLoadingInterruptCmdBeginHUDDraw = 0x9895;
    /// <summary><c>InstList_DraygonBody_Dying_0</c> at $A5:989B.</summary>
    private const ushort BodyDying0 = 0x989b;
    /// <summary><c>InstList_DraygonBody_Dying_1</c> at $A5:98A5.</summary>
    private const ushort BodyDying1 = 0x98a5;
    /// <summary><c>InstList_DraygonBody_Dying_2</c> at $A5:98BF.</summary>
    private const ushort BodyDying2 = 0x98bf;
    /// <summary><c>Instruction_Draygon_ParalyseDraygonTailAndArms</c> at $A5:98D3.</summary>
    private const ushort ParalyseDraygonTailAndArms = 0x98d3;
    /// <summary><c>Instruction_DraygonBody_SetAsIntangible</c> at $A5:98EF.</summary>
    private const ushort BodySetAsIntangible = 0x98ef;
    /// <summary><c>InstList_DraygonEye_FacingLeft_Dying_1</c> at $A5:997E.</summary>
    private const ushort EyeFacingLeftDying1 = 0x997e;
    /// <summary><c>InstList_DraygonTail_FacingLeft_Idle_0</c> at $A5:99C6.</summary>
    private const ushort TailFacingLeftIdle0 = 0x99c6;
    /// <summary><c>InstList_DraygonTail_FacingLeft_FinalTailWhips_1</c> at $A5:9A6C.</summary>
    private const ushort TailFacingLeftFinalTailWhips1 = 0x9a6c;
    /// <summary><c>Instruction_DraygonTail_TailWhipHit</c> at $A5:9B9A.</summary>
    private const ushort TailTailWhipHit = 0x9b9a;
    /// <summary><c>InstList_DraygonBody_FacingRight_Idle</c> at $A5:9C7E.</summary>
    private const ushort BodyFacingRightIdle = 0x9c7e;
    /// <summary><c>Instruction_Draygon_RoomLoadingInterruptCmd_BeginHUDDraw_dup</c> at $A5:9C8A.</summary>
    private const ushort RoomLoadingInterruptCmdBeginHUDDrawDup = 0x9c8a;
    /// <summary><c>InstList_DraygonEye_FacingRight_Idle</c> at $A5:9CD6.</summary>
    private const ushort EyeFacingRightIdle = 0x9cd6;
    /// <summary><c>InstList_DraygonEye_FacingRight_Dying_1</c> at $A5:9D20.</summary>
    private const ushort EyeFacingRightDying1 = 0x9d20;
    /// <summary><c>InstList_DraygonTail_FacingRight_Idle_0</c> at $A5:9D68.</summary>
    private const ushort TailFacingRightIdle0 = 0x9d68;
    /// <summary><c>Instruction_DraygonBody_DisplaceGraphics</c> at $A5:9E0A.</summary>
    private const ushort BodyDisplaceGraphics = 0x9e0a;
    /// <summary><c>InstList_DraygonTail_FacingRight_FinalTailWhips_1</c> at $A5:9E25.</summary>
    private const ushort TailFacingRightFinalTailWhips1 = 0x9e25;
    /// <summary><c>Instruction_Draygon_BodyFunctionInY</c> at $A5:9F57.</summary>
    private const ushort BodyFunctionInY = 0x9f57;
    /// <summary><c>Instruction_Draygon_QueueSFXInY_Lib2_Max6</c> at $A5:9F60.</summary>
    private const ushort QueueSFXInYLib2Max6 = 0x9f60;
    /// <summary><c>Instruction_Draygon_QueueSFXInY_Lib3_Max6</c> at $A5:9F6E.</summary>
    private const ushort QueueSFXInYLib3Max6 = 0x9f6e;
    /// <summary><c>Instruction_Draygon_SpawnGoop_Leftwards</c> at $A5:9F7C.</summary>
    private const ushort SpawnGoopLeftwards = 0x9f7c;
    /// <summary><c>Instruction_Draygon_SpawnGoop_Rightwards</c> at $A5:9FAE.</summary>
    private const ushort SpawnGoopRightwards = 0x9fae;
    /// <summary><c>Instruction_Draygon_EyeFunctionInY</c> at $A5:C47B.</summary>
    private const ushort EyeFunctionInY = 0xc47b;
    /// <summary><c>Function_DraygonEye_FacingLeft</c> at $A5:C48D.</summary>
    private const ushort EyeFacingLeftFunction = 0xc48d;
    /// <summary><c>Function_DraygonEye_FacingRight</c> at $A5:C513.</summary>
    private const ushort EyeFacingRightFunction = 0xc513;

    /// <summary>Native program bank $A5.</summary>
    internal const byte Bank = 0xa5;

    /// <summary>Compiled address layout for Draygon's body and component instruction programs, including mechanics and presentation slots.</summary>
    internal static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0x97b9),
        Entry(Sleep),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(BodyFacingLeftReset),
        Op(SetInstListBodyEyeTailArms, BodyFacingLeftIdle, EyeFacingLeftIdle, TailFacingLeftIdle0, ArmsFacingLeftIdle),
        Op(RoomLoadingInterruptCmdBeginHUDDraw),
        Op(EyeFunctionInY, EyeFacingLeftFunction),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(BodyFacingRightReset),
        Op(SetInstListBodyEyeTailArms, BodyFacingRightIdle, EyeFacingRightIdle, TailFacingRightIdle0, ArmsFacingRightIdle),
        Op(RoomLoadingInterruptCmdBeginHUDDraw),
        Op(EyeFunctionInY, EyeFacingRightFunction),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(ArmsFacingLeftIdle),
        Frame(5),
        Frame(5),
        Frame(5),
        Frame(5),
        Frame(5),
        Frame(5),
        Op(CommonEnemyInstructionCodes.Goto, ArmsFacingLeftIdle),
        Origin(0x9813),
        Entry(ArmsFacingLeftNearSwoopApex),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(64),
        Op(CommonEnemyInstructionCodes.Sleep),
        Origin(0x9845),
        Entry(ArmsFacingLeftGrab),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(8),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(ArmsFacingLeftDying),
        Frame(5),
        Frame(5),
        Frame(5),
        Frame(5),
        Op(CommonEnemyInstructionCodes.Goto, BodyDying0),
        Origin(0x9889),
        Entry(BodyFacingLeftIdle),
        Op(RoomLoadingInterruptCmdBeginHUDDraw),
        Op(EyeFunctionInY, EyeFacingLeftFunction),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Skip(6),
        Op(QueueSFXInYLib3Max6, 0x001b),
        Op(BodySetAsIntangible),
        Op(CommonEnemyInstructionCodes.SetTimer, 0x0008),
        Op(CommonEnemyInstructionCodes.WaitFrames, 0x000c, InstDraygonSpawnDyingDraygonSpriteObjectBigExplosion, InstDraygonSpawnDyingDraygonSpriteObjectSmallExplosion, InstDraygonSpawnDyingDraygonSpriteObjectBigDustCloud, InstDraygonSpawnDyingDraygonSpriteObjectBreathBubbles),
        Op(QueueSFXInYLib2Max6, 0x0025),
        Op(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate, BodyDying1),
        Op(CommonEnemyInstructionCodes.WaitFrames, 0x0001),
        Op(ParalyseDraygonTailAndArms),
        Op(CommonEnemyInstructionCodes.WaitFrames, 0x0010, InstDraygonSpawnDyingDraygonSpriteObjectBigExplosion, InstDraygonSpawnDyingDraygonSpriteObjectSmallExplosion, InstDraygonSpawnDyingDraygonSpriteObjectBigDustCloud, InstDraygonSpawnDyingDraygonSpriteObjectBreathBubbles),
        Op(QueueSFXInYLib2Max6, 0x0025),
        Op(CommonEnemyInstructionCodes.Goto, BodyDying2),
        Origin(0x98ed),
        Entry(Delete),
        Op(CommonEnemyInstructionCodes.StopScript),
        Origin(0x98fe),
        Entry(BodyFacingLeftFireGoop),
        Frame(1),
        Frame(2),
        Frame(3),
        Op(SpawnGoopLeftwards),
        Op(QueueSFXInYLib2Max6, 0x004c),
        Frame(3),
        Frame(2),
        Frame(2),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(BodyFacingLeftRoar),
        Op(QueueSFXInYLib2Max6, 0x0073),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(EyeFacingLeftIdle),
        Frame(21),
        Frame(5),
        Frame(5),
        Frame(10),
        Frame(10),
        Frame(10),
        Frame(10),
        Frame(10),
        Frame(10),
        Frame(5),
        Frame(5),
        Frame(5),
        Op(FunctionInY, EyeFacingLeftFunction),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(EyeFacingLeftDying),
        Op(CommonEnemyInstructionCodes.SetTimer, 0x0004),
        Frame(4),
        Frame(4),
        Frame(4),
        Frame(4),
        Op(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate, EyeFacingLeftDying1),
        Frame(32),
        Frame(16),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(EyeFacingLeftDead),
        Frame(32),
        Frame(32),
        Frame(32),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(EyeFacingLeftLookingLeft),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(EyeFacingLeftLookingRight),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(EyeFacingLeftLookingUp),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(EyeFacingLeftLookingDown),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Frame(8),
        Frame(7),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(7),
        Op(CommonEnemyInstructionCodes.Goto, TailFacingLeftIdle0),
        Skip(2),
        Entry(TailFacingLeftInitialFakeWhip),
        Op(BodyDisplaceGraphics, 0xffff, 0xffff),
        Frame(16),
        Op(BodyDisplaceGraphics, 0xfffe, 0xfffe),
        Frame(6),
        Op(BodyDisplaceGraphics, 0xfffd, 0xfffd),
        Frame(5),
        Op(BodyDisplaceGraphics, 0xfffc, 0xfffc),
        Frame(4),
        Op(BodyDisplaceGraphics, 0xfffb, 0xfffb),
        Frame(3),
        Op(BodyDisplaceGraphics, 0xfffa, 0xfffa),
        Frame(2),
        Op(BodyDisplaceGraphics, 0xfff8, 0xfff8),
        Frame(1),
        Op(BodyDisplaceGraphics, 0x0000, 0x0000),
        Frame(16),
        Frame(1),
        Frame(2),
        Frame(3),
        Frame(4),
        Frame(5),
        Frame(6),
        Op(CommonEnemyInstructionCodes.Goto, TailFacingLeftIdle0),
        Entry(TailFacingLeftFinalWhips),
        Op(CommonEnemyInstructionCodes.SetTimer, 0x0004),
        Op(BodyDisplaceGraphics, 0xffff, 0xffff),
        Frame(2),
        Op(BodyDisplaceGraphics, 0xfffe, 0xfffe),
        Frame(6),
        Op(BodyDisplaceGraphics, 0xfffd, 0xfffd),
        Frame(5),
        Op(BodyDisplaceGraphics, 0xfffc, 0xfffc),
        Frame(4),
        Op(BodyDisplaceGraphics, 0xfffb, 0xfffb),
        Frame(3),
        Op(BodyDisplaceGraphics, 0xfffa, 0xfffa),
        Frame(2),
        Op(BodyDisplaceGraphics, 0xfff8, 0xfff8),
        Frame(1),
        Op(BodyDisplaceGraphics, 0x0000, 0x0000),
        Op(TailTailWhipHit),
        Op(QueueSFXInYLib2Max6, 0x0025),
        Frame(3),
        Frame(1),
        Frame(2),
        Frame(3),
        Frame(4),
        Frame(5),
        Frame(6),
        Op(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate, TailFacingLeftFinalTailWhips1),
        Op(BodyFunctionInY, BodyGrabbedSamusFlailTailFlyStraightUpFunction),
        Op(CommonEnemyInstructionCodes.Goto, TailFacingLeftIdle0),
        Skip(2),
        Entry(TailFacingLeftWhip),
        Op(BodyDisplaceGraphics, 0xffff, 0xffff),
        Frame(2),
        Op(BodyDisplaceGraphics, 0xfffe, 0xfffe),
        Frame(6),
        Op(BodyDisplaceGraphics, 0xfffd, 0xfffd),
        Frame(5),
        Op(BodyDisplaceGraphics, 0xfffc, 0xfffc),
        Frame(4),
        Op(BodyDisplaceGraphics, 0xfffb, 0xfffb),
        Frame(3),
        Op(BodyDisplaceGraphics, 0xfffa, 0xfffa),
        Frame(2),
        Op(BodyDisplaceGraphics, 0xfff8, 0xfff8),
        Frame(1),
        Op(BodyDisplaceGraphics, 0x0000, 0x0000),
        Op(TailTailWhipHit),
        Op(QueueSFXInYLib2Max6, 0x0025),
        Frame(3),
        Frame(1),
        Frame(2),
        Frame(3),
        Frame(4),
        Frame(5),
        Frame(6),
        Op(CommonEnemyInstructionCodes.Goto, TailFacingLeftIdle0),
        Entry(TailFacingLeftFlail),
        Frame(2),
        Frame(6),
        Frame(5),
        Frame(4),
        Frame(3),
        Frame(2),
        Frame(1),
        Op(QueueSFXInYLib2Max6, 0x0025),
        Frame(3),
        Frame(1),
        Frame(2),
        Frame(3),
        Frame(4),
        Frame(5),
        Frame(6),
        Op(CommonEnemyInstructionCodes.Goto, TailFacingLeftIdle0),
        Origin(0x9bda),
        Entry(ArmsFacingRightIdle),
        Frame(5),
        Frame(5),
        Frame(5),
        Frame(5),
        Frame(5),
        Frame(5),
        Op(CommonEnemyInstructionCodes.Goto, ArmsFacingRightIdle),
        Origin(0x9c06),
        Entry(ArmsFacingRightNearSwoopApex),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(64),
        Op(CommonEnemyInstructionCodes.Sleep),
        Origin(0x9c38),
        Entry(ArmsFacingRightGrab),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(8),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(ArmsFacingRightDying),
        Frame(5),
        Frame(5),
        Frame(5),
        Frame(5),
        Op(CommonEnemyInstructionCodes.Goto, BodyDying0),
        Origin(0x9c7e),
        Op(RoomLoadingInterruptCmdBeginHUDDrawDup),
        Op(EyeFunctionInY, EyeFacingRightFunction),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Skip(6),
        Entry(BodyFacingRightFireGoop),
        Frame(1),
        Frame(2),
        Frame(3),
        Op(SpawnGoopRightwards),
        Op(QueueSFXInYLib2Max6, 0x004c),
        Frame(3),
        Frame(2),
        Frame(2),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(BodyFacingRightRoar),
        Op(QueueSFXInYLib2Max6, 0x0073),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Op(CommonEnemyInstructionCodes.Sleep),
        Frame(21),
        Frame(5),
        Frame(5),
        Frame(10),
        Frame(10),
        Frame(10),
        Frame(10),
        Frame(10),
        Frame(10),
        Frame(5),
        Frame(5),
        Frame(5),
        Op(FunctionInY, EyeFacingLeftFunction),
        Op(CommonEnemyInstructionCodes.Sleep),
        Origin(0x9d1c),
        Entry(EyeFacingRightDying),
        Op(CommonEnemyInstructionCodes.SetTimer, 0x0004),
        Frame(4),
        Frame(4),
        Frame(4),
        Frame(4),
        Op(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate, EyeFacingRightDying1),
        Frame(32),
        Frame(16),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(EyeFacingRightDead),
        Frame(32),
        Frame(32),
        Frame(32),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(EyeFacingRightLookingRight),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(EyeFacingRightLookingLeft),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(EyeFacingRightLookingUp),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(EyeFacingRightLookingDown),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Frame(8),
        Frame(7),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(6),
        Frame(7),
        Op(CommonEnemyInstructionCodes.Goto, TailFacingRightIdle0),
        Origin(0x9e21),
        Entry(TailFacingRightFinalWhips),
        Op(CommonEnemyInstructionCodes.SetTimer, 0x0004),
        Op(BodyDisplaceGraphics, 0x0001, 0xffff),
        Frame(2),
        Op(BodyDisplaceGraphics, 0x0002, 0xfffe),
        Frame(6),
        Op(BodyDisplaceGraphics, 0x0003, 0xfffd),
        Frame(5),
        Op(BodyDisplaceGraphics, 0x0004, 0xfffc),
        Frame(4),
        Op(BodyDisplaceGraphics, 0x0005, 0xfffb),
        Frame(3),
        Op(BodyDisplaceGraphics, 0x0006, 0xfffa),
        Frame(2),
        Op(BodyDisplaceGraphics, 0x0008, 0xfff8),
        Frame(1),
        Op(BodyDisplaceGraphics, 0x0000, 0x0000),
        Op(TailTailWhipHit),
        Op(QueueSFXInYLib2Max6, 0x0025),
        Frame(3),
        Frame(1),
        Frame(2),
        Frame(3),
        Frame(4),
        Frame(5),
        Frame(6),
        Op(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate, TailFacingRightFinalTailWhips1),
        Op(BodyFunctionInY, BodyGrabbedSamusFlailTailFlyStraightUpFunction),
        Op(CommonEnemyInstructionCodes.Goto, TailFacingRightIdle0),
        Skip(2),
        Entry(TailFacingRightWhip),
        Op(BodyDisplaceGraphics, 0x0001, 0xffff),
        Frame(2),
        Op(BodyDisplaceGraphics, 0x0002, 0xfffe),
        Frame(6),
        Op(BodyDisplaceGraphics, 0x0003, 0xfffd),
        Frame(5),
        Op(BodyDisplaceGraphics, 0x0004, 0xfffc),
        Frame(4),
        Op(BodyDisplaceGraphics, 0x0005, 0xfffb),
        Frame(3),
        Op(BodyDisplaceGraphics, 0x0006, 0xfffa),
        Frame(2),
        Op(BodyDisplaceGraphics, 0x0008, 0xfff8),
        Frame(1),
        Op(BodyDisplaceGraphics, 0x0000, 0x0000),
        Op(TailTailWhipHit),
        Op(QueueSFXInYLib2Max6, 0x0025),
        Frame(3),
        Frame(1),
        Frame(2),
        Frame(3),
        Frame(4),
        Frame(5),
        Frame(6),
        Op(CommonEnemyInstructionCodes.Goto, TailFacingRightIdle0),
        Skip(2),
        Entry(TailFacingRightFlail),
        Frame(2),
        Frame(6),
        Frame(5),
        Frame(4),
        Frame(3),
        Frame(2),
        Frame(1),
        Op(QueueSFXInYLib2Max6, 0x0025),
        Frame(3),
        Frame(1),
        Frame(2),
        Frame(3),
        Frame(4),
        Frame(5),
        Frame(6),
        Op(CommonEnemyInstructionCodes.Goto, TailFacingRightIdle0));

    /// <summary>Reads a word only when its compiled Draygon instruction slot is owned by mechanics.</summary>
    /// <param name="address">Bank-local address of the candidate instruction word.</param>
    /// <returns>The compiled mechanics value at that address.</returns>
    /// <exception cref="InvalidDataException">The address is outside the layout or names a presentation operand rather than mechanics data.</exception>
    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"Draygon instruction mechanics pointer $A5:{address:X4} is not compiled.");
}
