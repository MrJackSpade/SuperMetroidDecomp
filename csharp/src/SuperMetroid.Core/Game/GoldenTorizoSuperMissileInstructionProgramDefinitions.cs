namespace SuperMetroid.Core.Game;

internal readonly record struct GoldenTorizoSuperMissileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Golden Torizo's reflected Super Missile programs at
/// $86:B293-$B31A. Their twenty-four interleaved spritemap operands and packed impact
/// sound ID remain live cartridge presentation/audio data.
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

    private static readonly GoldenTorizoSuperMissileInstructionMechanicsWord[] Words =
    [
        new(RightInitial, 0x0030),
        new(0xb297, EnemyProjectileCodePointers.Instruction_AimSuperMissile_Rightwards),
        new(0xb299, EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY),
        new(0xb29b, EnemyProjectileCodePointers.PreInst_EnemyProjectile_GoldenTorizoSuperMissile_Thrown),
        new(RightLoop, 2),
        new(0xb2a1, 2),
        new(0xb2a5, 2),
        new(0xb2a9, 2),
        new(0xb2ad, 2),
        new(0xb2b1, 2),
        new(0xb2b5, 2),
        new(0xb2b9, 2),
        new(0xb2bd, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xb2bf, RightLoop),

        new(LeftInitial, 0x0030),
        new(0xb2c5, EnemyProjectileCodePointers.Instruction_AimSuperMissile_Leftwards),
        new(0xb2c7, EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY),
        new(0xb2c9, EnemyProjectileCodePointers.PreInst_EnemyProjectile_GoldenTorizoSuperMissile_Thrown),
        new(LeftLoop, 2),
        new(0xb2cf, 2),
        new(0xb2d3, 2),
        new(0xb2d7, 2),
        new(0xb2db, 2),
        new(0xb2df, 2),
        new(0xb2e3, 2),
        new(0xb2e7, 2),
        new(0xb2eb, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xb2ed, LeftLoop),

        new(Impact, EnemyProjectileCodePointers.Instruction_EnemyProjectile_XYRadiusInY),
        new(0xb2f1, 0x1010),
        new(0xb2f3, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        new(0xb2f5, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_OrY),
        new(0xb2f7, 0x5000),
        new(0xb2f9, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_AndY),
        new(0xb2fb, 0x5fff),
        new(0xb2fd, EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6),
        new(0xb300, 5),
        new(0xb304, 5),
        new(0xb308, 5),
        new(0xb30c, 5),
        new(0xb310, 5),
        new(0xb314, 5),
        new(0xb318, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xb295,
        0xb29f, 0xb2a3, 0xb2a7, 0xb2ab, 0xb2af, 0xb2b3, 0xb2b7, 0xb2bb,
        0xb2c3,
        0xb2cd, 0xb2d1, 0xb2d5, 0xb2d9, 0xb2dd, 0xb2e1, 0xb2e5, 0xb2e9,
        0xb302, 0xb306, 0xb30a, 0xb30e, 0xb312, 0xb316,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static GoldenTorizoSuperMissileInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            GoldenTorizoSuperMissileInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address) return candidate.Value;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }

        throw new InvalidDataException(
            $"Golden Torizo Super Missile mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        foreach (GoldenTorizoSuperMissileInstructionMechanicsWord word in Words)
        {
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
