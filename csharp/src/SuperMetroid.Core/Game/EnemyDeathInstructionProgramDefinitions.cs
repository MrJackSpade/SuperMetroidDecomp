namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for the five generic enemy-death animations and their shared blank
/// respawn tail. The thirty-one spritemap operands resolve through extracted presentation art.
/// </summary>
internal abstract class EnemyDeathInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_Pickup_HandleRespawningEnemy</c> at $86:ECA3.</summary>
    internal const ushort RespawnTail = 0xeca3;

    /// <summary>Big-explosion death program at $86:ECAB.</summary>
    internal const ushort BigExplosion = 0xecab;

    /// <summary>Mini-Kraid death program at $86:ECC5.</summary>
    internal const ushort MiniKraidExplosion = 0xecc5;

    /// <summary>Normal-explosion death program at $86:ED4B.</summary>
    internal const ushort NormalExplosion = 0xed4b;

    /// <summary>Small-explosion death program at $86:ED69.</summary>
    internal const ushort SmallExplosion = 0xed69;

    /// <summary>Samus-contact death program at $86:EDFF.</summary>
    internal const ushort KilledBySamusContact = 0xedff;

    /// <summary>Blank respawn countdown 64 at $86:ECA3. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort RespawnBlankDuration = 64;

    /// <summary>Big-explosion repetition count 5, TimerInY operand at $86:ECAD. Reviewed under #1165 as an authored repetition count: it only repeats the chosen frames, and no simulation quantity derives it.</summary>
    private const ushort BigExplosionRepetitions = 5;

    /// <summary>Mini-Kraid repetition count 16, TimerInY operand at $86:ECC7. Reviewed under #1165 as an authored repetition count: it only repeats the chosen frames, and no simulation quantity derives it.</summary>
    private const ushort MiniKraidRepetitions = 16;

    /// <summary>Big-explosion blank hold 8 at $86:ECB7, after two sprite-spawn commands. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort BigExplosionBlankDuration = 8;

    /// <summary>Mini-Kraid blank hold 8 at $86:ECD5, after three sprite-spawn commands. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort MiniKraidBlankDuration = 8;

    /// <summary>Normal-explosion pose hold 5 at $86:ED4B..ED61. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort NormalExplosionPoseDuration = 5;

    /// <summary>Samus-contact death pose hold 2 at $86:EDFF..EE3D. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort ContactDeathPoseDuration = 2;
    public static int PresentationWordCount => 31;

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index switch
        {
            0 => RespawnTail + 2,
            1 => BigExplosion + 14,
            2 => MiniKraidExplosion + 18,
            < 9 => NormalExplosion + FrameVisualOffset(index - 3, 3),
            < 15 => SmallExplosion + FrameVisualOffset(index - 9, 3),
            _ => KilledBySamusContact + FrameVisualOffset(index - 15, 7),
        });
    }

    private static int FrameVisualOffset(int frame, int framesBeforeSound) =>
        frame * 4 + 2 + (frame >= framesBeforeSound ? 2 : 0);

    internal static bool Owns(RoomEnemyProjectileKind kind, ushort address) =>
        (kind == RoomEnemyProjectileKind.EnemyDeathExplosion ||
         (kind == RoomEnemyProjectileKind.EnemyDeathPickup && address >= RespawnTail && address < BigExplosion)) &&
        TryWord(address, out _);

    internal static ushort ReadMechanicsWord(ushort address) => TryWord(address, out ushort value)
        ? value : throw new InvalidDataException($"Generic enemy-death mechanics pointer $86:{address:X4} is not compiled.");

    internal static bool TryWord(ushort address, out ushort value)
    {
        ushort? result = null;
        if (address >= RespawnTail && address < BigExplosion && ((address - RespawnTail) & 1) == 0)
            result = (address - RespawnTail) switch
            {
                0 => RespawnBlankDuration,
                4 => (ushort)EnemyProjectileInstruction.Pickup_HandleRespawningEnemy,
                6 => (ushort)EnemyProjectileInstruction.Delete,
                _ => null,
            };
        else if (address >= BigExplosion && address <= BigExplosion + 24 && ((address - BigExplosion) & 1) == 0)
            result = RepeatedExplosionWord(address - BigExplosion, miniKraid: false);
        else if (address >= MiniKraidExplosion && address <= MiniKraidExplosion + 28 && ((address - MiniKraidExplosion) & 1) == 0)
            result = RepeatedExplosionWord(address - MiniKraidExplosion, miniKraid: true);
        else if (address >= NormalExplosion && address <= NormalExplosion + 28 && ((address - NormalExplosion) & 1) == 0)
            result = AnimatedExplosionWord(address - NormalExplosion, small: false, contact: false);
        else if (address >= SmallExplosion && address <= SmallExplosion + 28 && ((address - SmallExplosion) & 1) == 0)
            result = AnimatedExplosionWord(address - SmallExplosion, small: true, contact: false);
        else if (address >= KilledBySamusContact && address <= KilledBySamusContact + 68 && ((address - KilledBySamusContact) & 1) == 0)
            result = AnimatedExplosionWord(address - KilledBySamusContact, small: false, contact: true);
        value = result.GetValueOrDefault();
        return result.HasValue;
    }

    // Big and Mini-Kraid deaths repeat two or three sprite spawns, one blank frame,
    // sound and timer/goto control before becoming a pickup.
    private static ushort? RepeatedExplosionWord(int offset, bool miniKraid)
    {
        int firstFrameOffset = miniKraid ? 16 : 12;
        if (offset == 0) return (ushort)EnemyProjectileInstruction.TimerInY;
        if (offset == 2) return miniKraid ? MiniKraidRepetitions : BigExplosionRepetitions;
        if (offset < firstFrameOffset)
            return offset % 4 == 0
                ? miniKraid ? (ushort)EnemyProjectileInstruction.EnemyDeathExpl_SpawnSpriteObjectInY_20
                    : (ushort)EnemyProjectileInstruction.EnemyDeathExpl_SpawnSpriteObjectInY_10
                : (ushort)(3 + 9 * ((offset - 6) / 4));
        return (offset - firstFrameOffset) switch
        {
            0 => miniKraid ? MiniKraidBlankDuration : BigExplosionBlankDuration,
            4 => (ushort)EnemyProjectileInstruction.EDeathExplo_QueueSmallExplosionSoundFX,
            6 => (ushort)EnemyProjectileInstruction.DecrementTimer_GotoYIfNonZero,
            8 => (ushort)((miniKraid ? MiniKraidExplosion : BigExplosion) + 4),
            10 => (ushort)EnemyProjectileInstruction.EnemyDeathExplosion_BecomePickup,
            12 => (ushort)EnemyProjectileInstruction.Delete,
            _ => null,
        };
    }

    // Six normal/small frames or sixteen contact frames, with one sound command
    // inserted before the second phase and the same pickup/delete tail.
    private static ushort? AnimatedExplosionWord(int offset, bool small, bool contact)
    {
        int sound = contact ? 28 : 12;
        int pickup = contact ? 66 : 26;
        if (offset == sound)
            return contact ? (ushort)EnemyProjectileInstruction.EDeathExplo_QueueContactKilledSoundFX
                : (ushort)EnemyProjectileInstruction.EnemyDeathExpl_QueueEnemyKilledSoundFX;
        if (offset == pickup) return (ushort)EnemyProjectileInstruction.EnemyDeathExplosion_BecomePickup;
        if (offset == pickup + 2) return (ushort)EnemyProjectileInstruction.Delete;
        int normalized = offset - (offset > sound ? 2 : 0);
        if (normalized % 4 != 0) return null;
        if (contact) return ContactDeathPoseDuration;
        if (!small) return NormalExplosionPoseDuration;
        return SmallExplosionAnimationDefinitions.Duration(normalized / 4);
    }
}
