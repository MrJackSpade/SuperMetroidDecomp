namespace SuperMetroid.Core.Game;

/// <summary>
/// Spore Spawn's bank-$86 actors. The stalk, ceiling emitters, and spores all share the
/// engine's finite eighteen-slot pool and ROM instruction interpreter; only their family
/// initializers and pre-instructions belong here.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const int SporeSpawnStalkYOffsetTable = 0x86dcb9;
    private const int SporeSpawnSpawnerXTable = 0x86dce6;
    private const int SporeSpawnMovementTable = 0x86dd6c;
    private const ushort SporeSpawnSpawnerSpawnInstruction = 0xdc06;
    private const ushort SporeSpawnGraphicsIndex = 0x0200;

    /// <summary>Ports projectile definition $86:DE6C and initializer $86:DCA3.</summary>
    private void SpawnSporeSpawnStalk(RoomEnemySlot body, ushort spawnArgument)
    {
        if (spawnArgument > 3)
            throw new ArgumentOutOfRangeException(nameof(spawnArgument));
        RoomEnemyProjectileSlot? stalk = AllocateEnemyProjectile();
        if (stalk is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            stalk,
            RoomEnemyProjectileKind.SporeSpawnStalk,
            unchecked((ushort)(body.VramTilesIndex | body.PaletteIndex)));
        stalk.XPosition = body.XPosition;
        stalk.YPosition = unchecked((ushort)(
            body.YPosition +
            ReadWord(_bus!, SporeSpawnStalkYOffsetTable + spawnArgument * 2)));
    }

    /// <summary>Ports projectile definition $86:DE88 and initializer $86:DCD4.</summary>
    private void SpawnSporeSpawnSpawner(RoomEnemySlot body, ushort spawnArgument)
    {
        if (spawnArgument > 3)
            throw new ArgumentOutOfRangeException(nameof(spawnArgument));
        RoomEnemyProjectileSlot? spawner = AllocateEnemyProjectile();
        if (spawner is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            spawner,
            RoomEnemyProjectileKind.SporeSpawnSpawner,
            unchecked((ushort)(body.VramTilesIndex | body.PaletteIndex)));
        spawner.XPosition = ReadWord(
            _bus!,
            SporeSpawnSpawnerXTable + spawnArgument * 2);
        spawner.YPosition = 520;
    }

    /// <summary>Ports projectile definition $86:DE7A and initializer $86:DC8D.</summary>
    private void SpawnSporeSpawnSpore(ushort x, ushort y)
    {
        RoomEnemyProjectileSlot? spore = AllocateEnemyProjectile();
        if (spore is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            spore,
            RoomEnemyProjectileKind.SporeSpawnSpore,
            SporeSpawnGraphicsIndex);
        spore.XPosition = x;
        spore.YPosition = y;

        // Var1 retains the original X coordinate. Bit seven alone mirrors horizontal motion,
        // so emitters at $20/$A0 move one way and $60/$E0 move the other.
        spore.Variable1 = x;
    }

    /// <summary>Ports <c>PreInstruction_EnemyProjectile_Spores</c> at $86:DCEE.</summary>
    private void RunSporeSpawnSporePreInstruction(RoomEnemyProjectileSlot spore)
    {
        int movementOffset = spore.Variable0 & 0x00ff;
        int xDelta = unchecked((sbyte)_bus!.ReadByte(
            SporeSpawnMovementTable + movementOffset));
        if ((spore.Variable1 & 0x0080) != 0)
            xDelta = -xDelta;
        spore.XPosition = unchecked((ushort)(spore.XPosition + xDelta));

        int yDelta = unchecked((sbyte)_bus.ReadByte(
            SporeSpawnMovementTable + ((movementOffset + 1) & 0x00ff)));
        spore.YPosition = unchecked((ushort)(spore.YPosition + yDelta + yDelta));
        if (unchecked((short)(spore.YPosition - 768)) >= 0)
            spore.Clear();

        // The native word keeps only the wrapped low byte after each two-byte vector pair.
        spore.Variable0 = unchecked((byte)(movementOffset + 2));
    }

    /// <summary>Ports <c>PreInstruction_EnemyProjectile_SporeSpawner</c> at $86:DD46.</summary>
    private void RunSporeSpawnSpawnerPreInstruction(RoomEnemyProjectileSlot spawner)
    {
        SporeSpawnEnemyState state = _sporeSpawn ?? throw new InvalidOperationException(
            "A Spore Spawn ceiling emitter survived without its owning boss state.");
        if (state.SporeGenerationFlag != 0)
            return;

        if (spawner.Variable1 == 0)
        {
            spawner.InstructionPointer = SporeSpawnSpawnerSpawnInstruction;
            spawner.InstructionTimer = 1;
            spawner.Variable1 = unchecked((ushort)(_nextRandom!() & 0x01ff));
        }
        spawner.Variable1 = unchecked((ushort)(spawner.Variable1 - 1));
    }

    /// <summary>Ports projectile instruction $86:DC5A's complete property-word replace.</summary>
    private static void SetSporeSpawnImpactProperties(RoomEnemyProjectileSlot spore)
    {
        if (spore.Kind != RoomEnemyProjectileKind.SporeSpawnSpore)
        {
            throw new InvalidOperationException(
                $"Projectile {spore.Kind} reached Spore Spawn property opcode $DC5A.");
        }

        // Literal $3000: damage zero, high priority, Samus collision enabled, projectile
        // collision disabled, and deletion on contact. Priority is not split by this host's
        // draw queue, but every collision bit is retained explicitly.
        spore.Damage = 0;
        spore.CanDamageSamus = true;
        spore.PersistsOnSamusContact = false;
        spore.BlocksSamusProjectiles = false;
    }

    /// <summary>Ports projectile instruction $86:DC61.</summary>
    private void RequestSporeSpawnSporeDrop(RoomEnemyProjectileSlot spore)
    {
        if (spore.Kind != RoomEnemyProjectileKind.SporeSpawnSpore)
        {
            throw new InvalidOperationException(
                $"Projectile {spore.Kind} reached Spore Spawn drop opcode $DC61.");
        }
        _sporeSpawnDropRequests.Add(new SporeSpawnDropRequest(
            spore.XPosition,
            spore.YPosition));
    }
}
