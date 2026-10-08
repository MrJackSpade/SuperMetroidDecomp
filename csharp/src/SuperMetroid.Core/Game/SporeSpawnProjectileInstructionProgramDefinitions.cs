namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Spore Spawn's stalk, ceiling emitter, and spore projectiles at
/// $86:DC00-$DC58. Their seventeen spritemap operands identify compiled presentation.
/// </summary>
internal abstract class SporeSpawnProjectileInstructionProgramDefinitions
{
    /// <summary>Closed/shot ceiling-emitter program at $86:DC00.</summary>
    internal const ushort SpawnerClosed = 0xdc00;
    /// <summary>Ceiling-emitter release program at $86:DC06.</summary>
    internal const ushort SpawnerRelease = 0xdc06;
    /// <summary>Looping airborne-spore program at $86:DC1E.</summary>
    internal const ushort Spore = 0xdc1e;
    /// <summary>Spore Spawn stalk display program at $86:DC2E.</summary>
    internal const ushort Stalk = 0xdc2e;
    /// <summary>Shot-spore explosion/drop program at $86:DC34.</summary>
    internal const ushort SporeShot = 0xdc34;

    /// <summary>
    /// $86:DC06 release-frame holds; the spawn callback lies between the second and third frames.
    /// Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.
    /// </summary>
    private static readonly ushort[] ReleaseDurations = [1, 6, 16, 6, 1];
    /// <summary>
    /// $86:DC34 shot-spore display holds around property, sound and drop callbacks.
    /// Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.
    /// </summary>
    private static readonly ushort[] ShotDurations = [1, 3, 6, 5, 5, 5, 6];
    // Closed-emitter hold 1 and airborne/stalk holds 5 are the same authored cadence (reviewed under #1165).
    public static int MechanicsWordCount => 28;
    public static int PresentationWordCount => 17;

    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index < 2) return new((ushort)(SpawnerClosed + 4 * index), index == 0 ? (ushort)1 : EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep);
        if (index < 9)
        {
            int local = index - 2;
            int offset = local < 3 ? 4 * local : 10 + 4 * (local - 3);
            ushort value = local switch
            {
                0 or 1 => ReleaseDurations[local],
                4 or 5 => ReleaseDurations[local - 1],
                2 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_SporeSpawner_SpawnSpore,
                3 => ReleaseDurations[2],
                _ => EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep,
            };
            return new((ushort)(SpawnerRelease + offset), value);
        }
        if (index < 14)
        {
            int local = index - 9;
            return new((ushort)(Spore + (local < 4 ? 4 * local : 14)),
                local < 3 ? (ushort)5 : local == 3 ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY : Spore);
        }
        if (index < 16) return new((ushort)(Stalk + 4 * (index - 14)), index == 14 ? (ushort)5 : EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep);
        int shot = index - 16;
        if (shot == 0) return new(SporeShot, ShotDurations[0]);
        if (shot == 1) return new((ushort)(SporeShot + 4), EnemyProjectileCodePointers.Instruction_EnemyProjectile_Spores_SetProperties3000);
        if (shot < 5) return new((ushort)(SporeShot + 6 + 4 * (shot - 2)), ShotDurations[shot - 1]);
        if (shot == 5) return new((ushort)(SporeShot + 18), EnemyProjectileCodePointers.Instruction_EnemyProj_EnemyDeathExpl_QueueEnemyKilledSoundFX);
        if (shot < 9) return new((ushort)(SporeShot + 20 + 4 * (shot - 6)), ShotDurations[shot - 2]);
        return new((ushort)(SporeShot + 32 + 2 * (shot - 9)), shot switch
        {
            9 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_Spores_SpawnEnemyDrops,
            10 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY,
            _ => CommonEnemyProjectileInstructionProgramDefinitions.Delete,
        });
    }

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index == 0) return SpawnerClosed + 2;
        if (index < 6) return (ushort)(SpawnerRelease + 2 + 4 * (index - 1) + (index < 3 ? 0 : 2));
        if (index < 9) return (ushort)(Spore + 2 + 4 * (index - 6));
        if (index == 9) return Stalk + 2;
        int shot = index - 10;
        return (ushort)(SporeShot + (shot == 0 ? 2 : shot < 4 ? 8 + 4 * (shot - 1) : 22 + 4 * (shot - 4)));
    }
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.SporeSpawnStalk or
        RoomEnemyProjectileKind.SporeSpawnSpore or
        RoomEnemyProjectileKind.SporeSpawnSpawner;

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
            $"Spore Spawn projectile mechanics pointer $86:{address:X4} is not compiled.");
    }
}
