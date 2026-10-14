using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction;
using static RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;

/// <summary>Checks projectile collision with Mother Brain's extended body rectangles.</summary>
internal static class MotherBrainBodyHitboxesVerification
{
    /// <summary>Confirms frame $A174's arm rectangle stops a Hyper Beam as a dud while a lower clear shot passes.</summary>
    /// <remarks>#1269: extended hitbox list $A504 and shot callback $A9:B503 govern the body; a header-radius check let the movie's arm hit reach the head.</remarks>
    internal static void VerifyMotherBrainBodyHitboxes()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrainEncounter.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");
        RoomEnemySlot body = state.Body;
        RoomEnemySlot head = state.Head;
        AssertTrue(body.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap),
            "the body is an extended-spritemap enemy");

        // The movie's frame: walking body at ($3E,$90) on root $A174 with phase-three
        // properties $3800 (tangible); the head well clear.
        state.Form = 4;
        body.Properties = 0x3800;
        body.Position = body.Position.WithWhole(new(0x003e, 0x0090));
        body.SpritemapPointer = 0xa174;
        head.Position = head.Position.WithWhole(new(0x0058, 0x0021));
        head.Health = 0x8020;
        runtime.Enemies.ProcessingWindow.PrepareEnemyProcessingList(new(0x0000, 0x0000));
        var shot = runtime.Projectiles.Slots[0];
        shot.Type = 0x9018; shot.Damage = 1000; shot.Direction = 7;
        shot.Position = shot.Position.WithWhole(new(0x008e, 0x0061)); shot.Radius = new(0x1c, 8);
        shot.InstructionPointer = 0x9000; shot.InstructionTimer = NativeCountdown.Start(1);
        runtime.Enemies.Collision.ResolveOrdinaryProjectileHits(bus, runtime.Projectiles, runtime.BombProjectiles, runtime.Player.Samus);
        AssertTrue((shot.Direction & 0x0010) != 0, "the beam stops on the body's arm rectangle");
        AssertEqual((ushort)0x8020, head.Health, "a body hit is a dud and never damages the head");

        // Lowered clear of that rectangle (and above the torso's), the same beam passes.
        shot.Direction = 7;
        shot.Position = shot.Position.WithWholeY(0x0080);
        runtime.Enemies.Collision.ResolveOrdinaryProjectileHits(bus, runtime.Projectiles, runtime.BombProjectiles, runtime.Player.Samus);
        AssertTrue((shot.Direction & 0x0010) == 0, "a beam clear of every body rectangle passes");
        Console.WriteLine("Mother Brain body hitboxes: shots resolve against the body's extended frame rectangles.");
    }

    /// <summary>Runs the focused Mother Brain body hitbox verification command.</summary>
    [VerificationCommand("--mother-brain-body-hitboxes")]
    internal static int MotherBrainBodyHitboxesCommand()
    {
        VerifyMotherBrainBodyHitboxes();
        return 0;
    }
}
