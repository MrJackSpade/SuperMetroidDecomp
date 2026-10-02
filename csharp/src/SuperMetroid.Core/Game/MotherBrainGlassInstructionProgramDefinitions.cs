namespace SuperMetroid.Core.Game;

/// <summary>One compiled Mother Brain glass-projectile mechanics word at its bank-$86 address.</summary>
internal readonly record struct MotherBrainGlassInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Mother Brain's eight glass-shard loops and finite sparkle program.
/// Their interleaved spritemap operands resolve through extracted presentation art.
/// </summary>
internal static class MotherBrainGlassInstructionProgramDefinitions
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

    private static readonly MotherBrainGlassInstructionMechanicsWord[] Words = BuildWords();
    private static readonly ushort[] PresentationWords = BuildPresentationWords();

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static int ShardProgramCount => 8;
    internal static MotherBrainGlassInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];
    /// <summary>Eight 36-byte loops at $86:CC93..CDB2, each eight timed frames plus goto/self.</summary>
    internal static ushort ShardProgram(int index)
    {
        if ((uint)index >= ShardProgramCount) throw new IndexOutOfRangeException();
        return (ushort)(ShardGroup0 + index * 36);
    }

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

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.MotherBrainGlassShard or
        RoomEnemyProjectileKind.MotherBrainGlassSparkle;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            MotherBrainGlassInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Mother Brain glass-projectile mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < Words.Length; index++)
        {
            ushort wordAddress = Words[index].Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }

    private static MotherBrainGlassInstructionMechanicsWord[] BuildWords()
    {
        var words = new List<MotherBrainGlassInstructionMechanicsWord>(85);
        ushort[] shardDurations = [4, 3, 2, 3, 4, 3, 2, 3];
        for (int group = 0; group < ShardProgramCount; group++)
        {
            ushort program = ShardProgram(group);
            AddTimedFrames(words, program, shardDurations);
            Add(words, program + 0x20,
                EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY);
            Add(words, program + 0x22, program);
        }

        AddTimedFrames(words, Sparkle, [6, 8, 6, 8]);
        Add(words, Sparkle + 0x10,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete);
        return words.ToArray();
    }

    private static ushort[] BuildPresentationWords()
    {
        var words = new List<ushort>(68);
        for (int group = 0; group < ShardProgramCount; group++)
            AddPresentationFrames(words, ShardProgram(group), 8);
        AddPresentationFrames(words, Sparkle, 4);
        return words.ToArray();
    }

    private static void AddTimedFrames(
        List<MotherBrainGlassInstructionMechanicsWord> words,
        ushort start,
        ReadOnlySpan<ushort> durations)
    {
        for (int index = 0; index < durations.Length; index++)
            Add(words, start + index * 4, durations[index]);
    }

    private static void AddPresentationFrames(List<ushort> words, ushort start, int count)
    {
        for (int index = 0; index < count; index++)
            words.Add(unchecked((ushort)(start + index * 4 + 2)));
    }

    private static void Add(
        List<MotherBrainGlassInstructionMechanicsWord> words,
        int address,
        ushort value) => words.Add(new(unchecked((ushort)address), value));
}
