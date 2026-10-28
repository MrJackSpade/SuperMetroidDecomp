using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a missile hits all overlapping Beetoms before deferred projectile removal.</summary>
internal static class OverlappingEnemyShotsVerification
{
    /// <summary>Runs the real Beetom hit callback against a three-enemy stack, then confirms the next projectile pass removes the spent shot.</summary>
    internal static void VerifyOverlappingEnemyShots()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().SeedRandom = _ => { };
        var interactive = (List<int>)enemies.ProcessingWindow.InteractiveEnemyIndexes;
        Action<RoomEnemySlot, SamusState, ControllerButton> initialize = enemies.BeetomBehavior.InitializeBeetom;
        var samus = new SamusState { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(64, 128)) };
        for (int index = 0; index < 3; index++)
        {
            var enemy = enemies.SlotTable.Slots[index];
            enemy.Bind(EnemyDefinitionId.Beetom, enemy.Definition);
            enemy.Bind(enemy.DefinitionId, RoomEnemyDefinitions.Get(enemy.DefinitionId));
            enemy.Position = enemy.Position.WithWhole(new(128, 128));
            enemy.Radius = new(enemy.Definition.Radius.X, enemy.Definition.Radius.Y);
            // Keep the real Beetom callback and vulnerability, but enough health to
            // observe damage independently from the death actor and drop machinery.
            enemy.Health = 180;
            initialize(enemy, samus, 0);
            enemy.SpritemapPointer = 1;
            interactive.Add(enemy.EnemyIndex);
        }
        var shots = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None);
        var bombs = new SamusBombProjectileSystem();
        var shot = shots.Slots[0];
        shot.Type = 0x8100;
        shot.Damage = 100;
        shot.Direction = 2;
        shot.Position = shot.Position.WithWhole(new(128, 128));
        shot.Radius = new(4, 4);
        shot.Cursor = ProjectileFixtureCursors.Active;
        shot.InstructionTimer = WrappingCountdown.Start(1);
        shot.PreInstruction = SamusProjectilePreInstruction.Missile;

        AssertEqual(3, enemies.Collision.ResolveOrdinaryProjectileHits(shots, bombs, samus),
            "one missile reaches all three overlapping Beetoms in the same pass");
        foreach (var enemy in enemies.SlotTable.Slots.Take(3))
            AssertEqual(80, enemy.Health, "each overlapping Beetom takes native missile damage");
        AssertTrue(shot.PackedDirection.HasLowByteLifecycleState, "non-piercing shot is marked for deferred removal");
        AssertEqual(0x8100, shot.Type, "collision preserves payload until projectile processing");
        var level = CreateRoom(new(16, 16), new ushort[256], new byte[256]);
        shots.StepFrame(new SamusTerrain { Level = level }, samus, new ProjectileButtonInput { ControllerInput = 0, ControllerNewInput = 0 }, Position16<RoomPixels>.Zero, bombs, projectileProducerEnabled: false);
        AssertEqual(0, shot.Type, "next projectile pass consumes the removal marker");
        foreach (var enemy in enemies.SlotTable.Slots.Take(3))
        {
            enemy.AiStates = EnemyAiStates.None;
            enemy.FlashTimer = WrappingCountdown.Start(0);
            enemy.InvincibilityTimer = WrappingCountdown.Start(0);
        }
        AssertEqual(0, enemies.Collision.ResolveOrdinaryProjectileHits(shots, bombs, samus),
            "removed projectile cannot damage the stack on the next pass");
        Console.WriteLine("Overlapping Beetoms: all three receive one shot's damage; deferred removal prevents a later hit.");
    }

    /// <summary>Runs the overlapping Beetom collision reproduction.</summary>
    [VerificationCommand("--overlapping-enemy-shots")]
    internal static int OverlappingEnemyShotsCommand()
    {
        Suite(nameof(VerifyOverlappingEnemyShots), () => VerifyOverlappingEnemyShots());
        return 0;
    }
}
