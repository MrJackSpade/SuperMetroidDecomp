namespace SuperMetroid.Core.Game;

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

    /// <summary>Calculated bank-$86 entry pointers for the ten spawned glass-shard programs.</summary>
    internal static NoobTubeShardProgramSequence ShardInstructionLists => default;

    /// <summary>Number of compiled timer, control-flow, and pre-instruction words across the crack, shards, and released bubbles.</summary>
    public static int MechanicsWordCount => 207;

    /// <summary>Number of compiled spritemap-selector operands embedded in the projectile programs.</summary>
    public static int PresentationWordCount => 90;

    /// <summary>Returns the mechanics address and value at an ordinal in the flattened native program-word sequence.</summary>
    /// <param name="index">Zero-based ordinal in the 207-word sequence.</param>
    /// <returns>The bank-relative address and compiled value of that mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the compiled sequence.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index < 46) return CrackWord(index);
        if (index < 186) return ShardWord((index-46)/14,(index-46)%14);
        int bubble = index-186;
        if (bubble < 2) return new((ushort)(ReleasedAirBubble+2*bubble),bubble == 0
            ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY
            : EnemyProjectileCodePointers.PreInstruction_NoobTubeBubbleFlying);
        if (bubble < 6) return new((ushort)(ReleasedAirBubble+4+4*(bubble-2)),2);
        if (bubble < 9) return new((ushort)(ReleasedAirBubble+20+2*(bubble-6)),bubble switch
        {
            6 => EnemyProjectileCodePointers.Instruction_NoobTubeBubbleAssignFallingAngle,
            7 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY,
            _ => EnemyProjectileCodePointers.PreInstruction_NoobTubeBubbleFalling,
        });
        if (bubble < 20) return new((ushort)(ReleasedAirBubble+26+4*(bubble-9)),bubble < 15 ? (ushort)2 : (ushort)4);
        return new((ushort)(ReleasedAirBubble+70),EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete);
    }

    /// <summary>Maps a compiled selector ordinal to the address of its spritemap operand.</summary>
    /// <param name="index">Zero-based ordinal among the 90 presentation words.</param>
    /// <returns>The bank-relative address of the selected operand in an instruction program.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the presentation-word sequence.</exception>
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

    /// <summary>Calculates one mechanics word from the crack's 46-word program segment.</summary>
    /// <param name="index">Zero-based ordinal within the crack segment, from 0 through 45.</param>
    /// <returns>The bank-relative address and value for the selected timer, control, or pre-instruction word.</returns>
    private static InstructionMechanicsWord CrackWord(int index)
    {
        if (index < 6) return new((ushort)(Crack+4*index),(ushort)Math.Max(6,12-2*index));
        if (index < 8) return new((ushort)(Crack+24+2*(index-6)),index == 6
            ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY
            : EnemyProjectileCodePointers.PreInstruction_NoobTubeCrackFlickering);
        if (index < 18) return new((ushort)(CrackFlicker+4*(index-8)),index < 13 ? (ushort)1 : CrackFlickerTailDurations[index-13]);
        if (index < 20) return new((ushort)(Crack+68+2*(index-18)),index == 18
            ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY
            : EnemyProjectileCodePointers.PreInstruction_NoobTubeCrackFalling);
        if (index < 39) return new((ushort)(CrackFalling+4*(index-20)),index == 38 ? (ushort)16 : (ushort)7);
        if (index < 41) return new((ushort)(CrackTail-4+2*(index-39)),index == 39
            ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY : (ushort)6);
        if (index < 43) return new((ushort)(CrackTail+4*(index-41)),16);
        return new((ushort)(CrackTail+8+2*(index-43)),index switch
        {
            43 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero,
            44 => CrackTail,
            _ => EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete,
        });
    }

    /// <summary>Calculates one mechanics word within a shard's native program, accounting for the compact ninth entry.</summary>
    /// <param name="shard">Zero-based shard program ordinal from 0 through 9.</param>
    /// <param name="index">Zero-based word ordinal within that program, from 0 through 13.</param>
    /// <returns>The bank-relative address and value of the selected shard mechanics word.</returns>
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
        ushort flicker = compact ? EnemyProjectileCodePointers.Instruction_NoobTubeShardFlicker
            : EnemyProjectileCodePointers.Instruction_NoobTubeShardReflectFlicker;
        (int offset,ushort value) = index switch
        {
            0 => (0,EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY),
            1 => (2,(ushort)32),
            2 => (4,flicker),
            3 => (firstBranch,EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero),
            4 => (firstBranch+2,(ushort)(start+4)),
            5 => (assign,EnemyProjectileCodePointers.Instruction_NoobTubeShardAssignFallingAngle),
            6 => (assign+2,EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY),
            7 => (assign+4,EnemyProjectileCodePointers.PreInstruction_NoobTubeShardFalling),
            8 => (secondTimer,EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY),
            9 => (secondTimer+2,(ushort)272),
            10 => (secondFlicker,flicker),
            11 => (secondBranch,EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero),
            12 => (secondBranch+2,(ushort)(start+secondFlicker)),
            _ => (secondBranch+4,EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
        };
        return new((ushort)(start+offset),value);
    }
    /// <summary>Determines whether these compiled programs own the crack, shard, or released-air-bubble projectile kind.</summary>
    /// <param name="kind">Projectile kind to classify.</param>
    /// <returns><see langword="true"/> for a projectile driven by one of these instruction programs.</returns>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.NoobTubeCrack or
        RoomEnemyProjectileKind.NoobTubeShard or
        RoomEnemyProjectileKind.NoobTubeReleasedAirBubble;

    /// <summary>Looks up a compiled mechanics word by its bank-relative instruction address.</summary>
    /// <param name="address">Bank-$86 address of a timer, control-flow, or pre-instruction word.</param>
    /// <returns>The compiled 16-bit mechanics value at that address.</returns>
    /// <exception cref="InvalidDataException">The address is not part of a compiled mechanics word.</exception>
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
    /// <summary>Returns a non-generic enumerator over this sequence.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
