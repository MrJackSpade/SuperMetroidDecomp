namespace SuperMetroid.Core.Game;

/// <summary>One compiled n00b-tube projectile mechanics word at its bank-$86 address.</summary>
internal readonly record struct NoobTubeProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for the tube crack, its ten glass shards, and six released-air bubbles.
/// Their interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class NoobTubeProjectileInstructionProgramDefinitions
{
    /// <summary>N00b-tube crack animation at $86:D3D7.</summary>
    internal const ushort Crack = 0xd3d7;

    /// <summary>Released-air-bubble animation at $86:D652.</summary>
    internal const ushort ReleasedAirBubble = 0xd652;

    private static readonly ushort[] ShardPrograms =
        [0xd47d, 0xd4a1, 0xd4c5, 0xd4e9, 0xd50d, 0xd531, 0xd555, 0xd579, 0xd59d, 0xd5bd];

    private static readonly NoobTubeProjectileInstructionMechanicsWord[] Words = BuildWords();
    private static readonly ushort[] PresentationWords = BuildPresentationWords();

    internal static ReadOnlySpan<ushort> ShardInstructionLists => ShardPrograms;
    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static NoobTubeProjectileInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.NoobTubeCrack or
        RoomEnemyProjectileKind.NoobTubeShard or
        RoomEnemyProjectileKind.NoobTubeReleasedAirBubble;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            NoobTubeProjectileInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"N00b-tube projectile mechanics pointer $86:{address:X4} is not compiled.");
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

    private static NoobTubeProjectileInstructionMechanicsWord[] BuildWords()
    {
        var words = new List<NoobTubeProjectileInstructionMechanicsWord>(207);
        ushort[] crackOpeningDurations = [12, 10, 8, 6, 6, 6];
        AddTimedFrames(words, Crack, crackOpeningDurations);
        Add(words, 0xd3ef, EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY);
        Add(words, 0xd3f1, EnemyProjectileCodePointers.PreInstruction_NoobTubeCrackFlickering);
        AddTimedFrames(words, 0xd3f3, [1, 1, 1, 1, 1, 2, 3, 6, 9, 8]);
        Add(words, 0xd41b, EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY);
        Add(words, 0xd41d, EnemyProjectileCodePointers.PreInstruction_NoobTubeCrackFalling);
        AddTimedFrames(words, 0xd41f,
            [7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 16]);
        Add(words, 0xd46b, EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY);
        Add(words, 0xd46d, 6);
        AddTimedFrames(words, 0xd46f, [16, 16]);
        Add(words, 0xd477,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero);
        Add(words, 0xd479, 0xd46f);
        Add(words, 0xd47b, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete);

        foreach (ushort program in ShardPrograms)
            AddShardProgram(words, program, program == 0xd59d);

        Add(words, ReleasedAirBubble,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY);
        Add(words, 0xd654, EnemyProjectileCodePointers.PreInstruction_NoobTubeBubbleFlying);
        AddTimedFrames(words, 0xd656, [2, 2, 2, 2]);
        Add(words, 0xd666, EnemyProjectileCodePointers.Instruction_NoobTubeBubbleAssignFallingAngle);
        Add(words, 0xd668,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY);
        Add(words, 0xd66a, EnemyProjectileCodePointers.PreInstruction_NoobTubeBubbleFalling);
        AddTimedFrames(words, 0xd66c, [2, 2, 2, 2, 2, 2, 4, 4, 4, 4, 4]);
        Add(words, 0xd698, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete);
        return words.ToArray();
    }

    private static ushort[] BuildPresentationWords()
    {
        var words = new List<ushort>(90);
        AddPresentationFrames(words, Crack, 6);
        AddPresentationFrames(words, 0xd3f3, 10);
        AddPresentationFrames(words, 0xd41f, 19);
        AddPresentationFrames(words, 0xd46f, 2);
        foreach (ushort program in ShardPrograms)
        {
            bool compact = program == 0xd59d;
            words.Add(unchecked((ushort)(program + 6)));
            if (!compact)
                words.Add(unchecked((ushort)(program + 8)));
            words.Add(unchecked((ushort)(program + (compact ? 0x18 : 0x1a))));
            if (!compact)
                words.Add(unchecked((ushort)(program + 0x1c)));
        }
        AddPresentationFrames(words, 0xd656, 4);
        AddPresentationFrames(words, 0xd66c, 11);
        words.Sort();
        return words.ToArray();
    }

    private static void AddShardProgram(
        List<NoobTubeProjectileInstructionMechanicsWord> words,
        ushort program,
        bool compact)
    {
        ushort flicker = compact
            ? EnemyProjectileCodePointers.Instruction_NoobTubeShardFlicker
            : EnemyProjectileCodePointers.Instruction_NoobTubeShardReflectFlicker;
        int firstBranch = compact ? 0x08 : 0x0a;
        int assign = compact ? 0x0c : 0x0e;
        int secondTimer = compact ? 0x12 : 0x14;
        int secondFlicker = compact ? 0x16 : 0x18;
        int secondBranch = compact ? 0x1a : 0x1e;
        int delete = compact ? 0x1e : 0x22;

        Add(words, program, EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY);
        Add(words, program + 2, 0x0020);
        Add(words, program + 4, flicker);
        Add(words, program + firstBranch,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero);
        Add(words, program + firstBranch + 2, unchecked((ushort)(program + 4)));
        Add(words, program + assign,
            EnemyProjectileCodePointers.Instruction_NoobTubeShardAssignFallingAngle);
        Add(words, program + assign + 2,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY);
        Add(words, program + assign + 4,
            EnemyProjectileCodePointers.PreInstruction_NoobTubeShardFalling);
        Add(words, program + secondTimer,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY);
        Add(words, program + secondTimer + 2, 0x0110);
        Add(words, program + secondFlicker, flicker);
        Add(words, program + secondBranch,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero);
        Add(words, program + secondBranch + 2, unchecked((ushort)(program + secondFlicker)));
        Add(words, program + delete, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete);
    }

    private static void AddTimedFrames(
        List<NoobTubeProjectileInstructionMechanicsWord> words,
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
        List<NoobTubeProjectileInstructionMechanicsWord> words,
        int address,
        ushort value) => words.Add(new(unchecked((ushort)address), value));
}
