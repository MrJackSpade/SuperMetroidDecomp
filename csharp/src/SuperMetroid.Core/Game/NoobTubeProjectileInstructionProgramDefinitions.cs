namespace SuperMetroid.Core.Game;

/// <summary>The six bank-$86 movement callbacks the n00b-tube projectile lists install.</summary>
internal enum NoobTubeProjectilePreInstruction : ushort
{
    /// <summary>$86:D7BF: flicker the n00b-tube crack actor.</summary>
    CrackFlickering = (ushort)EnemyProjectilePreInstruction.NoobTubeCrackFlickering,
    /// <summary>$86:D7DE: move the detached n00b-tube crack downward.</summary>
    CrackFalling = (ushort)EnemyProjectilePreInstruction.NoobTubeCrackFalling,
    /// <summary>$86:D7FD: move a newly emitted n00b-tube shard.</summary>
    ShardFlying = (ushort)EnemyProjectilePreInstruction.NoobTubeShardFlying,
    /// <summary>$86:D83D: rotate and fall after a shard's initial flight.</summary>
    ShardFalling = (ushort)EnemyProjectilePreInstruction.NoobTubeShardFalling,
    /// <summary>$86:D89F: rotate and fall a released-air bubble.</summary>
    BubbleFalling = (ushort)EnemyProjectilePreInstruction.NoobTubeBubbleFalling,
    /// <summary>$86:D8DF: move a released-air bubble vertically.</summary>
    BubbleFlying = (ushort)EnemyProjectilePreInstruction.NoobTubeBubbleFlying,
}

/// <summary>
/// Compiled control for the tube crack, its ten glass shards, and six released-air bubbles.
/// Their interleaved spritemap operands select separately installed presentation data.
/// </summary>
internal abstract class NoobTubeProjectileInstructionProgramDefinitions
{
    /// <summary>N00b-tube crack animation at $86:D3D7.</summary>
    internal const ushort Crack = 0xd3d7;

    /// <summary>Released-air-bubble animation at $86:D652.</summary>
    internal const ushort ReleasedAirBubble = 0xd652;

    /// <summary>$86:D47D begins ten shard programs; the ninth omits both reflected operands.</summary>
    internal const ushort FirstShard = 0xd47d;
    /// <summary>$86:D3F3 begins ten crack flicker poses after the pre-instruction handoff.</summary>
    private const ushort CrackFlicker = Crack + 28;
    /// <summary>$86:D41F begins the falling crack poses.</summary>
    private const ushort CrackFalling = Crack + 72;
    /// <summary>$86:D46F begins the two-pose counted tail.</summary>
    private const ushort CrackTail = Crack + 152;
    /// <summary>Five irregular final flicker holds at $86:D407-D417. Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.</summary>
    private static ReadOnlySpan<ushort> CrackFlickerTailDurations => [2,3,6,9,8];

    internal static NoobTubeShardProgramSequence ShardInstructionLists => default;
    public static int MechanicsWordCount => 207;
    public static int PresentationWordCount => 90;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index < 46) return CrackWord(index);
        if (index < 186) return ShardWord((index-46)/14,(index-46)%14);
        int bubble = index-186;
        if (bubble < 2) return new((ushort)(ReleasedAirBubble+2*bubble),bubble == 0
            ? (ushort)EnemyProjectileInstruction.PreInstructionInY
            : (ushort)EnemyProjectilePreInstruction.NoobTubeBubbleFlying);
        if (bubble < 6) return new((ushort)(ReleasedAirBubble+4+4*(bubble-2)),2);
        if (bubble < 9) return new((ushort)(ReleasedAirBubble+20+2*(bubble-6)),bubble switch
        {
            6 => (ushort)EnemyProjectileInstruction.NoobTubeBubbleAssignFallingAngle,
            7 => (ushort)EnemyProjectileInstruction.PreInstructionInY,
            _ => (ushort)EnemyProjectilePreInstruction.NoobTubeBubbleFalling,
        });
        if (bubble < 20) return new((ushort)(ReleasedAirBubble+26+4*(bubble-9)),bubble < 15 ? (ushort)2 : (ushort)4);
        return new((ushort)(ReleasedAirBubble+70),(ushort)EnemyProjectileInstruction.Delete);
    }

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index < 6) return (ushort)(Crack+2+4*index);
        if (index < 16) return (ushort)(CrackFlicker+2+4*(index-6));
        if (index < 35) return (ushort)(CrackFalling+2+4*(index-16));
        if (index < 37) return (ushort)(CrackTail+2+4*(index-35));
        if (index < 75)
        {
            int visual = index-37;
            int shard = visual < 32 ? visual/4 : visual < 34 ? 8 : 9;
            int local = visual < 32 ? visual%4 : visual < 34 ? visual-32 : visual-34;
            int offset = shard == 8 ? 6+18*local : 6+20*(local/2)+2*(local%2);
            return (ushort)(ShardInstructionLists[shard]+offset);
        }
        if (index < 79) return (ushort)(ReleasedAirBubble+6+4*(index-75));
        return (ushort)(ReleasedAirBubble+28+4*(index-79));
    }

    private static InstructionMechanicsWord CrackWord(int index)
    {
        if (index < 6) return new((ushort)(Crack+4*index),(ushort)Math.Max(6,12-2*index));
        if (index < 8) return new((ushort)(Crack+24+2*(index-6)),index == 6
            ? (ushort)EnemyProjectileInstruction.PreInstructionInY
            : (ushort)EnemyProjectilePreInstruction.NoobTubeCrackFlickering);
        if (index < 18) return new((ushort)(CrackFlicker+4*(index-8)),index < 13 ? (ushort)1 : CrackFlickerTailDurations[index-13]);
        if (index < 20) return new((ushort)(Crack+68+2*(index-18)),index == 18
            ? (ushort)EnemyProjectileInstruction.PreInstructionInY
            : (ushort)EnemyProjectilePreInstruction.NoobTubeCrackFalling);
        if (index < 39) return new((ushort)(CrackFalling+4*(index-20)),index == 38 ? (ushort)16 : (ushort)7);
        if (index < 41) return new((ushort)(CrackTail-4+2*(index-39)),index == 39
            ? (ushort)EnemyProjectileInstruction.TimerInY : (ushort)6);
        if (index < 43) return new((ushort)(CrackTail+4*(index-41)),16);
        return new((ushort)(CrackTail+8+2*(index-43)),index switch
        {
            43 => (ushort)EnemyProjectileInstruction.DecrementTimer_GotoYIfNonZero,
            44 => CrackTail,
            _ => (ushort)EnemyProjectileInstruction.Delete,
        });
    }

    private static InstructionMechanicsWord ShardWord(int shard,int index)
    {
        ushort start = ShardInstructionLists[shard];
        bool compact = shard == 8;
        int skipped = compact ? 2 : 4;
        int firstBranch = 6+skipped;
        int assign = firstBranch+4;
        int secondTimer = assign+6;
        int secondFlicker = secondTimer+4;
        int secondBranch = secondFlicker+2+skipped;
        ushort flicker = compact ? (ushort)EnemyProjectileInstruction.NoobTubeShardFlicker
            : (ushort)EnemyProjectileInstruction.NoobTubeShardReflectFlicker;
        (int offset,ushort value) = index switch
        {
            0 => (0,(ushort)EnemyProjectileInstruction.TimerInY),
            1 => (2,(ushort)32),
            2 => (4,flicker),
            3 => (firstBranch,(ushort)EnemyProjectileInstruction.DecrementTimer_GotoYIfNonZero),
            4 => (firstBranch+2,(ushort)(start+4)),
            5 => (assign,(ushort)EnemyProjectileInstruction.NoobTubeShardAssignFallingAngle),
            6 => (assign+2,(ushort)EnemyProjectileInstruction.PreInstructionInY),
            7 => (assign+4,(ushort)EnemyProjectilePreInstruction.NoobTubeShardFalling),
            8 => (secondTimer,(ushort)EnemyProjectileInstruction.TimerInY),
            9 => (secondTimer+2,(ushort)272),
            10 => (secondFlicker,flicker),
            11 => (secondBranch,(ushort)EnemyProjectileInstruction.DecrementTimer_GotoYIfNonZero),
            12 => (secondBranch+2,(ushort)(start+secondFlicker)),
            _ => (secondBranch+4,(ushort)EnemyProjectileInstruction.Delete),
        };
        return new((ushort)(start+offset),value);
    }
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.NoobTubeCrack or
        RoomEnemyProjectileKind.NoobTubeShard or
        RoomEnemyProjectileKind.NoobTubeReleasedAirBubble;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            InstructionMechanicsWord candidate = MechanicsWord(middle);
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

}

/// <summary>Ten native shard entry points: nine36-byte programs and one32-byte unreflected ninth program.</summary>
public readonly struct NoobTubeShardProgramSequence : IReadOnlyList<ushort>
{
    /// <summary>Ten entry pointers for the ten native shard spawn parameters, not a count of currently live projectiles.</summary>
    public int Count => 10;
    /// <summary>Calculates a bank-$86 instruction-list entry from $D47D; ordinal eight uses the compact $D59D program and ordinal nine begins at $D5BD.</summary>
    /// <param name="index">Zero-based shard ordinal 0..9, corresponding to native even parameter 2 times this index ($00..$12).</param>
    /// <returns>A 16-bit bank-relative program pointer, not a full cartridge address or spritemap operand.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside 0..9.</exception>
    public ushort this[int index] => (uint)index < Count
        ? (ushort)(NoobTubeProjectileInstructionProgramDefinitions.FirstShard+36*index-(index>8?4:0))
        : throw new IndexOutOfRangeException();
    /// <summary>Enumerates calculated entry pointers in native spawn-parameter order without retaining an array or mutable projectile state.</summary>
    /// <returns>An independent enumerator over all ten bank-relative pointers.</returns>
    public IEnumerator<ushort> GetEnumerator()
    {
        for(int index=0;index<Count;index++) yield return this[index];
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}