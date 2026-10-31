using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;

/// <summary>Reproduces a Super Missile pair striking Mother Brain's first-form glass.</summary>
internal static class MotherBrainGlassSuperMissileVerification
{
    /// <summary>
    /// Confirms the bank A0 walker marks the colliding missile, then $A9:B507
    /// advances glass, queues library-two $6E, and deals one 300-point hit.
    /// The next projectile pass removes the marked missile and link together.
    /// </summary>
    internal static void VerifyMotherBrainGlassSuperMissile()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        int glassHits = 0;
        enemies.VerificationServices().IncrementMotherBrainGlassRoomArgument = () => glassHits++;
        var head = enemies.SlotTable.Slots[1];
        head.Bind(EnemyDefinitionId.MotherBrainHead, head.Definition);
        head.Bind(head.DefinitionId, RoomEnemyHeaders.Get(head.DefinitionId));
        head.Position = head.Position.WithWhole(new(128, 128));
        head.Radius = new(16, 16);
        head.Frame = EnemyFrameId.MotherBrainBodyInitialDummy;
        head.Health = 3000;
        var state = new MotherBrainEnemyState(enemies.SlotTable.Slots[0]) { Head = head, Form = MotherBrainForm.Glass };
        enemies.MotherBrainEncounter.MotherBrain = state;
        ((List<RoomEnemySlot>)enemies.ProcessingWindow.InteractiveEnemies).Add(head);
        var samus = new SamusState(InstalledFixtures.AnimationContinuation);
        string root = Path.GetFullPath("out/workbook-investigation/crocomire-install");
        var installation = GameAssetInstaller.EnsureInstalled(root) ??
            GameAssetInstaller.Install(Path.GetFullPath("Super Metroid.smc"), root);
        var shots = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads) { Animation = { FrameBindings = installation.LoadProjectiles().FrameBindings } };
        var bombs = new SamusBombProjectileSystem();
        // A fired Super Missile occupies two slots at one position, as on the movie's frame:
        // the missile and its zero-radius link, with the instruction lists the port fires.
        MotherBrainGlassSuperMissilePair[] pair =
            [new MotherBrainGlassSuperMissilePair(0x9f6b, 8, SamusProjectilePreInstruction.SuperMissile), new MotherBrainGlassSuperMissilePair(0x9f7b, 0, SamusProjectilePreInstruction.SuperMissileLink)];
        for (int slot = 0; slot < pair.Length; slot++)
        {
            var shot = shots.Slots[slot];
            shot.Type = SamusProjectileType.Of(SamusProjectileFamily.SuperMissile); shot.Damage = 300; shot.Direction = SamusProjectileDirection.Left;
            shot.Position = shot.Position.WithWhole(new(128, 128)); shot.Radius = new(pair[slot].Radius, pair[slot].Radius);
            shot.Cursor = SamusProjectileProgramAddresses.CursorAt(pair[slot].List); shot.InstructionTimer = WrappingCountdown.Start(1);
            shot.PreInstruction = pair[slot].Pre;
        }

        AssertEqual(1, enemies.Collision.ResolveOrdinaryProjectileHits(shots, bombs, samus), "the glass accepts one projectile per pass");
        AssertEqual(2700, head.Health, "one Super Missile deals 300 to the glass");
        AssertEqual(1, glassHits, "the hit advances the glass PLM argument once");
        AssertTrue(enemies.Audio.SoundRequests.Any(request => request.SoundEffect == Library2Sound.ShotMotherBrain),
            "the hit queues library-two $6E");
        AssertTrue(shots.Slots[0].IsSpent, "the walker marks the first slot");
        AssertTrue(!shots.Slots[1].IsSpent, "the pair's second slot is not marked");

        var level = CreateEmptyRoom(new(16, 16));
        shots.StepFrame(new SamusTerrain { Level = level }, samus, new ProjectileButtonInput { ControllerInput = 0, ControllerNewInput = 0 }, Position16<RoomPixels>.Zero, bombs, projectileProducerEnabled: false);
        head.AiStates = EnemyAiStates.None;
        head.FlashTimer = WrappingCountdown.Start(0);
        head.InvincibilityTimer = WrappingCountdown.Start(0);
        AssertEqual(0, enemies.Collision.ResolveOrdinaryProjectileHits(shots, bombs, samus), "nothing of the pair is left to hit the glass");
        AssertEqual(2700, head.Health, "the glass takes no second hit");
        Console.WriteLine("Mother Brain glass Super Missile: one marked hit, $6E, and the pair removed on the next pass.");
    }

    /// <summary>Runs the focused single-hit glass and projectile-pair regression.</summary>
    [VerificationCommand("--mother-brain-glass-super-missile")]
    internal static int MotherBrainGlassSuperMissileCommand()
    {
        VerifyMotherBrainGlassSuperMissile();
        return 0;
    }
}
