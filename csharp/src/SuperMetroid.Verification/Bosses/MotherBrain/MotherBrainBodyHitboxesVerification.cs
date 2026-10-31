using SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Projectiles;

/// <summary>Checks projectile collision with Mother Brain's extended body rectangles.</summary>
internal static class MotherBrainBodyHitboxesVerification
{
    /// <summary>Confirms frame $A174's arm rectangle stops a Hyper Beam as a dud while a lower clear shot passes.</summary>
    /// <remarks>#1269: extended hitbox list $A504 and shot callback $A9:B503 govern the body; a header-radius check let the movie's arm hit reach the head.</remarks>
    internal static void VerifyMotherBrainBodyHitboxes()
    {
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrainEncounter.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");
        RoomEnemySlot body = state.Body;
        RoomEnemySlot head = state.Head;
        AssertTrue(body.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap),
            "the body is an extended-spritemap enemy");

        // The movie's frame: walking body at ($3E,$90) on root $A174 with phase-three
        // properties $3800 (tangible); the head well clear.
        state.Form = MotherBrainForm.ThirdPhase;
        body.Properties = SuperMetroid.AssetExtraction.Enemies.EnemyPropertyWordCodec.Decode(0x3800);
        body.Position = body.Position.WithWhole(new(0x003e, 0x0090));
        body.SpritemapPointer = 0xa174;
        head.Position = head.Position.WithWhole(new(0x0058, 0x0021));
        head.Health = 0x8020;
        runtime.Enemies.ProcessingWindow.PrepareEnemyProcessingList(new(0x0000, 0x0000));
        var shot = runtime.Projectiles.Slots[0];
        shot.Type = SamusProjectileType.Beam(SamusBeamCombination.Plasma, charged: true); shot.Damage = 1000; shot.Direction = SamusProjectileDirection.Left;
        shot.Position = shot.Position.WithWhole(new(0x008e, 0x0061)); shot.Radius = new(0x1c, 8);
        shot.Cursor = ProjectileFixtureCursors.Active; shot.InstructionTimer = WrappingCountdown.Start(1);
        runtime.Enemies.Collision.ResolveOrdinaryProjectileHits(runtime.Projectiles, runtime.BombProjectiles, runtime.Player.Samus);
        AssertTrue(shot.IsSpent, "the beam stops on the body's arm rectangle");
        AssertEqual(0x8020, head.Health, "a body hit is a dud and never damages the head");

        // Lowered clear of that rectangle (and above the torso's), the same beam passes.
        shot.IsSpent = false;
        shot.Position = shot.Position.WithWholeY(0x0080);
        runtime.Enemies.Collision.ResolveOrdinaryProjectileHits(runtime.Projectiles, runtime.BombProjectiles, runtime.Player.Samus);
        AssertTrue(!shot.IsSpent, "a beam clear of every body rectangle passes");
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
