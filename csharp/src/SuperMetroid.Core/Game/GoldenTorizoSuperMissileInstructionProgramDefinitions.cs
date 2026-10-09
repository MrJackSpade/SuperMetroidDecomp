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

    /// <summary>Compiled instruction layout separating native mechanics words from extracted spritemap operands.</summary>
    internal static readonly InstructionProgramLayout Layout = new(Bank,
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
    /// <summary>Number of interleaved spritemap operands supplied by extracted presentation data.</summary>
    public static int PresentationWordCount => Layout.PresentationSlotCount;

    /// <summary>Returns the native instruction address for a presentation operand's zero-based slot.</summary>
    /// <param name="index">Ordinal among the presentation slots in the compiled layout.</param>
    /// <returns>Instruction address where that slot is consumed.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the layout's presentation slots.</exception>
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);

    /// <summary>Resolves a compiled mechanics word by instruction address, excluding extracted presentation operands.</summary>
    /// <param name="address">Address of a mechanics instruction word.</param>
    /// <returns>The native operation or operand word at that address.</returns>
    /// <exception cref="InvalidDataException">The address is not part of the compiled mechanics layout.</exception>
    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"Golden Torizo Super Missile mechanics pointer $86:{address:X4} is not compiled.");
}
