using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    // #1269: Mother Brain's body collides through its extended frames' hitbox lists
    // ($A0:9B7F), and each body rectangle's shot callback $A9:B503 turns the projectile into
    // a dud. The port tested a header radius instead, so in the 100% movie a Hyper Beam that
    // native stopped on the body's arm (frame $A174, list $A504) flew on and hit the head.
    private static void VerifyMotherBrainBodyHitboxes()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");
        RoomEnemySlot body = state.Body;
        RoomEnemySlot head = state.Head ?? throw new InvalidOperationException("Mother Brain has no head enemy.");
        AssertTrue(body.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap),
            "the body is an extended-spritemap enemy");

        // The movie's frame: walking body at ($3E,$90) on root $A174 with phase-three
        // properties $3800 (tangible); the head well clear.
        state.Form = 4;
        body.Properties = 0x3800;
        body.XPosition = 0x003e;
        body.YPosition = 0x0090;
        body.SpritemapPointer = 0xa174;
        head.XPosition = 0x0058;
        head.YPosition = 0x0021;
        head.Health = 0x8020;
        runtime.Enemies.PrepareEnemyProcessingList(0x0000, 0x0000);
        var shot = runtime.Projectiles.Slots[0];
        shot.Type = 0x9018; shot.Damage = 1000; shot.Direction = 7;
        shot.XPosition = 0x008e; shot.YPosition = 0x0061; shot.XRadius = 0x1c; shot.YRadius = 8;
        shot.InstructionPointer = 0x9000; shot.InstructionTimer = 1;
        runtime.Enemies.ResolveOrdinaryProjectileHits(bus, runtime.Projectiles, runtime.BombProjectiles, runtime.Samus!);
        AssertTrue((shot.Direction & 0x0010) != 0, "the beam stops on the body's arm rectangle");
        AssertEqual((ushort)0x8020, head.Health, "a body hit is a dud and never damages the head");

        // Lowered clear of that rectangle (and above the torso's), the same beam passes.
        shot.Direction = 7;
        shot.YPosition = 0x0080;
        runtime.Enemies.ResolveOrdinaryProjectileHits(bus, runtime.Projectiles, runtime.BombProjectiles, runtime.Samus!);
        AssertTrue((shot.Direction & 0x0010) == 0, "a beam clear of every body rectangle passes");
        Console.WriteLine("Mother Brain body hitboxes: shots resolve against the body's extended frame rectangles.");
    }
}
