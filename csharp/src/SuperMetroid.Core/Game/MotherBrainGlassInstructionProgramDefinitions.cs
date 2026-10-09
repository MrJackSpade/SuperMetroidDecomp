namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Mother Brain's eight glass-shard loops and finite sparkle program.
/// Their interleaved spritemap operands resolve through extracted presentation art.
/// </summary>
internal abstract class MotherBrainGlassInstructionProgramDefinitions
{
    /// <summary>First glass-shard angle-group loop at $86:CC93.</summary>
    internal const ushort ShardGroup0 = 0xcc93;

    /// <summary>Second glass-shard angle-group loop at $86:CCB7.</summary>
    internal const ushort ShardGroup1 = 0xccb7;

    /// <summary>Third glass-shard angle-group loop at $86:CCDB.</summary>
    internal const ushort ShardGroup2 = 0xccdb;

    /// <summary>Fourth glass-shard angle-group loop at $86:CCFF.</summary>
    internal const ushort ShardGroup3 = 0xccff;

    /// <summary>Fifth glass-shard angle-group loop at $86:CD23.</summary>
    internal const ushort ShardGroup4 = 0xcd23;

    /// <summary>Sixth glass-shard angle-group loop at $86:CD47.</summary>
    internal const ushort ShardGroup5 = 0xcd47;

    /// <summary>Seventh glass-shard angle-group loop at $86:CD6B.</summary>
    internal const ushort ShardGroup6 = 0xcd6b;

    /// <summary>Eighth glass-shard angle-group loop at $86:CD8F.</summary>
    internal const ushort ShardGroup7 = 0xcd8f;

    /// <summary>Finite glass-sparkle animation at $86:CDB3.</summary>
    internal const ushort Sparkle = 0xcdb3;

    /// <summary>Total spritemap operands interleaved through the eight shard loops and finite sparkle program.</summary>
    public static int PresentationWordCount => 68;

    /// <summary>Number of distinct looping programs selected for glass-shard angle groups.</summary>
    internal static int ShardProgramCount => 8;

    /// <summary>Sixty-four shard and four sparkle art operands, each two bytes after its duration.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index < 64 ? ShardProgram(index / 8) + (index % 8) * 4 + 2 :
            Sparkle + (index - 64) * 4 + 2);
    }

    /// <summary>Eight 36-byte loops at $86:CC93..CDB2, each eight timed frames plus goto/self.</summary>
    internal static ushort ShardProgram(int index)
    {
        if ((uint)index >= ShardProgramCount) throw new IndexOutOfRangeException();
        return (ushort)(ShardGroup0 + index * 36);
    }

    /// <summary>Selects the native shard loop corresponding to one of the sixteen RNG-angle bins.</summary>
    /// <param name="animationIndex">Angle-bin index from zero through fifteen.</param>
    /// <returns>The bank-$86 entry address for the selected shard loop.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The angle-bin index is greater than fifteen.</exception>
    internal static ushort SelectShardProgram(ushort animationIndex)
    {
        if (animationIndex >= 16)
        {
            throw new ArgumentOutOfRangeException(
                nameof(animationIndex), animationIndex,
                "Mother Brain glass-shard animation index must be zero through fifteen.");
        }

        // $86:CE41 selects an angular sprite group from sixteen RNG-angle bins.
        // The asymmetric widths 1/2/3/2 repeat in the opposite half-turn.
        return animationIndex switch
        {
            0 => ShardGroup0,
            1 or 2 => ShardGroup1,
            3 or 4 or 5 => ShardGroup2,
            6 or 7 => ShardGroup3,
            8 => ShardGroup4,
            9 or 10 => ShardGroup5,
            11 or 12 or 13 => ShardGroup6,
            _ => ShardGroup7,
        };
    }

    /// <summary>Identifies projectile kinds whose instruction streams are defined by this catalog.</summary>
    /// <param name="kind">Projectile kind to classify.</param>
    /// <returns><see langword="true"/> for Mother Brain glass shards and glass sparkles.</returns>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.MotherBrainGlassShard or
        RoomEnemyProjectileKind.MotherBrainGlassSparkle;

    /// <summary>
    /// Exact aligned mechanics words only. Shard frame holds repeat the symmetric
    /// 4/3/2/3 cadence twice, followed by goto/self; sparkle alternates 6/8 then deletes.
    /// Interleaved artwork operands and interior-byte word starts remain rejected.
    /// </summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - ShardGroup0;
        if (offset >= 0 && offset < 8 * 36)
        {
            int local = offset % 36;
            if (local < 32 && local % 4 == 0)
            {
                int phase = local / 4 % 4;
                return (ushort)(4 - Math.Min(phase, 4 - phase));
            }
            if (local == 32) return EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY;
            if (local == 34) return ShardProgram(offset / 36);
        }
        int sparkleOffset = address - Sparkle;
        if (sparkleOffset >= 0 && sparkleOffset < 16 && sparkleOffset % 4 == 0)
            return (ushort)(6 + 2 * ((sparkleOffset / 4) & 1));
        if (sparkleOffset == 16) return EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete;
        throw new InvalidDataException(
            $"Mother Brain glass-projectile mechanics pointer $86:{address:X4} is not compiled.");
    }
}
