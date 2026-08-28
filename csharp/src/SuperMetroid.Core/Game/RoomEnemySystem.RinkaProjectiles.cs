namespace SuperMetroid.Core.Game;

/// <summary>
/// Rinka-owned uses of the shared bank-$86 projectile pool: the room-graphics dust emitted
/// by special Mother Brain Rinkas and variant-zero generic enemy death explosions.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort MiscDustInstructionPointerTable = 0xe42c;
    private const ushort RinkaDustAnimationIndex = 3;
    private const ushort RinkaDeathAnimationIndex = 0;
    private const ushort EnemyDeathInstructionPointerTable = 0xefd5;
    private const ushort EnemyDeathNoDropTail = 0xeca3;

    /// <summary>Allocates <c>SpawnEprojWithRoomGfx($E509, 3)</c>.</summary>
    private void SpawnRinkaDustExplosion(ushort xPosition, ushort yPosition)
    {
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
        // selects $E138, the seven-frame yellow/orange burst used by special Rinkas.
        projectile.InstructionPointer = ReadWord(
            _bus!,
            0x860000 | unchecked((ushort)(
                MiscDustInstructionPointerTable + RinkaDustAnimationIndex * 2)));
        projectile.InstructionTimer = 1;
    }

    /// <summary>Ports <c>RinkasDeathAnimation(0)</c> at $A0:A410.</summary>
    private void RunRinkaDeathAnimation(RoomEnemySlot slot)
    {
        bool respawns = slot.Properties.HasAny(EnemyProperties.RespawnIfKilled);
        RoomEnemySpawnSnapshot survivingSpawnSnapshot = slot.Spawn;

        // SpawnEprojWithGfx copies the dying actor's combined room tile/palette index before
        // the common enemy record is cleared. Pool exhaustion genuinely loses both the
        // visual effect and the only future respawn instruction, just as on the cartridge.
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is not null)
        {
            InitializeEnemyProjectileFromDefinition(
                projectile,
                RoomEnemyProjectileKind.EnemyDeathExplosion,
                unchecked((ushort)(slot.VramTilesIndex | slot.PaletteIndex)));
            projectile.XPosition = slot.XPosition;
            projectile.YPosition = slot.YPosition;
            projectile.KilledEnemyNativeIndex = respawns
                ? unchecked((ushort)(slot.NativeIndex | 0x8000))
                : slot.NativeIndex;
            projectile.InstructionPointer = ReadWord(
                _bus!,
                0x860000 | unchecked((ushort)(
                    EnemyDeathInstructionPointerTable + RinkaDeathAnimationIndex * 2)));
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
            slot.EnemyDefinitionPointer = 0xdaff;
            slot.AiBank = 0xa3;
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

        RoomEnemyDefinition definition = ReadDefinition(
            _bus!,
            spawn.Population.DefinitionPointer);
        InitializeSlotFromDefinition(slot, spawn.Population, definition);
        RunInitializationAi(slot);

        // InitializeEnemies and RespawnEnemy both enter the family initializer, then leave
        // instruction-driven actors on the canonical empty map until their first timed frame.
        slot.SpritemapPointer = slot.Properties.HasAny(EnemyProperties.ProcessInstructions)
            ? slot.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap)
                ? (ushort)0x804f
                : (ushort)0x804d
            : (ushort)0;
    }

    /// <summary>Runs $86:E4FE's strict 256x256 camera cull for dust/explosion actors.</summary>
    private static void CullMiscDustOutsideCamera(
        RoomEnemyProjectileSlot projectile,
        ushort cameraX,
        ushort cameraY)
    {
        if (unchecked((short)(projectile.XPosition - cameraX)) < 0 ||
            unchecked((short)(cameraX + 256 - projectile.XPosition)) < 0 ||
            unchecked((short)(projectile.YPosition - cameraY)) < 0 ||
            unchecked((short)(cameraY + 256 - projectile.YPosition)) < 0)
        {
            projectile.Clear();
        }
    }

    /// <summary>
    /// Handles $86:EEAF for the translated Rinka death subset. Item-pickup actors are not
    /// yet owned by RoomEnemySystem; retain the cartridge's no-pickup continuation rather
    /// than fabricating ammo restoration. The exact death frames, delay, EF10 respawn, and
    /// shared-slot competition remain real ROM behavior, and this seam can be replaced by
    /// the generic pickup family without changing Rinka AI.
    /// </summary>
    private static void ContinueRinkaDeathWithoutPickup(
        RoomEnemyProjectileSlot projectile)
    {
        projectile.InstructionPointer = EnemyDeathNoDropTail;
        projectile.InstructionTimer = 1;
        projectile.PreInstruction = 0xefdf;
        projectile.CanDamageSamus = false;
        projectile.PersistsOnSamusContact = false;
        projectile.BlocksSamusProjectiles = false;
    }
}
