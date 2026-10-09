namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Bomb and Golden Torizo's Chozo-orb programs at $86:AB15-$AB89.
/// Their eighteen interleaved spritemap operands use extracted presentation art.
/// </summary>
internal abstract class TorizoChozoOrbInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_TorizoChozoOrbs_Left</c> at $86:AB15.</summary>
    internal const ushort MovingLeft = 0xab15;
    /// <summary><c>InstList_EnemyProjectile_TorizoChozoOrbs_BreakOnWall</c> at $86:AB25.</summary>
    internal const ushort WallImpact = 0xab25;
    /// <summary><c>InstList_EnemyProjectile_TorizoChozoOrbs_BreakOnFloor</c> at $86:AB41.</summary>
    internal const ushort FloorImpact = 0xab41;
    /// <summary><c>InstList_EnemyProjectile_Shot_TorizoChozoOrbs</c> at $86:AB68.</summary>
    internal const ushort Shot = 0xab68;
    /// <summary><c>EnemyHeaders_BombTorizoOrb</c> at $A0:EF3F.</summary>
    internal const ushort BombOrbEnemyHeader = 0xef3f;
    /// <summary><c>EnemyHeaders_GoldenTorizoOrb</c> at $A0:EFBF.</summary>
    internal const ushort GoldenOrbEnemyHeader = 0xefbf;

    /// <summary>Number of compiled address/value pairs covering mechanics operands in the orb instruction lists.</summary>
    public static int MechanicsWordCount => 40;
    /// <summary>Number of extracted spritemap operand words retained as presentation data.</summary>
    public static int PresentationWordCount => 18;

    /// <summary>Returns the compiled address/value pair for one mechanics operand in the orb instruction programs.</summary>
    /// <param name="index">Zero-based mechanics operand index, from zero through <see cref="MechanicsWordCount"/> minus one.</param>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index < 6)
        {
            ushort start = (ushort)(MovingLeft + 8 * (index / 3));
            return (index % 3) switch
            {
                0 => new(start, 85),
                1 => new((ushort)(start + 4), EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
                _ => new((ushort)(start + 6), start),
            };
        }
        if (index < 15) return ImpactWord(WallImpact, index - 6, shot: false);
        if (index < 28)
        {
            int local = index - 15;
            if (local is >= 6 and < 12)
                return new((ushort)(FloorImpact + 13 + 4 * (local - 6)), (ushort)(local - 2));
            (int offset, ushort value) = local switch
            {
                0 => (0, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
                1 => (2, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_AndY),
                2 => (4, (ushort)0xdfff),
                3 => (6, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_OrY),
                4 => (8, (ushort)0x5000),
                5 => (10, EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib3_Max6),
                _ => (37, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
            };
            return new((ushort)(FloorImpact + offset), value);
        }
        return ImpactWord(Shot, index - 28, shot: true);
    }

    /// <summary>Wall and shot impacts share property setup and five four-tick poses; only shot impact emits drops.</summary>
    private static InstructionMechanicsWord ImpactWord(ushort start, int index, bool shot)
    {
        if (index is >= 3 and < 8)
            return new((ushort)(start + 6 + 4 * (index - 3)), 4);
        (int offset, ushort value) = index switch
        {
            0 => (0, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
            1 => (2, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_OrY),
            2 => (4, (ushort)0x5000),
            8 => (26, shot ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_SpawnEnemyDropsWIthYDropChances
                : EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
            9 => (28, BombOrbEnemyHeader),
            10 => (30, GoldenOrbEnemyHeader),
            _ => (32, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
        };
        return new((ushort)(start + offset), value);
    }

    /// <summary>Maps a presentation operand ordinal to its native address in the corresponding instruction list.</summary>
    /// <param name="index">Zero-based presentation operand index, from zero through <see cref="PresentationWordCount"/> minus one.</param>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index < 2) return (ushort)(MovingLeft + 2 + 8 * index);
        if (index < 7) return (ushort)(WallImpact + 8 + 4 * (index - 2));
        if (index < 13) return (ushort)(FloorImpact + 15 + 4 * (index - 7));
        return (ushort)(Shot + 8 + 4 * (index - 13));
    }
    /// <summary>Reports whether the projectile kind is one of the Bomb or Golden Torizo Chozo orbs.</summary>
    /// <param name="kind">The runtime projectile kind to classify.</param>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.BombTorizoChozoOrb or
        RoomEnemyProjectileKind.GoldenTorizoChozoOrb;

    /// <summary>Looks up a compiled mechanics operand by native address and fails if the address is not part of these lists.</summary>
    /// <param name="address">The bank-$86 instruction-list address whose compiled value is required.</param>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            InstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address) return candidate.Value;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }

        throw new InvalidDataException(
            $"Torizo Chozo-orb mechanics pointer $86:{address:X4} is not compiled.");
    }
}
