namespace SuperMetroid.Core.Game;

internal readonly record struct GoldenTorizoEggInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Golden Torizo's egg programs at $86:B104-$B1C0. Their twenty-six
/// interleaved spritemap operands and two packed sound IDs remain live cartridge
/// presentation/audio data. The shot path reuses the compiled Torizo-orb wall-break list.
/// </summary>
internal static class GoldenTorizoEggInstructionProgramDefinitions
{
    /// <summary><c>InitAI_EnemyProjectile_GoldenTorizoEgg</c> at $86:B001.</summary>
    internal const ushort InitializationAi = 0xb001;
    /// <summary><c>InstList_EnemyProjectile_GoldenTorizoEgg_BouncingLeft</c> at $86:B104.</summary>
    internal const ushort BouncingLeft = 0xb104;
    /// <summary><c>InstList_EnemyProjectile_GoldenTorizoEgg_BouncingRight</c> at $86:B11C.</summary>
    internal const ushort BouncingRight = 0xb11c;
    /// <summary><c>InstList_EnemyProjectile_GoldenTorizoEgg_Hatch</c> at $86:B134.</summary>
    internal const ushort Hatch = 0xb134;
    /// <summary><c>InstList_EnemyProjectile_GoldenTorizoEgg_Hatched_Left_0</c> at $86:B14B.</summary>
    internal const ushort HatchedLeft = 0xb14b;
    /// <summary><c>InstList_EnemyProjectile_GoldenTorizoEgg_Hatched_Left_1</c> at $86:B152.</summary>
    internal const ushort HatchedLeftLoop = 0xb152;
    /// <summary><c>InstList_EnemyProjectile_GoldenTorizoEgg_Hatched_Right_0</c> at $86:B166.</summary>
    internal const ushort HatchedRight = 0xb166;
    /// <summary><c>InstList_EnemyProjectile_GoldenTorizoEgg_Hatched_Right_1</c> at $86:B16D.</summary>
    internal const ushort HatchedRightLoop = 0xb16d;
    /// <summary><c>InstList_EnemyProjectile_GoldenTorizoEgg_Break_FacingLeft</c> at $86:B190.</summary>
    internal const ushort BreakLeft = 0xb190;
    /// <summary><c>InstList_EnemyProjectile_GoldenTorizoEgg_Break_FacingRight</c> at $86:B1A8.</summary>
    internal const ushort BreakRight = 0xb1a8;

    private static readonly GoldenTorizoEggInstructionMechanicsWord[] Words =
    [
        new(BouncingLeft, 0x0030),
        new(0xb108, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(0xb10a, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        new(0xb10c, 4),
        new(0xb110, 4),
        new(0xb114, 4),
        new(0xb118, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xb11a, Hatch),

        new(BouncingRight, 0x0030),
        new(0xb120, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(0xb122, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        new(0xb124, 4),
        new(0xb128, 4),
        new(0xb12c, 4),
        new(0xb130, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xb132, Hatch),

        new(Hatch, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_AndY),
        new(0xb136, 0xdfff),
        new(0xb138, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_OrY),
        new(0xb13a, 0x8000),
        new(0xb13c, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GoldenTorizoEgg_GoToHatched),

        new(HatchedLeft, EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6),
        new(0xb14e, EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY),
        new(0xb150, EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_GoldenTorizoEgg_Hatched),
        new(HatchedLeftLoop, 6),
        new(0xb156, 6),
        new(0xb15a, 6),
        new(0xb15e, 6),
        new(0xb162, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xb164, HatchedLeftLoop),

        new(HatchedRight, EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6),
        new(0xb169, EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY),
        new(0xb16b, EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_GoldenTorizoEgg_Hatched),
        new(HatchedRightLoop, 6),
        new(0xb171, 6),
        new(0xb175, 6),
        new(0xb179, 6),
        new(0xb17d, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xb17f, HatchedRightLoop),

        new(BreakLeft, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        new(0xb192, 4),
        new(0xb196, 4),
        new(0xb19a, 4),
        new(0xb19e, 4),
        new(0xb1a2, 10),
        new(0xb1a6, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),

        new(BreakRight, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        new(0xb1aa, 4),
        new(0xb1ae, 4),
        new(0xb1b2, 4),
        new(0xb1b6, 4),
        new(0xb1ba, 8),
        new(0xb1be, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xb106, 0xb10e, 0xb112, 0xb116,
        0xb11e, 0xb126, 0xb12a, 0xb12e,
        0xb154, 0xb158, 0xb15c, 0xb160,
        0xb16f, 0xb173, 0xb177, 0xb17b,
        0xb194, 0xb198, 0xb19c, 0xb1a0, 0xb1a4,
        0xb1ac, 0xb1b0, 0xb1b4, 0xb1b8, 0xb1bc,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static GoldenTorizoEggInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (address is >= TorizoChozoOrbInstructionProgramDefinitions.WallImpact and <= 0xab3f)
            return TorizoChozoOrbInstructionProgramDefinitions.ReadMechanicsWord(address);

        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            GoldenTorizoEggInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address) return candidate.Value;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }

        throw new InvalidDataException(
            $"Golden Torizo egg mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if (TorizoChozoOrbInstructionProgramDefinitions.IsCompiledMechanicsByte(address) &&
            unchecked((ushort)address) is >= TorizoChozoOrbInstructionProgramDefinitions.WallImpact and <= 0xab40)
        {
            return true;
        }
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        foreach (GoldenTorizoEggInstructionMechanicsWord word in Words)
        {
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
