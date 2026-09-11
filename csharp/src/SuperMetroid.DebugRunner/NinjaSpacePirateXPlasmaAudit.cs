using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class NinjaSpacePirateAudit
{
    /// <summary>
    /// Isolates the steel Pirate's vulnerable component and EnemyMain freeze boundary.
    /// The initial map and overlapping projectile are constructed: this does not claim
    /// to reproduce the player's firing trajectory or controller X-ray admission.
    /// </summary>
    public static int RunXPlasma(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var room = CartridgeRoomHeader.Load(bus, MetalPiratesRoomPointer);
        var assets = CartridgeRoomAssets.Load(bus, room);
        foreach (bool charged in new[] { false, true })
        foreach (int freezeFrames in new[] { 15, 16 })
        {
            var loaded = Load(bus, room, assets);
            var actor = KeepOnly(loaded, 0);
            PrimeActive(loaded, assets, actor);
            // Same authored vulnerable component used by the family's combat audit.
            actor.SpritemapPointer = 0x89c4;
            var shot = loaded.Projectiles.Slots[0];
            ushort type = SamusProjectileTypeWord.CreateBeam(
                (ushort)SamusBeamFlags.Plasma, charged: charged);
            ushort damage = (ushort)(charged ? 450 : 150);
            ArmProjectile(shot, (ushort)(actor.XPosition - 10),
                (ushort)(actor.YPosition - 10), type);
            shot.Damage = damage;
            if (loaded.Enemies.ResolveOrdinaryProjectileHits(
                    bus, loaded.Projectiles, loaded.SharedProjectiles, loaded.Samus) != 1 ||
                actor.Health != 1800 - damage || actor.InvincibilityTimer != 16 ||
                shot.Type != type || shot.Damage != damage)
                throw new InvalidDataException($"Steel Pirate initial Plasma: charged={charged}, HP={actor.Health}, timer={actor.InvincibilityTimer}, type={shot.Type:X4}.");

            ushort x = actor.XPosition, y = actor.YPosition, flash = actor.FlashTimer;
            for (int frame = 1; frame <= freezeFrames; frame++)
            {
                loaded.Enemies.StepFrame(0, 0, true, loaded.Samus,
                    level: assets.LevelData, samusProjectiles: loaded.Projectiles,
                    sharedProjectiles: loaded.SharedProjectiles,
                    resolveSamusContactBeforeAi: true);
                if (actor.Health != 1800 - damage || actor.InvincibilityTimer != 16 - frame ||
                    actor.XPosition != x || actor.YPosition != y || actor.FlashTimer != flash ||
                    actor.SpritemapPointer != 0x89c4 || shot.Type != type)
                    throw new InvalidDataException($"Steel Pirate frozen state changed at frame {frame}.");
            }

            // Exercise the actual pre-AI dispatch, including the entry-timer gate.
            // Expiring 1 -> 0 on release is not permission to collide in that same pass.
            loaded.Enemies.StepFrame(0, 0, false, loaded.Samus,
                level: assets.LevelData, samusProjectiles: loaded.Projectiles,
                sharedProjectiles: loaded.SharedProjectiles,
                resolveSamusContactBeforeAi: true);
            int expectedHits = freezeFrames == 16 ? 2 : 1;
            if (actor.Health != 1800 - damage * expectedHits || shot.Type != type ||
                shot.Damage != damage || actor.InvincibilityTimer != (freezeFrames == 16 ? 16 : 0))
                throw new InvalidDataException($"Steel Pirate release: charged={charged}, frozen={freezeFrames}, HP={actor.Health}, timer={actor.InvincibilityTimer}, type={shot.Type:X4}.");
            Console.WriteLine($"Steel Pirate charged={charged}, frozen={freezeFrames}: hits={expectedHits}, HP={actor.Health}, timer={actor.InvincibilityTimer}.");
        }
        return 0;
    }
}
