using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Golden Torizo's reflected Super Missile programs at
/// $86:B293-$B31A. Their twenty-four interleaved spritemap operands are extracted
/// presentation data; the packed impact sound ID remains live cartridge audio data.
/// </summary>
internal abstract class GoldenTorizoSuperMissileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProj_GoldenTorizoSuperMissile_Rightwards_0</c> at $86:B293.</summary>
    internal const ushort RightInitial = 0xb293;
    /// <summary><c>InstList_EnemyProj_GoldenTorizoSuperMissile_Rightwards_1</c> at $86:B29D.</summary>
    internal const ushort RightLoop = 0xb29d;
    /// <summary><c>InstList_EnemyProj_GoldenTorizoSuperMissile_Leftwards_0</c> at $86:B2C1.</summary>
    internal const ushort LeftInitial = 0xb2c1;
    /// <summary><c>InstList_EnemyProj_GoldenTorizoSuperMissile_Leftwards_1</c> at $86:B2CB.</summary>
    internal const ushort LeftLoop = 0xb2cb;
    /// <summary><c>InstList_EnemyProjectile_Shot_GoldenTorizoSuperMissile</c> at $86:B2EF.</summary>
    internal const ushort Impact = 0xb2ef;

    /// <summary>Native program bank $86.</summary>
    internal const byte Bank = 0x86;

    internal static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xb293),
        Entry(RightInitial),
        Frame(48),
        Op((ushort)EnemyProjectileInstruction.AimSuperMissile_Rightwards),
        Op((ushort)EnemyProjectileInstruction.PreInstructionInY, (ushort)EnemyProjectilePreInstruction.GoldenTorizoSuperMissile_Thrown),
        Entry(RightLoop),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Op((ushort)EnemyProjectileInstruction.GotoY, RightLoop),
        Entry(LeftInitial),
        Frame(48),
        Op((ushort)EnemyProjectileInstruction.AimSuperMissile_Leftwards),
        Op((ushort)EnemyProjectileInstruction.PreInstructionInY, (ushort)EnemyProjectilePreInstruction.GoldenTorizoSuperMissile_Thrown),
        Entry(LeftLoop),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Op((ushort)EnemyProjectileInstruction.GotoY, LeftLoop),
        Entry(Impact),
        Op((ushort)EnemyProjectileInstruction.XYRadiusInY, InstructionWord.Bytes(0x10, 0x10)),
        Op((ushort)EnemyProjectileInstruction.ClearPreInstruction),
        Op((ushort)EnemyProjectileInstruction.Properties_OrY, 0x5000),
        Op((ushort)EnemyProjectileInstruction.Properties_AndY, 0x5fff),
        Op((ushort)EnemyProjectileInstruction.QueueSoundInY_Lib2_Max6),
        Skip(1),
        Frame(5),
        Frame(5),
        Frame(5),
        Frame(5),
        Frame(5),
        Frame(5),
        Op((ushort)EnemyProjectileInstruction.Delete));
    public static int PresentationWordCount => Layout.PresentationSlotCount;
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);

    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"Golden Torizo Super Missile mechanics pointer $86:{address:X4} is not compiled.");
}
