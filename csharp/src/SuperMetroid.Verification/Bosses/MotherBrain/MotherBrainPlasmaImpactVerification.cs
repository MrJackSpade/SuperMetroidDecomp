using SuperMetroid.Verification.Samus.Projectiles;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;

/// <summary>Checks Mother Brain head damage and shot removal after a charged Plasma impact.</summary>
internal static class MotherBrainPlasmaImpactVerification
{
    /// <summary>Confirms one accepted hit, recoil, immunity, first-form restriction, and later Hyper Beam behavior.</summary>
    internal static int VerifyMotherBrainPlasmaImpact()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
        // Mother Brain is placed as her room's population places her, her head after her body, and stands in her second
        // form with her head's hit area shown and walk credit to spend.
        // Her setup reads the room's random numbers, which stand at zero here.
        enemies.VerificationServices().ReadRandom = () => 0;
        enemies.VerificationServices().NextRandom = () => 0;
        MotherBrainEnemyState state = MotherBrainBabyArrival.PlaceMotherBrain(enemies, samus);
        // Her second phase sets her brain up as her rise does, her body and brain tangible.
        MotherBrainAscent.SetupMotherBrainPhaseTwoBrain(state);
        RoomEnemyPlace head = state.Head;
        head.Contact = head.Contact.With(EnemyContact.BlocksPlasmaBeam);
        head.Position = head.Position.WithWhole(new(128, 128));
        head.Radius = new(16, 16);
        head.Frame = EnemyFrameId.MotherBrainBrainHitArea;
        head.Health = 18000;
        state.Form = MotherBrainForm.SecondPhase;
        state.WalkCounter = MotherBrainWalkCredit.Of(4096);
        ((List<RoomEnemyOccupant>)enemies.ProcessingWindow.InteractiveEnemies).Add(head.HeldOccupant());
        var shots = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(projectileFrames: InstalledFixtures.Projectiles.FrameBindings), new global::SuperMetroid.Core.Fx.RoomEarthquake());
        var bombs = new SamusBombProjectileSystem(GameplayAudioRequests.Standalone(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
        var shot = shots.Shots[0];
        SeedChargedPlasma();
        Hit();
        AssertEqual(17550, head.Health, "Native charged vulnerability applies exactly one 450-damage hit");
        // The bank-$A0 walker only marks the shot; the next projectile pass removes it (#1269).
        AssertTrue(shot.IsSpent,
            "Mother Brain plasma-blocking property marks the shot for removal");
        AssertEqual(MotherBrainWalkCredit.Of(4096 - MotherBrainWalkCredit.StepCredit), state.WalkCounter,
            "Accepted beam spends one step of walk credit");
        AssertEqual(EnemyCollisionDefinitions.PlasmaShotInvincibilityUpdates, head.Invincibility.UpdatesLeft,
            "The no-death damage tail grants Plasma's hit immunity");
        var level = CreateEmptyRoom(new(16, 16));
        for (int frame = 0; frame < 3; frame++)
        {
            shots.HandleShots(SamusTerrain.InRoom(level, SamusRoomObjectFixtures.Uninstalled), samus, Position16<RoomPixels>.Zero, bombs, new SharedGameState(new SharedRandom()));
            Hit();
            AssertEqual(17550, head.Health, "Later overlap preserves health after the one impact");
            AssertEqual(MotherBrainWalkCredit.Of(4096 - MotherBrainWalkCredit.StepCredit), state.WalkCounter,
                "Later overlap does not spend walk credit again");
        }
        // Later checks are new shots after that immunity has run out.
        head.Invincibility.End();
        state.Form = MotherBrainForm.Glass;
        SeedChargedPlasma();
        Hit();
        AssertEqual(17550, head.Health, "First form retains its missile-only damage restriction");
        head.Invincibility.End();
        state.Form = MotherBrainForm.ThirdPhase;
        MotherBrainRainbowBeamAttackSequence lateFight = state.BeginLateFight();
        lateFight.Encounter.Form = MotherBrainForm.ThirdPhase;
        // The sequence shares the encounter's walk credit (#1388); recoil needs less than the Hyper Beam's cost.
        state.WalkCounter = MotherBrainWalkCredit.Empty;
        Seed(1000);
        shot.HeldShot().Behavior = SamusShotBehaviors.HyperBeam;
        // The first case's Plasma hit left the head invincible; $A0:A17C skips it until the
        // timer expires, so this case starts once that immunity has run out.
        head.Invincibility.End();
        Hit();
        AssertEqual(16550, head.Health, "Hyper Beam retains its native charged damage selection");
        AssertEqual(MotherBrainPhase3NeckPhase.StartHyperBeamRecoil, global::SuperMetroid.Core.PrivateState.Get<global::SuperMetroid.Core.Bosses.MotherBrain.MotherBrainThirdPhaseFight, global::SuperMetroid.Core.Bosses.MotherBrain.MotherBrainPhase3NeckPhase>(lateFight.ThirdPhaseFight, "Phase3NeckPhase"),
            "Hyper Beam retains its phase-three recoil transition");
        AssertTrue(shot.IsSpent, "Hyper Beam also obeys the head collision property");
        Console.WriteLine("Mother Brain Plasma: one charged impact/damage event, no later-frame repeats, recoil and first-form restriction passed.");
        return 0;

        void Hit() => enemies.ResolveShotsOnInteractiveEnemies(shots, bombs, SuperMetroid.Core.Progression.AreaId.Crateria, samus);
        void SeedChargedPlasma() => Seed(450);
        void Seed(int damage)
        {
            // Each case fires a new shot into the place, ending the one the last case left flying.
            shot.End();
            shot.Occupy(SamusProjectileType.Beam(SamusBeamCombination.Plasma, charged: true), damage); shot.HeldShot().Direction = SamusProjectileDirection.Right;
            shot.HeldShot().Position = shot.Position.WithWhole(new(128, 128)); shot.HeldShot().Radius = new(4, 4);
            shot.HeldShot().Animation = ProjectileFixturePlaybacks.Active();
            shot.HeldShot().Behavior = SamusShotBehaviors.Beam;
        }
    }

    /// <summary>Runs the focused Mother Brain Plasma collision reproduction.</summary>
    [VerificationCommand("--mother-brain-plasma-impact")]
    internal static int MotherBrainPlasmaImpactCommand()
    {
        return VerifyMotherBrainPlasmaImpact();
    }
}
