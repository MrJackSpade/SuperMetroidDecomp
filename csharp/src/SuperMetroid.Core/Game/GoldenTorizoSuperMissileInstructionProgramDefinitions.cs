using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

internal readonly record struct GoldenTorizoSuperMissileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Golden Torizo's reflected Super Missile programs at
/// $86:B293-$B31A. Their twenty-four interleaved spritemap operands are extracted
/// presentation data; the packed impact sound ID remains live cartridge audio data.
/// </summary>
internal static class GoldenTorizoSuperMissileInstructionProgramDefinitions
{
    /// <summary><c>InitAI_EnemyProjectile_GoldenTorizoSuperMissile</c> at $86:B1CE.</summary>
    internal const ushort InitializationAi = 0xb1ce;
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

    private static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xb293),
        Entry(RightInitial),
        Frame(48),
        Op(EnemyProjectileCodePointers.Instruction_AimSuperMissile_Rightwards),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY, EnemyProjectileCodePointers.PreInst_EnemyProjectile_GoldenTorizoSuperMissile_Thrown),
        Entry(RightLoop),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY, RightLoop),
        Entry(LeftInitial),
        Frame(48),
        Op(EnemyProjectileCodePointers.Instruction_AimSuperMissile_Leftwards),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY, EnemyProjectileCodePointers.PreInst_EnemyProjectile_GoldenTorizoSuperMissile_Thrown),
        Entry(LeftLoop),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY, LeftLoop),
        Entry(Impact),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_XYRadiusInY, InstructionWord.Bytes(0x10, 0x10)),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_OrY, 0x5000),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_AndY, 0x5fff),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6),
        Skip(1),
        Frame(5),
        Frame(5),
        Frame(5),
        Frame(5),
        Frame(5),
        Frame(5),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete));

    internal static int MechanicsWordCount => Layout.MechanicsWordCount;
    internal static int PresentationWordCount => Layout.PresentationSlotCount;
    internal static GoldenTorizoSuperMissileInstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = Layout.MechanicsWord(index);
        return new(address, value);
    }
    internal static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);

    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"Golden Torizo Super Missile mechanics pointer $86:{address:X4} is not compiled.");

    internal static bool IsCompiledMechanicsByte(int address) => Layout.IsCompiledMechanicsByte(address);
}
