using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Bomb/Golden Torizo's right- and left-foot landing-dust programs
/// at $86:AF9D-$AFCB. Their eight spritemap operands use extracted presentation art.
/// </summary>
internal abstract class TorizoLandingDustInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary><c>InstList_EnemyProjectile_TorizoLandingDustCloud_RightFoot</c> at $86:AF9D.</summary>
    internal const ushort RightFoot = 0xaf9d;
    /// <summary><c>InstList_EnemyProjectile_TorizoLandingDustCloud_LeftFoot</c> at $86:AFB5.</summary>
    internal const ushort LeftFoot = 0xafb5;

    /// <summary>Native program bank $86.</summary>
    internal const byte Bank = 0x86;
    static int IDeclaredProgramBank.Bank => Bank;

    private static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xaf9d),
        Entry(RightFoot),
        Frame(4),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_TorizoLandingDustClouds),
        Frame(4),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_TorizoLandingDustClouds),
        Frame(4),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_TorizoLandingDustClouds),
        Frame(4),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
        Entry(LeftFoot),
        Frame(4),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_TorizoLandingDustClouds),
        Frame(4),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_TorizoLandingDustClouds),
        Frame(4),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_TorizoLandingDustClouds),
        Frame(4),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete));

    public static int MechanicsWordCount => Layout.MechanicsWordCount;
    public static int PresentationWordCount => Layout.PresentationSlotCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.BombTorizoRightFootDust or
        RoomEnemyProjectileKind.BombTorizoLeftFootDust;

    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"Torizo landing-dust mechanics pointer $86:{address:X4} is not compiled.");

    public static bool IsCompiledMechanicsByte(int address) => Layout.IsCompiledMechanicsByte(address);
}
