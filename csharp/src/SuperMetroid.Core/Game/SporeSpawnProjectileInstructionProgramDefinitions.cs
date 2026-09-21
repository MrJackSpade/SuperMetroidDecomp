namespace SuperMetroid.Core.Game;

internal readonly record struct SporeSpawnProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Spore Spawn's stalk, ceiling emitter, and spore projectiles at
/// $86:DC00-$DC58. Their seventeen spritemap operands remain live presentation data.
/// </summary>
internal static class SporeSpawnProjectileInstructionProgramDefinitions
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

    private static readonly SporeSpawnProjectileInstructionMechanicsWord[] Words =
    [
        new(SpawnerClosed, 1),
        new(0xdc04, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(SpawnerRelease, 1), new(0xdc0a, 6),
        new(0xdc0e, EnemyProjectileCodePointers.Instruction_EnemyProjectile_SporeSpawner_SpawnSpore),
        new(0xdc10, 0x10), new(0xdc14, 6), new(0xdc18, 1),
        new(0xdc1c, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(Spore, 5), new(0xdc22, 5), new(0xdc26, 5),
        new(0xdc2a, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xdc2c, Spore),
        new(Stalk, 5),
        new(0xdc32, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(SporeShot, 1),
        new(0xdc38, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Spores_SetProperties3000),
        new(0xdc3a, 3), new(0xdc3e, 6), new(0xdc42, 5),
        new(0xdc46, EnemyProjectileCodePointers.Instruction_EnemyProj_EnemyDeathExpl_QueueEnemyKilledSoundFX),
        new(0xdc48, 5), new(0xdc4c, 5), new(0xdc50, 6),
        new(0xdc54, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Spores_SpawnEnemyDrops),
        new(0xdc56, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xdc58, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xdc02, 0xdc08, 0xdc0c, 0xdc12, 0xdc16, 0xdc1a,
        0xdc20, 0xdc24, 0xdc28, 0xdc30,
        0xdc36, 0xdc3c, 0xdc40, 0xdc44, 0xdc4a, 0xdc4e, 0xdc52,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static SporeSpawnProjectileInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.SporeSpawnStalk or
        RoomEnemyProjectileKind.SporeSpawnSpore or
        RoomEnemyProjectileKind.SporeSpawnSpawner;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            SporeSpawnProjectileInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address) return candidate.Value;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }
        throw new InvalidDataException(
            $"Spore Spawn projectile mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        foreach (SporeSpawnProjectileInstructionMechanicsWord word in Words)
        {
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
