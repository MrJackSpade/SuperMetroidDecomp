using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Destructive interaction probes for the naturally produced Powamp spike burst. They use a
/// fresh retail death sequence so movement/disposal coverage in the main audit is not altered
/// by deliberately placing Samus and a beam on one physical projectile slot.
/// </summary>
internal static partial class PowampAudit
{
    private static void VerifySpikeInteractions(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        (LoadedPowamps loaded, RoomEnemyProjectileSlot spike) =
            SpawnFreshSpikeForInteraction(bus, room, assets, direction: 2);

        // Property $8000 is clear in definition $D298. A beam in the same native 32-pixel
        // collision cell therefore remains untouched even though the record contains a
        // defensive delete shot-list pointer.
        var shots = new SamusProjectileSystem();
        SamusProjectileSlot beam = shots.Slots[0];
        beam.Type = 0x0001;
        beam.Damage = 20;
        beam.Direction = (ushort)SamusProjectileDirection.Right;
        beam.XPosition = spike.XPosition;
        beam.YPosition = spike.YPosition;
        beam.XRadius = 4;
        beam.YRadius = 4;
        beam.InstructionPointer = 0x9000;
        beam.InstructionTimer = 1;
        int hits = loaded.Enemies.ResolveEnemyProjectileSamusProjectileHits(
            bus,
            shots,
            new SamusBombProjectileSystem());
        if (hits != 0 || beam.InstructionPointer != 0x9000 || !spike.IsActive)
        {
            throw new InvalidDataException(
                $"Powamp spike incorrectly blocked a Samus beam: hits={hits}, " +
                $"beam=${beam.InstructionPointer:X4}, live={spike.IsActive}.");
        }

        // The shared assertion proves literal 20 damage, 96 invincibility frames, five-frame
        // knockback, and deletion because property $4000 is also clear in the same record.
        EnemyProjectileAuditAssertions.VerifyNaturalSamusContact(
            bus,
            loaded.Enemies,
            loaded.Samus,
            new SamusBombProjectileSystem(),
            assets.LevelData,
            spike,
            cameraX: 0,
            cameraY: 0);
    }

    private static (LoadedPowamps Loaded, RoomEnemyProjectileSlot Spike)
        SpawnFreshSpikeForInteraction(
            SuperMetroidAddressSpace bus,
            CartridgeRoomHeader room,
            CartridgeRoomAssets assets,
            int direction)
    {
        LoadedPowamps loaded = LoadEnemies(bus, room, assets);
        RoomEnemySlot body = loaded.Enemies.Slots[1];
        StepCentered(loaded.Enemies, room, assets, loaded.Samus, body);

        var shots = new SamusProjectileSystem();
        ArmProjectile(shots.Slots[0], body, projectileType: 0x0200, damage: 1000);
        int hitCount = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            shots,
            new SamusBombProjectileSystem(),
            loaded.Samus);
        for (int frame = 0; frame < 33; frame++)
            StepCentered(loaded.Enemies, room, assets, loaded.Samus, body);

        RoomEnemyProjectileSlot? spike = loaded.Enemies.EnemyProjectiles.SingleOrDefault(
            projectile => projectile.Kind == RoomEnemyProjectileKind.PowampSpike &&
                projectile.DirectionParameter == direction);
        if (hitCount != 1 || spike is null || spike.Damage != 20 ||
            spike.XRadius != 4 || spike.YRadius != 4 || !spike.CanDamageSamus ||
            spike.PersistsOnSamusContact || spike.BlocksSamusProjectiles ||
            spike.CollisionOption != 0)
        {
            throw new InvalidDataException(
                $"Fresh Powamp death produced invalid interaction spike {direction}: " +
                $"hit={hitCount}, found={spike is not null}.");
        }

        return (loaded, spike);
    }
}
