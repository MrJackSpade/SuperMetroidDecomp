namespace SuperMetroid.Core.Game;

/// <summary>
/// Rinka-owned uses of the shared bank-$86 projectile pool: the room-graphics dust emitted
/// by special Mother Brain Rinkas and variant-zero generic enemy death explosions.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort RinkaDustAnimationIndex = 3;
    /// <summary>Allocates <c>SpawnEprojWithRoomGfx($E509, 3)</c>.</summary>
    private void SpawnRinkaDustExplosion(ushort xPosition, ushort yPosition)
        => SpawnRoomGraphicsDustExplosion(xPosition, yPosition, RinkaDustAnimationIndex);

    /// <summary>
    /// Allocates the shared room-graphics dust/explosion actor at <c>$86:E509</c>.
    /// The caller supplies the native animation-table index exactly as the spawn parameter;
    /// this is shared by special Rinka deaths, Boulder's impact clouds, and later families
    /// that invoke the same engine primitive.
    /// </summary>
    private void SpawnRoomGraphicsDustExplosion(
        ushort xPosition,
        ushort yPosition,
        ushort animationIndex)
    {
        ushort instructionList =
            MiscDustProjectileDefinitions.InstructionList(animationIndex);
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.MiscDustExplosion,
            graphicsIndex: 0);
        projectile.XPosition = xPosition;
        projectile.YPosition = yPosition;

        // EprojInit_DustCloudOrExplosion ignores the definition's placeholder list and
        // indexes this literal thirty-word table with the spawn parameter. Parameter three
        // selects $E138 for Rinka; Boulder passes $11 for its impact cloud.
        projectile.InstructionPointer = instructionList;
        projectile.InstructionTimer = 1;
    }

    /// <summary>Ports <c>RinkasDeathAnimation(0)</c> at $A0:A410.</summary>
    private void RunRinkaDeathAnimation(RoomEnemySlot slot)
    {
        _enemyDeathRespawnScratch = RespawnScratchWord(slot);
        bool respawns = slot.Properties.HasAny(EnemyProperties.RespawnIfKilled);
        RoomEnemySpawnSnapshot survivingSpawnSnapshot = slot.Spawn;

        // The shared spawn helper initially carries the dying enemy's graphics word, but
        // EprojInit_EnemyDeathExplosion immediately replaces eproj_gfx_idx with zero. Pool
        // exhaustion genuinely loses both the effect and the only future respawn instruction.
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is not null)
        {
            InitializeEnemyProjectileFromDefinition(
                projectile,
                RoomEnemyProjectileKind.EnemyDeathExplosion,
                graphicsIndex: 0);
            projectile.XPosition = slot.XPosition;
            projectile.YPosition = slot.YPosition;
            projectile.EnemyHeaderPointer = slot.EnemyDefinitionPointer;
            projectile.KilledEnemyNativeIndex = respawns
                ? unchecked((ushort)(slot.NativeIndex | 0x8000))
                : slot.NativeIndex;
            projectile.InstructionPointer = EnemyDeathExplosionDefinitions
                .InstructionPointer((ushort)EnemyDeathAnimation.SmallExplosion);
            projectile.InstructionTimer = 1;
        }

        int slotIndex = slot.SlotIndex;
        slot.Clear();

        // EnemySpawnData occupies a separate WRAM region and is not part of memset's
        // 64-byte target. Preserve it even for a non-respawning special Rinka so debugger
        // state remains structurally faithful after the common record disappears.
        slot.Spawn = survivingSpawnSnapshot;
        _rinkaStates[slotIndex] = null;
        if (respawns)
        {
            InstallRespawnPlaceholder(slot);
        }
    }

    /// <summary>
    /// Executes instruction $86:EF10 for the high-bit enemy index retained by a death actor.
    /// The method is deliberately population/snapshot based so later respawning families can
    /// share it without acquiring a Rinka-specific reconstruction path.
    /// </summary>
    private void RespawnEnemyFromSnapshot(ushort nativeIndex)
    {
        RoomEnemySlot slot = SlotFromNativeIndex(nativeIndex);
        RoomEnemySpawnSnapshot spawn = slot.Spawn;
        if (spawn.Population.DefinitionPointer == 0)
        {
            throw new InvalidDataException(
                $"Enemy slot ${nativeIndex:X4} has no surviving population snapshot to respawn.");
        }

        RoomEnemyDefinition definition = ResolveRoomEnemyDefinition(
            _bus!,
            spawn.Population.DefinitionPointer);
        // Unlike Initialise_Enemies ($A0:8B93) and Spawn_Enemy ($A0:93D9), Respawn_Enemy
        // ($86:F264) never writes the spritemap: the actor keeps EnemyDeath's cleared word,
        // or what its init AI wrote, until its first instruction. A zero spritemap is also
        // $A0:A08C's contact gate, so the respawned actor cannot touch Samus before then.
        InitializeSlotFromDefinition(slot, spawn.Population, definition);
        RunInitializationAi(slot);
    }

    /// <summary>Runs $86:E4FE's strict 256x256 camera cull for dust/explosion actors.</summary>
    private static void CullMiscDustOutsideCamera(
        RoomEnemyProjectileSlot projectile,
        ushort cameraX,
        ushort cameraY)
    {
        // $86:E6E0 compares the position against camera + $100 and culls on BPL, so
        // a projectile exactly $100 past the camera is already off-screen.
        if (unchecked((short)(projectile.XPosition - cameraX)) < 0 ||
            unchecked((short)(projectile.XPosition - (cameraX + 256))) >= 0 ||
            unchecked((short)(projectile.YPosition - cameraY)) < 0 ||
            unchecked((short)(projectile.YPosition - (cameraY + 256))) >= 0)
        {
            projectile.Clear();
        }
    }

}
