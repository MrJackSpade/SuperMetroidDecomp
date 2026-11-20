using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;

/// <summary>Checks later-form Mother Brain contact and projectile blocking.</summary>
internal static class MotherBrainLaterContactVerification
{
    /// <summary>Checks body and neck knockback, side immunity, post-dispatch position, and beam or missile duds.</summary>
    internal static int VerifyMotherBrainLaterContact()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        var body = enemies.Places.InUpdateOrder[0];
        body.Bind(EnemyDefinitionId.MotherBrainBody, body.KeptDefinition);
        body.Bind(body.HeldOccupant().DefinitionId, RoomEnemyDefinitionLookup.Get(body.HeldOccupant().DefinitionId));
        var state = new MotherBrainEnemyState(body.HeldOccupant(), enemies.Places.InUpdateOrder[1], new MotherBrainWalkCounter(), new MotherBrainBurstCycle(), new MotherBrainBrainAnimation()) { Form = MotherBrainForm.SecondPhase, HitboxesEnabled = MotherBrainHitboxParts.Body | MotherBrainHitboxParts.Brain | MotherBrainHitboxParts.Neck };
        state.Head.Position = state.Head.Position.WithWhole(new(500, 500));
        state.Neck.Lower[1].Position = new(250, 80);
        state.Neck.Lower[2].Position = state.Neck.Upper[0].Position = new(500, 500);
        enemies.MotherBrainEncounter.BeginAsSetUp(state);
        Action<RoomEnemyPlace, SamusState, SamusBombProjectileSystem> main = (body, samus, bombs) => enemies.MotherBrainBehavior.RunMotherBrainBodyMain(body, state, samus, bombs);
        foreach (string contact in new[] { "body", "neck", "no-damage side", "post-dispatch position" })
        {
            body.Position = body.Position.WithWhole(new(128, 128));
            state.AscentPhase = MotherBrainAscentPhase.ContinuePausing;
            state.PhaseTimer.LastFor(contact == "post-dispatch position" ? 1 : 101);
            var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(158, 128)), Health = 500 };
            samus.Kinematics.Radius = new(8, 16);
            ushort expectedX = 20;
            if (contact == "neck") { samus.Position = samus.Position.WithWhole(new(250, 80)); expectedX = 16; }
            if (contact == "no-damage side") { samus.Position = samus.Position.WithWholeX(152); expectedX = 26; }
            if (contact == "post-dispatch position") { samus.Position = samus.Position.WithWhole(new(89, 279)); }
            main(body, samus, SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.CreateBombSystem());
            AssertEqual((ushort)(contact == "no-damage side" ? 500 : 500 - body.Definition.Damage),
                samus.Health, $"Later-phase {contact} applies native contact damage");
            AssertEqual(expectedX, samus.Kinematics.ExtraDisplacement.X.WholePixels, $"{contact} horizontal displacement");
            AssertEqual(4, samus.Kinematics.ExtraDisplacement.Y.WholePixels, $"{contact} vertical displacement");
            AssertEqual((ushort)96, samus.InvincibilityTimer.RawValue, "Native contact invincibility timer");
            AssertEqual(5, samus.KnockbackHold.UpdatesLeft, "Native contact knockback timer");
        }
        // Isolate the actual body callback in the ordinary radius collision pass.
        // Shot blocking is native behavior and must remain a dud, not body damage.
        body.Position = body.Position.WithWhole(new(128, 128));
        body.Radius = new(16, 16);
        body.Frame = EnemyFrameId.MotherBrainBodyStanding;
        body.Health = 1000;
        var interactive = (List<RoomEnemyOccupant>)enemies.ProcessingWindow.InteractiveEnemies;
        interactive.Add(body.HeldOccupant());
        foreach (SamusProjectileType type in new[] { SamusProjectileType.Beam(SamusBeamCombination.Power, charged: false), SamusProjectileType.Of(SamusProjectileFamily.Missile) })
        {
            var shots = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(), new global::SuperMetroid.Core.Fx.RoomEarthquake());
            var shot = shots.Shots[0];
            shot.Occupy(type, 100); shot.Direction = SamusProjectileDirection.Right;
            shot.Position = shot.Position.WithWhole(new(128, 128)); shot.Radius = new(4, 4);
            shot.Animation = ProjectileFixturePlaybacks.Active();
            shot.Behavior = type.Family == SamusProjectileFamily.Missile ? SamusShotBehaviors.Missile : SamusShotBehaviors.Beam;
            enemies.ResolveShotsOnInteractiveEnemies(shots, new SamusBombProjectileSystem(GameplayAudioRequests.Standalone(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()), SuperMetroid.Core.Progression.AreaId.Crateria, new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()));
            AssertEqual(1000, body.Health, "Body projectile callback retains body health");
            AssertTrue(shot.IsSpent, "Body blocks the beam or missile");
            AssertEqual((SoundEffectId?)SamusSound.DudShot, enemies.Audio.LatestSound, "Body emits native dud sound");
        }
        AssertEqual(2, enemies.RoomEffectActors.Count(slot => slot.Kind == RoomEffectActorKind.DudShot),
            "Both beam and missile callbacks create their dud sprite");
        Console.WriteLine("Mother Brain later contact: body/neck damage, side condition, post-dispatch position and projectile duds passed.");
        return 0;
    }

    /// <summary>Runs the focused later-form contact and dud check.</summary>
    [VerificationCommand("--mother-brain-later-contact")]
    internal static int MotherBrainLaterContactCommand()
    {
        return VerifyMotherBrainLaterContact();
    }
}
