namespace SuperMetroid.Core.Game;

internal readonly record struct GoldenTorizoEggInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Golden Torizo's egg programs at $86:B104-$B1C0. Their twenty-six
/// interleaved spritemap operands are extracted presentation data; two packed sound IDs
/// remain live cartridge audio data. The shot path reuses the compiled Torizo-orb
/// wall-break list.
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

    internal static int MechanicsWordCount => 53;
    internal static int PresentationWordCount => 26;

    internal static GoldenTorizoEggInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index < 16)
        {
            ushort start = (ushort)(BouncingLeft + 24 * (index / 8));
            int local = index % 8;
            if (local is >= 3 and < 6) return new((ushort)(start + 8 + 4 * (local - 3)), 4);
            return local switch
            {
                0 => new(start, 48),
                1 => new((ushort)(start + 4), EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
                2 => new((ushort)(start + 6), EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
                6 => new((ushort)(start + 20), EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
                _ => new((ushort)(start + 22), Hatch),
            };
        }
        if (index < 21)
        {
            int local = index - 16;
            ushort value = local switch
            {
                0 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_AndY,
                1 => 0xdfff,
                2 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_OrY,
                3 => 0x8000,
                _ => EnemyProjectileCodePointers.Instruction_EnemyProjectile_GoldenTorizoEgg_GoToHatched,
            };
            return new((ushort)(Hatch + 2 * local), value);
        }
        if (index < 39)
        {
            int offset = index - 21;
            ushort start = (ushort)(HatchedLeft + 27 * (offset / 9));
            int local = offset % 9;
            if (local is >= 3 and < 7) return new((ushort)(start + 7 + 4 * (local - 3)), 6);
            return local switch
            {
                0 => new(start, EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6),
                1 => new((ushort)(start + 3), EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY),
                2 => new((ushort)(start + 5), EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_GoldenTorizoEgg_Hatched),
                7 => new((ushort)(start + 23), EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
                _ => new((ushort)(start + 25), (ushort)(start + 7)),
            };
        }
        int breakOffset = index - 39;
        int facing = breakOffset / 7;
        ushort breakStart = (ushort)(BreakLeft + 24 * facing);
        int breakWord = breakOffset % 7;
        if (breakWord == 0) return new(breakStart, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction);
        if (breakWord == 6) return new((ushort)(breakStart + 22), EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete);
        // Four four-tick shattering poses precede the facing-specific final hold.
        return new((ushort)(breakStart + 2 + 4 * (breakWord - 1)),
            breakWord == 5 ? (ushort)(facing == 0 ? 10 : 8) : (ushort)4);
    }

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index < 8)
        {
            int local = index % 4;
            return (ushort)(BouncingLeft + 24 * (index / 4) + (local == 0 ? 2 : 10 + 4 * (local - 1)));
        }
        if (index < 16)
        {
            int offset = index - 8;
            return (ushort)(HatchedLeft + 27 * (offset / 4) + 9 + 4 * (offset % 4));
        }
        int breakOffset = index - 16;
        return (ushort)(BreakLeft + 24 * (breakOffset / 5) + 4 + 4 * (breakOffset % 5));
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (address is >= TorizoChozoOrbInstructionProgramDefinitions.WallImpact and <= 0xab3f)
            return TorizoChozoOrbInstructionProgramDefinitions.ReadMechanicsWord(address);

        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            GoldenTorizoEggInstructionMechanicsWord candidate = MechanicsWord(middle);
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
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            var word = MechanicsWord(index);
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
