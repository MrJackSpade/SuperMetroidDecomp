using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class PhantoonPlasmaAudit
{
    public static int RunRelease(string rom)
    {
        foreach (ushort entryInvincibility in new ushort[] { 0, 1 })
            VerifyRelease(rom, entryInvincibility);
        Console.WriteLine("Full runtime Phantoon release matches native damage/invincibility/flash ordering.");
        return 0;
    }

    private static void VerifyRelease(string rom, ushort entryInvincibility)
    {
        var runtime = new SuperMetroidRuntime(SuperMetroidAddressSpace.LoadRetailRom(rom));
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xcd13);
        runtime.InitializeDebugGroundedSamus(64, 166, 8);
        runtime.Samus!.InputLocked = false;
        var body = runtime.Enemies.Phantoon!.Body;
        // Native v5 boundary after twenty frozen calls following a charged hit.
        body.XPosition = body.YPosition = 128;
        body.Health = 2050;
        body.InvincibilityTimer = entryInvincibility;
        body.FlashTimer = 16;
        body.AiHandlerBits = 2;
        body.SpritemapPointer = 0xdee7;
        body.Properties = 0;
        body.ExtraProperties = 4;
        body.VariableF = (ushort)PhantoonAiFunction.Swooping;
        body.VariableE = 1;
        runtime.Enemies.Phantoon.Tentacles!.VariableB = 450;
        var shot = runtime.Projectiles.Slots[0];
        shot.Type = SamusProjectileTypeWord.CreateBeam((ushort)SamusBeamFlags.Plasma, true);
        shot.Damage = 450;
        shot.XPosition = shot.YPosition = 128;
        shot.XRadius = shot.YRadius = 4;
        shot.InstructionPointer = 0x9000;
        shot.InstructionTimer = 100;
        runtime.StepFrame(0);
        ushort expectedHealth = entryInvincibility == 0 ? (ushort)1600 : (ushort)2050;
        ushort expectedInvincibility = entryInvincibility == 0 ? (ushort)16 : (ushort)0;
        if (body.Health != expectedHealth || body.InvincibilityTimer != expectedInvincibility || body.FlashTimer != 15)
            throw new InvalidDataException($"Runtime Phantoon release health/invincibility/flash: {body.Health}/{body.InvincibilityTimer}/{body.FlashTimer}, expected {expectedHealth}/{expectedInvincibility}/15 (entry timer {entryInvincibility}).");
    }

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
        // Independent original-CPU encounter frame 1565: the primer retains its
        // missile type/damage and receives direction bit $10, rather than becoming
        // a missile explosion. Its own following pre-instruction deletes it.
        body.Health = 2500;
        body.InvincibilityTimer = 0;
        shot.Type = 0x8100;
        shot.Damage = 100;
        shot.Direction = 0;
        if (enemies.ResolvePhantoonProjectileHits(bus, shots, shared) != 1 ||
            body.Health != 2400 || shot.Type != 0x8100 || shot.Damage != 100 || shot.Direction != 0x10)
            throw new InvalidDataException($"Phantoon native primer lifecycle: health={body.Health}, type={shot.Type:X4}, damage={shot.Damage}, direction={shot.Direction:X4}.");
        // A missile that explodes on room geometry can still overlap the moving
        // boss. A0:9BF3 rejects the entire family range, not just beam explosions.
        // Keep unknown upper families unnamed while testing their native rejection.
        for (int family = (ushort)SamusProjectileFamily.BeamExplosion; family <= 0x0f00; family += 0x0100)
        {
            shot.Type = (ushort)(0x8000 | family);
            shot.Direction = 0;
            ushort health = body.Health;
            if (enemies.ResolvePhantoonProjectileHits(bus, shots, shared) != 0 ||
                body.Health != health || shot.Direction != 0)
                throw new InvalidDataException($"Phantoon accepted excluded explosion family {family:X4}.");
        }
        return 0;
    }
}
