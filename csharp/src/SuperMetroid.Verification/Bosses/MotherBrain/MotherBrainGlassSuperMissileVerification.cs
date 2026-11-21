using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
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
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        int glassHits = 0;
        enemies.VerificationServices().HeadShotOnGlassCounted = () => glassHits++;
        var head = enemies.Places.InUpdateOrder[1];
        head.Bind(EnemyDefinitionId.MotherBrainHead, head.KeptDefinition);
        head.Bind(head.HeldOccupant().DefinitionId, RoomEnemyDefinitionLookup.Get(head.HeldOccupant().DefinitionId));
        head.Position = head.Position.WithWhole(new(128, 128));
        head.Radius = new(16, 16);
        head.Frame = EnemyFrameId.MotherBrainBrainHitArea;
        head.Health = 3000;
        var state = new MotherBrainEnemyState(enemies.Places.InUpdateOrder[0].BoundFor(EnemyDefinitionId.MotherBrainBody), head, new MotherBrainWalkCounter(), new MotherBrainBurstCycle(), new MotherBrainBrainAnimation(), RoomWarningFixtures.Zebes()) { Form = MotherBrainForm.Glass };
        enemies.MotherBrainEncounter.BeginAsSetUp(state, head);
        ((List<RoomEnemyOccupant>)enemies.ProcessingWindow.InteractiveEnemies).Add(head.HeldOccupant());
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
        var shots = InstalledFixtures.CreateProjectileSystem();
        var bombs = new SamusBombProjectileSystem(GameplayAudioRequests.Standalone(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
        // A fired Super Missile occupies two slots at one position, as on the movie's frame:
        // the missile and its zero-radius link, with the instruction lists the port fires.
        MotherBrainGlassSuperMissilePair[] pair =
            [new MotherBrainGlassSuperMissilePair(0x9f6b, 8, IsLink: false), new MotherBrainGlassSuperMissilePair(0x9f7b, 0, IsLink: true)];
        for (int slot = 0; slot < pair.Length; slot++)
        {
            var shot = shots.Shots[slot];
            SamusProjectileOccupant held = shot.Occupy(SamusProjectileType.Of(SamusProjectileFamily.SuperMissile), 300); held.Direction = SamusProjectileDirection.Left;
            held.Position = shot.Position.WithWhole(new(128, 128)); held.Radius = new(pair[slot].Radius, pair[slot].Radius);
            held.Animation = SamusProjectileProgramAddresses.PlayerAt(pair[slot].List);
            // The owner names its link's place, the link the place every link names, as firing installs them.
            if (pair[slot].IsLink)
                held.Install(SamusShotBehaviors.SuperMissileLink, new SuperMissileShotState(new SamusMissileIgnition.Pending(), SuperMetroid.Core.Compatibility.UnlinkedSuperMissileLink.Place));
            else
                held.Install(SamusShotBehaviors.SuperMissile, new SuperMissileShotState(new SamusMissileIgnition.Pending(), linkPlace: 1));
        }

        enemies.ResolveShotsOnInteractiveEnemies(shots, bombs, SuperMetroid.Core.Progression.AreaId.Crateria, samus);
        AssertEqual(2700, head.Health, "one Super Missile deals 300 to the glass");
        AssertEqual(1, glassHits, "the hit advances the glass PLM argument once");
        AssertTrue(enemies.Audio.SoundRequests.Any(request => request.SoundEffect == EnemySound.ShotMotherBrain),
            "the hit queues library-two $6E");
        AssertTrue(shots.Shots[0].IsSpent, "the walker marks the first slot");
        AssertTrue(!shots.Shots[1].IsSpent, "the pair's second slot is not marked");

        var level = CreateEmptyRoom(new(16, 16));
        shots.HandleShots(SamusTerrain.InRoom(level, SamusRoomObjectFixtures.Uninstalled), samus, Position16<RoomPixels>.Zero, bombs, new SharedGameState(new SharedRandom()));
        head.AiStates = EnemyAiStates.None;
        head.HurtFlash.End();
        head.Invincibility.End();
        enemies.ResolveShotsOnInteractiveEnemies(shots, bombs, SuperMetroid.Core.Progression.AreaId.Crateria, samus);
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
