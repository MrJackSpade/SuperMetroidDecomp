namespace SuperMetroid.Core.Game;

internal readonly record struct GoldenTorizoEyeBeamInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Golden Torizo's eye-beam programs at $86:B3CD-$B428. Their
/// seventeen interleaved spritemap operands and packed floor-impact sound ID remain live
/// cartridge presentation/audio data.
/// </summary>
internal static class GoldenTorizoEyeBeamInstructionProgramDefinitions
{
    /// <summary><c>InitAI_EnemyProjectile_GoldenTorizoEyeBeam</c> at $86:B328.</summary>
    internal const ushort InitializationAi = 0xb328;
    /// <summary><c>InstList_EnemyProjectile_GoldenTorizoEyeBeam_HitWall</c> at $86:B3CD.</summary>
    internal const ushort WallImpact = 0xb3cd;
    /// <summary><c>InstList_EnemyProjectile_GoldenTorizoEyeBeam_HitFloor_0</c> at $86:B3E5.</summary>
    internal const ushort FloorImpact = 0xb3e5;
    /// <summary><c>InstList_EnemyProjectile_GoldenTorizoEyeBeam_HitFloor_1</c> at $86:B3E7.</summary>
    internal const ushort FloorImpactLoop = 0xb3e7;
    /// <summary><c>InstList_EnemyProjectile_GoldenTorizoEyeBeam_Normal</c> at $86:B410.</summary>
    internal const ushort Normal = 0xb410;

    private static readonly GoldenTorizoEyeBeamInstructionMechanicsWord[] Words =
    [
        new(WallImpact, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        new(0xb3cf, 4),
        new(0xb3d3, 4),
        new(0xb3d7, 4),
        new(0xb3db, 4),
        new(0xb3df, 4),
        new(0xb3e3, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),

        new(FloorImpact, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        new(FloorImpactLoop, 8),
        new(0xb3eb, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoYIfEyeBeamExplosionsDisabled),
        new(0xb3ed, FloorImpactLoop),
        new(0xb3ef, EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib3_Max6),
        new(0xb3f2, 4),
        new(0xb3f6, 5),
        new(0xb3fa, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_AndY),
        new(0xb3fc, 0xdfff),
        new(0xb3fe, 6),
        new(0xb402, 7),
        new(0xb406, 8),
        new(0xb40a, 9),
        new(0xb40e, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),

        new(Normal, 1),
        new(0xb414, 1),
        new(0xb418, 1),
        new(0xb41c, 1),
        new(0xb420, 1),
        new(0xb424, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xb426, Normal),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xb3d1, 0xb3d5, 0xb3d9, 0xb3dd, 0xb3e1,
        0xb3e9,
        0xb3f4, 0xb3f8, 0xb400, 0xb404, 0xb408, 0xb40c,
        0xb412, 0xb416, 0xb41a, 0xb41e, 0xb422,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static GoldenTorizoEyeBeamInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            GoldenTorizoEyeBeamInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address) return candidate.Value;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }

        throw new InvalidDataException(
            $"Golden Torizo eye-beam mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        foreach (GoldenTorizoEyeBeamInstructionMechanicsWord word in Words)
        {
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
