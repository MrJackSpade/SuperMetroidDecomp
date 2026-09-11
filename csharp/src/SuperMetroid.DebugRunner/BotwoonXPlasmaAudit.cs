using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class BotwoonAudit
{
    /// <summary>
    /// Room-local combat prerequisite for #402. Freezing is supplied at EnemyMain's
    /// boundary; this deliberately does not claim to test controller X-ray admission.
    /// </summary>
    public static int RunXPlasma(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var room = CartridgeRoomHeader.Load(bus, RoomPointer);
        var assets = CartridgeRoomAssets.Load(bus, room);
        var loaded = Load(bus, room, assets, alreadyDefeated: false);
        AdvanceUntilShootable(loaded, assets.LevelData);
        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ArmProjectile(shots.Slots[0], loaded.Head,
            type: SamusProjectileTypeWord.CreateBeam((ushort)SamusBeamFlags.Plasma, charged: true), damage: 100);
        int Hit() => loaded.Enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, loaded.Samus);
        if (Hit() != 1 || loaded.Head.Health != 2900 || loaded.Head.InvincibilityTimer != 16)
            throw new InvalidDataException($"Initial Plasma hit: health={loaded.Head.Health}, invincibility={loaded.Head.InvincibilityTimer}.");
        ushort x = loaded.Head.XPosition, y = loaded.Head.YPosition;
        ushort flash = loaded.Head.FlashTimer;
        for (int frame = 1; frame <= 14; frame++)
        {
            loaded.Enemies.StepFrame(CameraX, CameraY, timeIsFrozen: true, loaded.Samus, level: assets.LevelData);
            if (loaded.Head.InvincibilityTimer != 16 - frame || loaded.Head.Health != 2900 ||
                loaded.Head.XPosition != x || loaded.Head.YPosition != y || loaded.Head.FlashTimer != flash)
                throw new InvalidDataException($"Frozen Botwoon changed the wrong state at frame {frame}.");
        }
        // Runtime does not resolve beam hits during X-ray. The first ordinary enemy
        // pass on release republishes the interactive index list before beam collision.
        StepEnemies(loaded, assets.LevelData);
        if (loaded.Head.InvincibilityTimer != 1 || Hit() != 0 || loaded.Head.Health != 2900)
            throw new InvalidDataException("Plasma repeated its hit on the early release frame.");
        StepEnemies(loaded, assets.LevelData);
        if (Hit() != 1 || loaded.Head.Health != 2800 || loaded.Head.InvincibilityTimer != 16)
            throw new InvalidDataException($"Retained Plasma hit: health={loaded.Head.Health}, invincibility={loaded.Head.InvincibilityTimer}, direction={shots.Slots[0].Direction:X4}.");
        Console.WriteLine("Botwoon retained Plasma: health 3000->2900->2800; 15-frame rejection, 16-frame expiry, independent flash/position passed.");
        foreach (bool blocksPlasma in new[] { false, true })
        {
            ArmProjectile(shots.Slots[0], loaded.Head,
                SamusProjectileTypeWord.CreateBeam(blocksPlasma ? (ushort)SamusBeamFlags.Plasma : (ushort)0, charged: true), 100);
            if (!shots.TryStartEnemyImpact(bus, bombs, 0, blocksPlasma) ||
                shots.Slots[0].PackedType.Family != SamusProjectileFamily.BeamExplosion)
                throw new InvalidDataException("Nonpenetrating beam or Plasma-blocking target failed to consume the shot.");
        }
        Console.WriteLine("Controls: ordinary charged beam and Plasma-blocking target both produce beam impacts.");
        return 0;
    }
}
