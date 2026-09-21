namespace SuperMetroid.Core.Game;

/// <summary>One compiled Mother Brain glass-projectile mechanics word at its bank-$86 address.</summary>
internal readonly record struct MotherBrainGlassInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Mother Brain's eight glass-shard loops and finite sparkle program.
/// Their interleaved spritemap operands remain live cartridge presentation data.
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

    private static readonly ushort[] ShardPrograms =
    [
        ShardGroup0, ShardGroup1, ShardGroup2, ShardGroup3,
        ShardGroup4, ShardGroup5, ShardGroup6, ShardGroup7,
    ];

    private static readonly ushort[] ShardSelectorPrograms =
    [
        ShardGroup0,
        ShardGroup1,
        ShardGroup1,
        ShardGroup2,
        ShardGroup2,
        ShardGroup2,
        ShardGroup3,
        ShardGroup3,
        ShardGroup4,
        ShardGroup5,
        ShardGroup5,
        ShardGroup6,
        ShardGroup6,
        ShardGroup6,
        ShardGroup7,
        ShardGroup7,
    ];

    private static readonly MotherBrainGlassInstructionMechanicsWord[] Words = BuildWords();
    private static readonly ushort[] PresentationWords = BuildPresentationWords();

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static int ShardProgramCount => ShardPrograms.Length;
    internal static MotherBrainGlassInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];
    internal static ushort ShardProgram(int index) => ShardPrograms[index];

    internal static ushort SelectShardProgram(ushort animationIndex)
    {
        if (animationIndex >= ShardSelectorPrograms.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(animationIndex), animationIndex,
                "Mother Brain glass-shard animation index must be zero through fifteen.");
        }

        return ShardSelectorPrograms[animationIndex];
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
        foreach (ushort program in ShardPrograms)
        {
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
        foreach (ushort program in ShardPrograms)
            AddPresentationFrames(words, program, 8);
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
