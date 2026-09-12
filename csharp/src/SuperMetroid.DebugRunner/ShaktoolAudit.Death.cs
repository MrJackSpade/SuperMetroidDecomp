using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class ShaktoolAudit
{
    private static void VerifyDeathClearOrdering(SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room, CartridgeRoomAssets assets)
    {
        foreach (int endpoint in new[] { 0, 6 })
        foreach (bool useBomb in new[] { false, true })
        {
            var enemies = CreateEncounter(bus, room, assets, out var samus);
            enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
            var group = GetGroup(enemies);
            var victim = group[endpoint];
            ushort deathX = victim.XPosition, deathY = victim.YPosition;
            var shots = new SamusProjectileSystem();
            var bombs = new SamusBombProjectileSystem();
            int hits;
            if (useBomb)
            {
                EnemyProjectileAuditAssertions.ArmExplodingNormalBomb(bombs, deathX, deathY, damage: 1000);
                hits = enemies.ResolveOrdinaryBombHits(bombs, shots, samus, victim.NativeIndex);
            }
            else
            {
                var shot = shots.Slots[0];
                shot.Type = 0;
                shot.Damage = 1000;
                shot.Direction = (ushort)SamusProjectileDirection.Right;
                shot.XPosition = deathX;
                shot.YPosition = deathY;
                shot.XRadius = victim.XRadius;
                shot.YRadius = victim.YRadius;
                shot.InstructionPointer = 0x9000;
                shot.InstructionTimer = 1;
                hits = enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, samus, victim.NativeIndex);
            }
            if (hits != 1 || enemies.EnemiesKilled != 1 || victim.EnemyDefinitionPointer != 0 ||
                victim.Health != 0 || victim.XPosition != 0 || victim.YPosition != 0 || victim.VariableE != 0 ||
                group.Any(s => s.Properties != (ushort)EnemyProperties.Deleted) ||
                group.Where(s => s != victim).Any(s => s.EnemyDefinitionPointer != Definition))
                throw new InvalidDataException($"Shaktool death-clear mismatch: end={endpoint}, bomb={useBomb}, hits={hits}, kills={enemies.EnemiesKilled}.");
            var effects = enemies.EnemyProjectiles.Where(p => p.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion).ToArray();
            if (effects.Length != 1 || effects[0].EnemyHeaderPointer != Definition ||
                effects[0].KilledEnemyNativeIndex != victim.NativeIndex ||
                effects[0].XPosition != deathX || effects[0].YPosition != deathY || effects[0].InstructionTimer != 1)
                throw new InvalidDataException("Shaktool death effect lost pre-clear identity or position.");
        }

        // Faithful constructed nonzero-root case: common death clears VariableE.
        // $AA:DF40 subsequently reads ZERO rather than retaining the old root.
        // The cartridge bug deletes the first seven slots, even unrelated actors.
        var shifted = CreateEncounter(bus, room, assets, out _);
        var displacedVictim = shifted.Slots[10];
        displacedVictim.EnemyDefinitionPointer = Definition;
        displacedVictim.XPosition = 200;
        displacedVictim.YPosition = 150;
        displacedVictim.VariableE = 8 * RoomEnemySystem.NativeSlotSize;
        displacedVictim.Health = 0;
        shifted.StartGenericEnemyDeath(displacedVictim, deathAnimation: 0);
        ushort[] propertiesBeforeTail = shifted.Slots.Select(s => s.Properties).ToArray();
        var tail = typeof(RoomEnemySystem).GetMethod("ResolveShaktoolShotAfterCommon",
            BindingFlags.NonPublic | BindingFlags.Instance)!.CreateDelegate<Action<RoomEnemySlot>>(shifted);
        tail(displacedVictim);
        for (int i = 0; i < shifted.Slots.Count; i++)
        {
            ushort expected = i < 7 ? (ushort)EnemyProperties.Deleted : propertiesBeforeTail[i];
            if (shifted.Slots[i].Properties != expected)
                throw new InvalidDataException($"Shaktool post-clear root mismatch at slot {i}.");
        }
        if (shifted.EnemiesKilled != 1 || displacedVictim.VariableE != 0)
            throw new InvalidDataException("Shaktool post-clear tail changed common death publication.");
        Console.WriteLine("Shaktool death ordering: both endpoints by beam/bomb retain one death effect and delete seven slots; nonzero-root cartridge quirk preserved.");
    }
}
