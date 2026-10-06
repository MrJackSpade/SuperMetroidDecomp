namespace SuperMetroid.Core.Game;

/// <summary>One compiled generic enemy-death mechanics word at its bank-$86 address.</summary>
internal readonly record struct EnemyDeathInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for the five generic enemy-death animations and their shared blank
/// respawn tail. The thirty-one spritemap operands resolve through extracted presentation art.
/// </summary>
internal static class EnemyDeathInstructionProgramDefinitions
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

    /// <summary>Unresolved blank respawn countdown 64 at $86:ECA3; no timing exception approved.</summary>
    private const ushort UnresolvedRespawnBlankDuration = 64;

    /// <summary>Unresolved big-explosion repetition count 5, TimerInY operand at $86:ECAD.</summary>
    private const ushort UnresolvedBigExplosionRepetitions = 5;

    /// <summary>Unresolved Mini-Kraid repetition count 16, TimerInY operand at $86:ECC7.</summary>
    private const ushort UnresolvedMiniKraidRepetitions = 16;

    /// <summary>Unresolved big-explosion blank hold 8 at $86:ECB7, after two sprite-spawn commands.</summary>
    private const ushort UnresolvedBigExplosionBlankDuration = 8;

    /// <summary>Unresolved Mini-Kraid blank hold 8 at $86:ECD5, after three sprite-spawn commands.</summary>
    private const ushort UnresolvedMiniKraidBlankDuration = 8;

    /// <summary>Unresolved normal-explosion pose hold 5 at $86:ED4B..ED61.</summary>
    private const ushort UnresolvedNormalExplosionPoseDuration = 5;

    /// <summary>Unresolved Samus-contact death pose hold 2 at $86:EDFF..EE3D.</summary>
    private const ushort UnresolvedContactDeathPoseDuration = 2;

    internal static int MechanicsWordCount => 66;
    internal static int PresentationWordCount => 31;

    internal static EnemyDeathInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int address;
        if (index < 3) address = RespawnTail + (index == 0 ? 0 : 2 * (index + 1));
        else if (index < 15) address = BigExplosion + LoopMechanicsOffset(index - 3, 12);
        else if (index < 29) address = MiniKraidExplosion + LoopMechanicsOffset(index - 15, 16);
        else if (index < 38) address = NormalExplosion + FrameMechanicsOffset(index - 29, 3, 6);
        else if (index < 47) address = SmallExplosion + FrameMechanicsOffset(index - 38, 3, 6);
        else address = KilledBySamusContact + FrameMechanicsOffset(index - 47, 7, 16);
        return new((ushort)address, ReadMechanicsWord((ushort)address));
    }

    internal static ushort PresentationWordAddress(int index)
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

    private static int LoopMechanicsOffset(int index, int delayOffset) =>
        index * 2 + (index > delayOffset / 2 ? 2 : 0);

    private static int FrameMechanicsOffset(int index, int framesBeforeSound, int frameCount) =>
        index <= framesBeforeSound ? 4 * index
        : index <= frameCount ? 4 * (index - 1) + 2
        : 4 * frameCount + 2 + 2 * (index - frameCount - 1);

    private static int FrameVisualOffset(int frame, int framesBeforeSound) =>
        frame * 4 + 2 + (frame >= framesBeforeSound ? 2 : 0);

    internal static bool Owns(RoomEnemyProjectileKind kind, ushort address) =>
        (kind == RoomEnemyProjectileKind.EnemyDeathExplosion ||
         (kind == RoomEnemyProjectileKind.EnemyDeathPickup && address >= RespawnTail && address < BigExplosion)) &&
        TryWord(address, out _);

    internal static ushort ReadMechanicsWord(ushort address) => TryWord(address, out ushort value)
        ? value : throw new InvalidDataException($"Generic enemy-death mechanics pointer $86:{address:X4} is not compiled.");

    internal static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == EnemyProjectileCodePointers.BankBase &&
        (TryWord(unchecked((ushort)address), out _) || TryWord(unchecked((ushort)(address - 1)), out _));

    private static bool TryWord(ushort address, out ushort value)
    {
        ushort? result = null;
        if (address >= RespawnTail && address < BigExplosion && ((address - RespawnTail) & 1) == 0)
            result = (address - RespawnTail) switch
            {
                0 => UnresolvedRespawnBlankDuration,
                4 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_Pickup_HandleRespawningEnemy,
                6 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete,
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
        if (offset == 0) return EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY;
        if (offset == 2) return miniKraid ? UnresolvedMiniKraidRepetitions : UnresolvedBigExplosionRepetitions;
        if (offset < firstFrameOffset)
            return offset % 4 == 0
                ? miniKraid ? EnemyProjectileCodePointers.Instruction_EnemyProj_EnemyDeathExpl_SpawnSpriteObjectInY_20
                    : EnemyProjectileCodePointers.Instruction_EnemyProj_EnemyDeathExpl_SpawnSpriteObjectInY_10
                : (ushort)(3 + 9 * ((offset - 6) / 4));
        return (offset - firstFrameOffset) switch
        {
            0 => miniKraid ? UnresolvedMiniKraidBlankDuration : UnresolvedBigExplosionBlankDuration,
            4 => EnemyProjectileCodePointers.Instruction_EnemyProj_EDeathExplo_QueueSmallExplosionSoundFX,
            6 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero,
            8 => (ushort)((miniKraid ? MiniKraidExplosion : BigExplosion) + 4),
            10 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_EnemyDeathExplosion_BecomePickup,
            12 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete,
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
            return contact ? EnemyProjectileCodePointers.Instruction_EnemyProj_EDeathExplo_QueueContactKilledSoundFX
                : EnemyProjectileCodePointers.Instruction_EnemyProj_EnemyDeathExpl_QueueEnemyKilledSoundFX;
        if (offset == pickup) return EnemyProjectileCodePointers.Instruction_EnemyProjectile_EnemyDeathExplosion_BecomePickup;
        if (offset == pickup + 2) return EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete;
        int normalized = offset - (offset > sound ? 2 : 0);
        if (normalized % 4 != 0) return null;
        if (contact) return UnresolvedContactDeathPoseDuration;
        if (!small) return UnresolvedNormalExplosionPoseDuration;
        return SmallExplosionAnimationDefinitions.Duration(normalized / 4);
    }
}
