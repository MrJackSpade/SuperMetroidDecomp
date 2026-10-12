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
    /// <summary><c>InstList_Ridley_FacingLeft_Fireballing_1</c> at $A6:E7AC.</summary>
    private const ushort FacingLeftFireballing1 = 0xe7ac;
    /// <summary><c>InstList_Ridley_FacingRight_Fireballing_0</c> at $A6:E7B4.</summary>
    private const ushort FacingRightFireballing0 = 0xe7b4;
    /// <summary><c>InstList_Ridley_FacingRight_Fireballing_1</c> at $A6:E820.</summary>
    private const ushort FacingRightFireballing1 = 0xe820;
    /// <summary><c>InstList_RidleyCeres_FacingRight_TransitionToFlying</c> at $A6:E945.</summary>
    private const ushort CeresFacingRightTransitionToFlying = 0xe945;

    /// <summary>Native program bank $A6.</summary>
    internal const byte Bank = 0xa6;

    internal static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xe538),
        Entry(Initial),
        Op((ushort)RidleyInstruction.GotoYIfNotFacingLeft, FacingRightInitial),
        Frame(12),
        Op((ushort)CommonEnemyInstruction.Sleep),
        Frame(12),
        Op((ushort)CommonEnemyInstruction.Sleep),
        Entry(CeresLunge),
        Op((ushort)RidleyInstruction.GotoYIfNotFacingLeft, UNUSEDInstListRidleyCeresFacingRightLungingA6E576),
        Op((ushort)RidleyInstruction.CeresFeetDistanceIndexInY, 0x0000),
        Frame(4),
        Op((ushort)RidleyInstruction.CeresFeetDistanceIndexInY, 0x0002),
        Frame(6),
        Op((ushort)RidleyInstruction.CeresFeetDistanceIndexInY, 0x0004),
        Frame(80),
        Op((ushort)RidleyInstruction.CeresFeetDistanceIndexInY, 0x0002),
        Frame(6),
        Op((ushort)RidleyInstruction.CeresFeetDistanceIndexInY, 0x0000),
        Frame(4),
        Op((ushort)CommonEnemyInstruction.Sleep),
        Op((ushort)RidleyInstruction.CeresFeetDistanceIndexInY, 0x0000),
        Frame(4),
        Op((ushort)RidleyInstruction.CeresFeetDistanceIndexInY, 0x0002),
        Frame(6),
        Op((ushort)RidleyInstruction.CeresFeetDistanceIndexInY, 0x0004),
        Frame(80),
        Op((ushort)RidleyInstruction.CeresFeetDistanceIndexInY, 0x0002),
        Frame(6),
        Op((ushort)RidleyInstruction.CeresFeetDistanceIndexInY, 0x0000),
        Frame(4),
        Op((ushort)CommonEnemyInstruction.Sleep),
        Origin(0xe658),
        Entry(RetrieveBabyMetroid),
        Op((ushort)RidleyInstruction.GotoYIfNotFacingLeft, UNUSEDInstListRidleyCeresFacingRightRetrieveBabyMetroidA6E676),
        Op((ushort)RidleyInstruction.CeresFeetDistanceIndexInY, 0x0000),
        Frame(4),
        Op((ushort)RidleyInstruction.CeresFeetDistanceIndexInY, 0x0002),
        Frame(6),
        Op((ushort)RidleyInstruction.CeresFeetDistanceIndexInY, 0x0004),
        Frame(1),
        Op((ushort)CommonEnemyInstruction.Sleep),
        Op((ushort)RidleyInstruction.CeresFeetDistanceIndexInY, 0x0000),
        Frame(4),
        Op((ushort)RidleyInstruction.CeresFeetDistanceIndexInY, 0x0002),
        Frame(6),
        Op((ushort)RidleyInstruction.CeresFeetDistanceIndexInY, 0x0004),
        Frame(1),
        Op((ushort)CommonEnemyInstruction.Sleep),
        Entry(OpeningRoar),
        Op((ushort)RidleyInstruction.GotoYIfNotFacingLeft, FacingRightOpeningRoar),
        Frame(6),
        Op((ushort)RidleyInstruction.Roar),
        Frame(8),
        Frame(96),
        Frame(8),
        Op((ushort)RidleyInstruction.ClearRoaringFlag),
        Frame(1),
        Op((ushort)CommonEnemyInstruction.Sleep),
        Frame(6),
        Op((ushort)RidleyInstruction.Roar),
        Frame(8),
        Frame(96),
        Frame(8),
        Op((ushort)RidleyInstruction.ClearRoaringFlag),
        Frame(1),
        Op((ushort)CommonEnemyInstruction.Sleep),
        Entry(DeathRoar),
        Op((ushort)RidleyInstruction.GotoYIfNotFacingLeft, FacingRightDeathRoar),
        Frame(6),
        Op((ushort)RidleyInstruction.Roar),
        Frame(8),
        Frame(16),
        Op((ushort)RidleyInstruction.ClearRoaringFlag),
        Op((ushort)CommonEnemyInstruction.Sleep),
        Frame(6),
        Op((ushort)RidleyInstruction.Roar),
        Frame(8),
        Frame(16),
        Op((ushort)RidleyInstruction.ClearRoaringFlag),
        Op((ushort)CommonEnemyInstruction.Sleep),
        Entry(TurnFromLeftToRight),
        Op((ushort)RidleyInstruction.FaceForward),
        Frame(1),
        Frame(8),
        Op((ushort)RidleyInstruction.FlipRight),
        Frame(1),
        Frame(1),
        Op((ushort)CommonEnemyInstruction.Sleep),
        Entry(TurnFromRightToLeft),
        Op((ushort)RidleyInstruction.FaceForward),
        Frame(1),
        Frame(8),
        Op((ushort)RidleyInstruction.FlipLeft),
        Frame(1),
        Frame(1),
        Op((ushort)CommonEnemyInstruction.Sleep),
        Origin(0xe73a),
        Entry(Fireballing),
        Op((ushort)RidleyInstruction.GotoYIfNotFacingLeft, FacingRightFireballing0),
        Op((ushort)RidleyInstruction.GotoYIfNotNorfairAndSamusHasLowEnergy, FacingLeftFireballing1),
        Frame(8),
        Op((ushort)RidleyInstruction.Roar),
        Frame(8),
        Frame(2),
        Op((ushort)RidleyInstruction.CalculateFireballXYVelocities),
        Op((ushort)RidleyInstruction.SpawnFireballWithAfterburn),
        Frame(5),
        Op((ushort)RidleyInstruction.CalculateFireballXYVelocities),
        Op((ushort)RidleyInstruction.SpawnFireballWithoutAfterburn),
        Frame(5),
        Op((ushort)RidleyInstruction.CalculateFireballXYVelocities),
        Op((ushort)RidleyInstruction.SpawnFireballWithoutAfterburn),
        Frame(5),
        Op((ushort)RidleyInstruction.CalculateFireballXYVelocities),
        Op((ushort)RidleyInstruction.SpawnFireballWithoutAfterburn),
        Frame(48),
        Frame(8),
        Op((ushort)RidleyInstruction.GotoYIfNotNorfairAndSamusHasLowEnergy, FacingLeftFireballing1),
        Frame(32),
        Op((ushort)RidleyInstruction.CalculateFireballXYVelocities),
        Op((ushort)RidleyInstruction.Roar),
        Frame(8),
        Frame(2),
        Op((ushort)RidleyInstruction.CalculateFireballXYVelocities),
        Op((ushort)RidleyInstruction.SpawnFireballWithAfterburn),
        Frame(5),
        Op((ushort)RidleyInstruction.CalculateFireballXYVelocities),
        Op((ushort)RidleyInstruction.SpawnFireballWithoutAfterburn),
        Frame(5),
        Op((ushort)RidleyInstruction.CalculateFireballXYVelocities),
        Op((ushort)RidleyInstruction.SpawnFireballWithoutAfterburn),
        Frame(5),
        Op((ushort)RidleyInstruction.CalculateFireballXYVelocities),
        Op((ushort)RidleyInstruction.SpawnFireballWithoutAfterburn),
        Frame(48),
        Frame(8),
        Op((ushort)RidleyInstruction.ClearRoaringFlag),
        Frame(1),
        Op((ushort)CommonEnemyInstruction.Sleep),
        Op((ushort)RidleyInstruction.GotoYIfNotNorfairAndSamusHasLowEnergy, FacingRightFireballing1),
        Frame(8),
        Op((ushort)RidleyInstruction.Roar),
        Frame(8),
        Frame(2),
        Op((ushort)RidleyInstruction.CalculateFireballXYVelocities),
        Op((ushort)RidleyInstruction.SpawnFireballWithAfterburn),
        Frame(5),
        Op((ushort)RidleyInstruction.CalculateFireballXYVelocities),
        Op((ushort)RidleyInstruction.SpawnFireballWithoutAfterburn),
        Frame(5),
        Op((ushort)RidleyInstruction.CalculateFireballXYVelocities),
        Op((ushort)RidleyInstruction.SpawnFireballWithoutAfterburn),
        Frame(5),
        Op((ushort)RidleyInstruction.CalculateFireballXYVelocities),
        Op((ushort)RidleyInstruction.SpawnFireballWithoutAfterburn),
        Frame(48),
        Frame(8),
        Op((ushort)RidleyInstruction.GotoYIfNotNorfairAndSamusHasLowEnergy, FacingRightFireballing1),
        Frame(32),
        Op((ushort)RidleyInstruction.Roar),
        Frame(8),
        Frame(2),
        Op((ushort)RidleyInstruction.CalculateFireballXYVelocities),
        Op((ushort)RidleyInstruction.SpawnFireballWithAfterburn),
        Frame(5),
        Op((ushort)RidleyInstruction.CalculateFireballXYVelocities),
        Op((ushort)RidleyInstruction.SpawnFireballWithoutAfterburn),
        Frame(5),
        Op((ushort)RidleyInstruction.CalculateFireballXYVelocities),
        Op((ushort)RidleyInstruction.SpawnFireballWithoutAfterburn),
        Frame(5),
        Op((ushort)RidleyInstruction.CalculateFireballXYVelocities),
        Op((ushort)RidleyInstruction.SpawnFireballWithoutAfterburn),
        Frame(48),
        Frame(8),
        Op((ushort)RidleyInstruction.ClearRoaringFlag),
        Frame(1),
        Op((ushort)CommonEnemyInstruction.Sleep),
        Origin(0xe91d),
        Entry(TransitionToFlying),
        Op((ushort)RidleyInstruction.GotoYIfNotFacingLeft, CeresFacingRightTransitionToFlying),
        Frame(3),
        Op((ushort)RidleyInstruction.MoveWithArgsInY, 0x0001, 0xfff4),
        Frame(4),
        Op((ushort)RidleyInstruction.MoveWithArgsInY, 0xfffc, 0xfff8),
        Frame(5),
        Op((ushort)RidleyInstruction.CeresStartLiftoff),
        Frame(17),
        Frame(17),
        Op((ushort)CommonEnemyInstruction.Sleep),
        Frame(3),
        Op((ushort)RidleyInstruction.MoveWithArgsInY, 0xffff, 0xfff4),
        Frame(4),
        Op((ushort)RidleyInstruction.MoveWithArgsInY, 0x0004, 0xfff8),
        Frame(5),
        Op((ushort)RidleyInstruction.StartLiftoff),
        Frame(17),
        Frame(17),
        Op((ushort)CommonEnemyInstruction.Sleep));

    /// <summary>Reads one compiled mechanics word and rejects presentation or foreign data.</summary>
    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"Ridley instruction mechanics pointer $A6:{address:X4} is not compiled.");
}
