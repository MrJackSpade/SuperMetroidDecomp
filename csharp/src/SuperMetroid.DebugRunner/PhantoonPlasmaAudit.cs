using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static class PhantoonPlasmaAudit
{
    // Isolate the real boss's extended collision adapter from movement/AI.
    // The swooping state keeps the hitbox available after taking charged damage.
    public static int Run(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var room = CartridgeRoomHeader.Load(bus, 0xcd13);
        var assets = CartridgeRoomAssets.Load(bus, room);
        var enemies = new RoomEnemySystem();
        enemies.Load(bus, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer,
            new SnesVram(), new SnesCgram(), () => 1, level: assets.LevelData,
            samus: new SamusState(), isAreaBossDefeated: () => false);
        var body = enemies.Phantoon!.Body;
        body.XPosition = body.YPosition = 128;
        body.Health = 2500;
        body.SpritemapPointer = 0xdee7;
        body.Properties = 0;
        body.ExtraProperties = 4;
        body.VariableF = (ushort)PhantoonAiFunction.Swooping;
        body.VariableE = 60;
        var shots = new SamusProjectileSystem();
        var shared = new SamusBombProjectileSystem();
        var shot = shots.Slots[0];
        shot.Type = SamusProjectileTypeWord.CreateBeam((ushort)SamusBeamFlags.Plasma, true);
        shot.Damage = 450;
        shot.XPosition = shot.YPosition = 128;
        shot.XRadius = shot.YRadius = 4;
        shot.InstructionPointer = 0x9000;
        shot.InstructionTimer = 1;
        int first = enemies.ResolvePhantoonProjectileHits(bus, shots, shared);
        if (first != 1 || body.Health != 2050 || body.InvincibilityTimer != 16)
            throw new InvalidDataException($"Phantoon Plasma: hits={first}, health={body.Health}, invincibility={body.InvincibilityTimer}; expected 1/2050/16.");
        if (enemies.ResolvePhantoonProjectileHits(bus, shots, shared) != 0 || body.Health != 2050)
            throw new InvalidDataException("Phantoon accepted another Plasma contact before invincibility expired.");
        Console.WriteLine("Phantoon Plasma damage sets the cartridge's 16-frame invincibility and rejects immediate repeat contact.");
        return 0;
    }
}
