namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Phantoon's starting and destroyable flame instruction lists.
/// Interleaved spritemap selectors address separately installed artwork.
/// </summary>
internal abstract class PhantoonProjectileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_PhantoonDestroyableFlame_Idle</c> at $86:975C.</summary>
    internal const ushort DestroyableIdle = 0x975c;

    /// <summary><c>InstList_EnemyProj_PhantoonDestroyableFlame_Casual_HitGround</c> at $86:976C.</summary>
    internal const ushort CasualHitGround = 0x976c;

    /// <summary><c>InstList_EnemyProj_PhantoonDestroyableFlame_Casual_Bouncing</c> at $86:9772.</summary>
    internal const ushort CasualBouncing = 0x9772;

    /// <summary><c>InstList_EnemyProj_PhantoonDestroyableFlame_Casual_Resetting</c> at $86:9782.</summary>
    internal const ushort CasualResting = 0x9782;

    /// <summary><c>InstList_EnemyProj_PhantoonDestroyableFlame_Dying</c> at $86:979A.</summary>
    internal const ushort Dying = 0x979a;

    /// <summary><c>InstList_EnemyProj_PhantoonDestroyableFlame_Rain_Falling</c> at $86:97AC.</summary>
    internal const ushort RainImpact = 0x97ac;

    /// <summary><c>InstList_EnemyProj_PhantoonDestroyableFlame_Casual_Falling_0</c> at $86:97B4.</summary>
    internal const ushort CasualFalling = 0x97b4;

    /// <summary><c>InstList_EnemyProjectile_PhantoonStartingFlames</c> at $86:97E8.</summary>
    internal const ushort StartingFlame = 0x97e8;

    /// <summary><c>InstList_EnemyProjectile_PhantoonDestroyableFlame_Delete</c> at $86:97F8.</summary>
    internal const ushort Delete = 0x97f8;

    /// <summary><c>InstList_EnemyProjectile_Shot_PhantoonDestroyableFlames</c> at $86:97FA.</summary>
    internal const ushort DestroyableShot = 0x97fa;

    /// <summary>Five-tick flame pose cadence in86:975C..9806. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort FlamePoseFrames = 5;
    /// <summary>Impact dwell at86:97AC. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort RainImpactFrames = 8;
    /// <summary>Falling-phase repetition at86:97B6/97C6/97D6. Reviewed under #1165 as an authored repetition count: it only repeats the chosen frames, and no simulation quantity derives it.</summary>
    private const ushort FallingRepeatCount = 4;

    public static int MechanicsWordCount => 58;
    public static int PresentationWordCount => 31;

    /// <summary>Shared three-pose loops, counted falling cycles and impact/deletion tails.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 5) return LoopWord(DestroyableIdle, index);
        if (index < 7)
            return new((ushort)(CasualHitGround + (index - 5) * 4),
                index == 5 ? (ushort)1 : EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep);
        if (index < 12) return LoopWord(CasualBouncing, index - 7);
        if (index < 18) return Frame(CasualResting, index - 12, FlamePoseFrames);
        if (index < 23)
            return index < 22 ? Frame(Dying, index - 18, FlamePoseFrames)
                : new((ushort)(Dying + 16), EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete);
        if (index < 26)
            return index == 23 ? Frame(RainImpact, 0, RainImpactFrames)
                : new((ushort)(RainImpact + 4 + (index - 24) * 2),
                    index == 24 ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY : Dying);
        if (index < 44)
        {
            int phase = (index - 26) / 6;
            int word = (index - 26) % 6;
            ushort start = (ushort)(CasualFalling + phase * 16);
            int offset = word < 2 ? word * 2 : word < 4 ? 4 + (word - 2) * 4 : 12 + (word - 4) * 2;
            return new((ushort)(start + offset), word switch
            {
                0 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY,
                1 => FallingRepeatCount,
                2 or 3 => 1,
                4 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero,
                _ => (ushort)(start + 4),
            });
        }
        if (index < 46)
            return new((ushort)(CasualFalling + 48 + (index - 44) * 2),
                index == 44 ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY : CasualFalling);
        if (index < 51) return LoopWord(StartingFlame, index - 46);
        if (index == 51) return new(Delete, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete);
        if (index < 56) return Frame(DestroyableShot, index - 52, FlamePoseFrames);
        return new((ushort)(DestroyableShot + 16 + (index - 56) * 2),
            index == 56 ? EnemyProjectileCodePointers.Instruction_SpawnPhantoonDrop
                : EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete);
    }

    private static InstructionMechanicsWord Frame(ushort start, int frame, ushort duration) =>
        new((ushort)(start + frame * 4), duration);

    private static InstructionMechanicsWord LoopWord(ushort start, int word) =>
        word < 3 ? Frame(start, word, FlamePoseFrames)
            : new((ushort)(start + 12 + (word - 3) * 2),
                word == 3 ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY : start);

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 3) return (ushort)(DestroyableIdle + index * 4 + 2);
        if (index == 3) return CasualHitGround + 2;
        if (index < 7) return (ushort)(CasualBouncing + (index - 4) * 4 + 2);
        if (index < 13) return (ushort)(CasualResting + (index - 7) * 4 + 2);
        if (index < 17) return (ushort)(Dying + (index - 13) * 4 + 2);
        if (index == 17) return RainImpact + 2;
        if (index < 24)
            return (ushort)(CasualFalling + (index - 18) / 2 * 16 + (index % 2) * 4 + 6);
        if (index < 27) return (ushort)(StartingFlame + (index - 24) * 4 + 2);
        return (ushort)(DestroyableShot + (index - 27) * 4 + 2);
    }

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.PhantoonDestroyableFlame or
        RoomEnemyProjectileKind.PhantoonStartingFlame;

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
            $"Phantoon projectile mechanics pointer $86:{address:X4} is not compiled.");
    }
}
