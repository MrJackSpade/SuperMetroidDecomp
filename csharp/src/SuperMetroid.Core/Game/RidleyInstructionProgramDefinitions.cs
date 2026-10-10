using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for the shared Ceres and Lower Norfair Ridley programs.
/// Interleaved extended-spritemap pointers remain live cartridge presentation data.
/// </summary>
internal abstract class RidleyInstructionProgramDefinitions
{
    /// <summary><c>InstList_Ridley_FacingLeft_Initial</c> at $A6:E538.</summary>
    public const ushort Initial = 0xe538;
    /// <summary><c>InstList_RidleyCeres_FacingLeft_Lunging</c> at $A6:E548.</summary>
    public const ushort CeresLunge = 0xe548;
    /// <summary><c>InstList_RidleyCeres_RetrieveBabyMetroid</c> at $A6:E658.</summary>
    public const ushort RetrieveBabyMetroid = 0xe658;
    /// <summary><c>InstList_Ridley_FacingLeft_OpeningRoar</c> at $A6:E690.</summary>
    public const ushort OpeningRoar = 0xe690;
    /// <summary><c>InstList_Ridley_FacingLeft_DeathRoar</c> at $A6:E6C8.</summary>
    public const ushort DeathRoar = 0xe6c8;
    /// <summary><c>InstList_Ridley_TurnFromLeftToRight</c> at $A6:E6F0.</summary>
    public const ushort TurnFromLeftToRight = 0xe6f0;
    /// <summary><c>InstList_Ridley_TurnFromRightToLeft</c> at $A6:E706.</summary>
    public const ushort TurnFromRightToLeft = 0xe706;
    /// <summary><c>InstList_Ridley_FacingLeft_Fireballing_0</c> at $A6:E73A.</summary>
    public const ushort Fireballing = 0xe73a;
    /// <summary><c>InstList_RidleyCeres_FacingLeft_TransitionToFlying</c> at $A6:E91D.</summary>
    public const ushort TransitionToFlying = 0xe91d;

    /// <summary><c>Instruction_Ridley_Roar</c> at $A6:E4BE.</summary>
    private const ushort Roar = 0xe4be;
    /// <summary><c>Instruction_Ridley_ClearRoaringFlag</c> at $A6:E4CA.</summary>
    private const ushort ClearRoaringFlag = 0xe4ca;
    /// <summary><c>Instruction_Ridley_GotoYIfNotNorfairAndSamusHasLowEnergy</c> at $A6:E4D2.</summary>
    private const ushort GotoYIfNotNorfairAndSamusHasLowEnergy = 0xe4d2;
    /// <summary><c>Instruction_RidleyCeres_RidleyFeetDistanceIndexInY</c> at $A6:E501.</summary>
    private const ushort CeresRidleyFeetDistanceIndexInY = 0xe501;
    /// <summary><c>Instruction_Ridley_GotoYIfNotFacingLeft</c> at $A6:E517.</summary>
    private const ushort GotoYIfNotFacingLeft = 0xe517;
    /// <summary><c>Instruction_Ridley_MoveRidleyWithArgsInY</c> at $A6:E51F.</summary>
    private const ushort MoveRidleyWithArgsInY = 0xe51f;
    /// <summary><c>InstList_Ridley_FacingRight_Initial</c> at $A6:E542.</summary>
    private const ushort FacingRightInitial = 0xe542;
    /// <summary><c>UNUSED_InstList_RidleyCeres_FacingRight_Lunging_A6E576</c> at $A6:E576.</summary>
    private const ushort UNUSEDInstListRidleyCeresFacingRightLungingA6E576 = 0xe576;
    /// <summary><c>UNUSED_InstList_RidleyCeres_FacingRight_RetrieveBabyMetroid_A6E676</c> at $A6:E676.</summary>
    private const ushort UNUSEDInstListRidleyCeresFacingRightRetrieveBabyMetroidA6E676 = 0xe676;
    /// <summary><c>InstList_Ridley_FacingRight_OpeningRoar</c> at $A6:E6AE.</summary>
    private const ushort FacingRightOpeningRoar = 0xe6ae;
    /// <summary><c>InstList_Ridley_FacingRight_DeathRoar</c> at $A6:E6DE.</summary>
    private const ushort FacingRightDeathRoar = 0xe6de;
    /// <summary><c>Instruction_Ridley_FlipRidleyLeft</c> at $A6:E71C.</summary>
    private const ushort FlipRidleyLeft = 0xe71c;
    /// <summary><c>Instruction_Ridley_FaceRidleyForward</c> at $A6:E727.</summary>
    private const ushort FaceRidleyForward = 0xe727;
    /// <summary><c>Instruction_Ridley_FlipRidleyRight</c> at $A6:E72F.</summary>
    private const ushort FlipRidleyRight = 0xe72f;
    /// <summary><c>InstList_Ridley_FacingLeft_Fireballing_1</c> at $A6:E7AC.</summary>
    private const ushort FacingLeftFireballing1 = 0xe7ac;
    /// <summary><c>InstList_Ridley_FacingRight_Fireballing_0</c> at $A6:E7B4.</summary>
    private const ushort FacingRightFireballing0 = 0xe7b4;
    /// <summary><c>InstList_Ridley_FacingRight_Fireballing_1</c> at $A6:E820.</summary>
    private const ushort FacingRightFireballing1 = 0xe820;
    /// <summary><c>Instruction_Ridley_CalculateFireballXYVelocities</c> at $A6:E84D.</summary>
    private const ushort CalculateFireballXYVelocities = 0xe84d;
    /// <summary><c>Instruction_Ridley_SpawnRidleysFireballWithAfterburn</c> at $A6:E904.</summary>
    private const ushort SpawnRidleysFireballWithAfterburn = 0xe904;
    /// <summary><c>Instruction_Ridley_SpawnRidleysFireballWithoutAfterburn</c> at $A6:E909.</summary>
    private const ushort SpawnRidleysFireballWithoutAfterburn = 0xe909;
    /// <summary><c>InstList_RidleyCeres_FacingRight_TransitionToFlying</c> at $A6:E945.</summary>
    private const ushort CeresFacingRightTransitionToFlying = 0xe945;
    /// <summary><c>Instruction_RidleyCeres_StartLiftoff</c> at $A6:E969.</summary>
    private const ushort CeresStartLiftoff = 0xe969;
    /// <summary><c>Instruction_Ridley_StartLiftoff</c> at $A6:E976.</summary>
    private const ushort StartLiftoff = 0xe976;

    /// <summary>Native program bank $A6.</summary>
    internal const byte Bank = 0xa6;

    /// <summary>Address-ordered layout of Ridley's compiled instruction words, distinguishing mechanics from live visual operands.</summary>
    internal static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xe538),
        Entry(Initial),
        Op(GotoYIfNotFacingLeft, FacingRightInitial),
        Frame(12),
        Op(CommonEnemyInstructionCodes.Sleep),
        Frame(12),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(CeresLunge),
        Op(GotoYIfNotFacingLeft, UNUSEDInstListRidleyCeresFacingRightLungingA6E576),
        Op(CeresRidleyFeetDistanceIndexInY, 0x0000),
        Frame(4),
        Op(CeresRidleyFeetDistanceIndexInY, 0x0002),
        Frame(6),
        Op(CeresRidleyFeetDistanceIndexInY, 0x0004),
        Frame(80),
        Op(CeresRidleyFeetDistanceIndexInY, 0x0002),
        Frame(6),
        Op(CeresRidleyFeetDistanceIndexInY, 0x0000),
        Frame(4),
        Op(CommonEnemyInstructionCodes.Sleep),
        Op(CeresRidleyFeetDistanceIndexInY, 0x0000),
        Frame(4),
        Op(CeresRidleyFeetDistanceIndexInY, 0x0002),
        Frame(6),
        Op(CeresRidleyFeetDistanceIndexInY, 0x0004),
        Frame(80),
        Op(CeresRidleyFeetDistanceIndexInY, 0x0002),
        Frame(6),
        Op(CeresRidleyFeetDistanceIndexInY, 0x0000),
        Frame(4),
        Op(CommonEnemyInstructionCodes.Sleep),
        Origin(0xe658),
        Entry(RetrieveBabyMetroid),
        Op(GotoYIfNotFacingLeft, UNUSEDInstListRidleyCeresFacingRightRetrieveBabyMetroidA6E676),
        Op(CeresRidleyFeetDistanceIndexInY, 0x0000),
        Frame(4),
        Op(CeresRidleyFeetDistanceIndexInY, 0x0002),
        Frame(6),
        Op(CeresRidleyFeetDistanceIndexInY, 0x0004),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Op(CeresRidleyFeetDistanceIndexInY, 0x0000),
        Frame(4),
        Op(CeresRidleyFeetDistanceIndexInY, 0x0002),
        Frame(6),
        Op(CeresRidleyFeetDistanceIndexInY, 0x0004),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(OpeningRoar),
        Op(GotoYIfNotFacingLeft, FacingRightOpeningRoar),
        Frame(6),
        Op(Roar),
        Frame(8),
        Frame(96),
        Frame(8),
        Op(ClearRoaringFlag),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Frame(6),
        Op(Roar),
        Frame(8),
        Frame(96),
        Frame(8),
        Op(ClearRoaringFlag),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(DeathRoar),
        Op(GotoYIfNotFacingLeft, FacingRightDeathRoar),
        Frame(6),
        Op(Roar),
        Frame(8),
        Frame(16),
        Op(ClearRoaringFlag),
        Op(CommonEnemyInstructionCodes.Sleep),
        Frame(6),
        Op(Roar),
        Frame(8),
        Frame(16),
        Op(ClearRoaringFlag),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(TurnFromLeftToRight),
        Op(FaceRidleyForward),
        Frame(1),
        Frame(8),
        Op(FlipRidleyRight),
        Frame(1),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(TurnFromRightToLeft),
        Op(FaceRidleyForward),
        Frame(1),
        Frame(8),
        Op(FlipRidleyLeft),
        Frame(1),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Origin(0xe73a),
        Entry(Fireballing),
        Op(GotoYIfNotFacingLeft, FacingRightFireballing0),
        Op(GotoYIfNotNorfairAndSamusHasLowEnergy, FacingLeftFireballing1),
        Frame(8),
        Op(Roar),
        Frame(8),
        Frame(2),
        Op(CalculateFireballXYVelocities),
        Op(SpawnRidleysFireballWithAfterburn),
        Frame(5),
        Op(CalculateFireballXYVelocities),
        Op(SpawnRidleysFireballWithoutAfterburn),
        Frame(5),
        Op(CalculateFireballXYVelocities),
        Op(SpawnRidleysFireballWithoutAfterburn),
        Frame(5),
        Op(CalculateFireballXYVelocities),
        Op(SpawnRidleysFireballWithoutAfterburn),
        Frame(48),
        Frame(8),
        Op(GotoYIfNotNorfairAndSamusHasLowEnergy, FacingLeftFireballing1),
        Frame(32),
        Op(CalculateFireballXYVelocities),
        Op(Roar),
        Frame(8),
        Frame(2),
        Op(CalculateFireballXYVelocities),
        Op(SpawnRidleysFireballWithAfterburn),
        Frame(5),
        Op(CalculateFireballXYVelocities),
        Op(SpawnRidleysFireballWithoutAfterburn),
        Frame(5),
        Op(CalculateFireballXYVelocities),
        Op(SpawnRidleysFireballWithoutAfterburn),
        Frame(5),
        Op(CalculateFireballXYVelocities),
        Op(SpawnRidleysFireballWithoutAfterburn),
        Frame(48),
        Frame(8),
        Op(ClearRoaringFlag),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Op(GotoYIfNotNorfairAndSamusHasLowEnergy, FacingRightFireballing1),
        Frame(8),
        Op(Roar),
        Frame(8),
        Frame(2),
        Op(CalculateFireballXYVelocities),
        Op(SpawnRidleysFireballWithAfterburn),
        Frame(5),
        Op(CalculateFireballXYVelocities),
        Op(SpawnRidleysFireballWithoutAfterburn),
        Frame(5),
        Op(CalculateFireballXYVelocities),
        Op(SpawnRidleysFireballWithoutAfterburn),
        Frame(5),
        Op(CalculateFireballXYVelocities),
        Op(SpawnRidleysFireballWithoutAfterburn),
        Frame(48),
        Frame(8),
        Op(GotoYIfNotNorfairAndSamusHasLowEnergy, FacingRightFireballing1),
        Frame(32),
        Op(Roar),
        Frame(8),
        Frame(2),
        Op(CalculateFireballXYVelocities),
        Op(SpawnRidleysFireballWithAfterburn),
        Frame(5),
        Op(CalculateFireballXYVelocities),
        Op(SpawnRidleysFireballWithoutAfterburn),
        Frame(5),
        Op(CalculateFireballXYVelocities),
        Op(SpawnRidleysFireballWithoutAfterburn),
        Frame(5),
        Op(CalculateFireballXYVelocities),
        Op(SpawnRidleysFireballWithoutAfterburn),
        Frame(48),
        Frame(8),
        Op(ClearRoaringFlag),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Origin(0xe91d),
        Entry(TransitionToFlying),
        Op(GotoYIfNotFacingLeft, CeresFacingRightTransitionToFlying),
        Frame(3),
        Op(MoveRidleyWithArgsInY, 0x0001, 0xfff4),
        Frame(4),
        Op(MoveRidleyWithArgsInY, 0xfffc, 0xfff8),
        Frame(5),
        Op(CeresStartLiftoff),
        Frame(17),
        Frame(17),
        Op(CommonEnemyInstructionCodes.Sleep),
        Frame(3),
        Op(MoveRidleyWithArgsInY, 0xffff, 0xfff4),
        Frame(4),
        Op(MoveRidleyWithArgsInY, 0x0004, 0xfff8),
        Frame(5),
        Op(StartLiftoff),
        Frame(17),
        Frame(17),
        Op(CommonEnemyInstructionCodes.Sleep));

    /// <summary>Reads one compiled mechanics word and rejects presentation or foreign data.</summary>
    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"Ridley instruction mechanics pointer $A6:{address:X4} is not compiled.");
}
