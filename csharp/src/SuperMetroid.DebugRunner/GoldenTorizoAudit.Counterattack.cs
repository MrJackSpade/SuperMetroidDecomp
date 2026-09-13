using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class GoldenTorizoAudit
{
    /// <summary>Checks the caught-Super branch and its flash/animation-lock predecessors at $AA:D667.</summary>
    private static void VerifyCaughtSuperCounterattack(SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room, CartridgeRoomAssets assets)
    {
        foreach (ushort callback in new[] { NormalBodyShotCallback, StandUpSitDownShotCallback })
        foreach (ushort guard in new ushort[] { 0, 1 })
        foreach (ushort flash in new ushort[] { 0, 1 })
        {
            var loaded = Load(bus, room, assets, false, () => { });
            if (!AdvanceToShotCallback(bus, loaded, assets.LevelData, callback, out ushort x, out ushort y))
                throw new InvalidDataException("Counterattack fixture never reached its retail hitbox.");
            loaded.Head.FlashTimer = flash;
            loaded.State.ShotGuard = guard;
            loaded.State.CapturedProjectileFamily = 0x5555;
            loaded.Head.Parameter2 = (ushort)((loaded.Head.Parameter2 & 0xcfff) | 0x1000);
            ushort health = loaded.Head.Health;
            var shots = new SamusProjectileSystem();
            ArmProjectile(shots.Slots[0], loaded.Head, 0x8200, 300);
            shots.Slots[0].XPosition = x;
            shots.Slots[0].YPosition = y;
            shots.Slots[0].XRadius = shots.Slots[0].YRadius = 1;
            int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(bus, shots,
                new SamusBombProjectileSystem(), loaded.Samus);
            bool expectedStun = callback == NormalBodyShotCallback && guard == 0 && flash == 0;
            bool expectedDamage = flash == 0 && (callback == NormalBodyShotCallback || guard == 0);
            if (hits != 1 || ((loaded.Head.Parameter2 & 0x2000) != 0) != expectedStun ||
                (loaded.Head.Health < health) != expectedDamage || loaded.State.CapturedProjectileFamily != 0x5555)
                throw new InvalidDataException($"Caught-Super hit callback={callback:X4}, guard={guard}, flash={flash}: " +
                    $"hits={hits}, flags={loaded.Head.Parameter2:X4}, expected stun={expectedStun}, health={health}->{loaded.Head.Health}.");
        }
        Console.WriteLine("Golden Torizo caught-Super branch: eight hitbox/guard/flash cases passed.");
    }
}
