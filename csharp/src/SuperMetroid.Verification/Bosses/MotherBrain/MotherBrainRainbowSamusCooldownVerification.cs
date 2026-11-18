using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.World;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the projectile cooldown Mother Brain's rainbow beam gives Samus (#1697).</summary>
internal static class MotherBrainRainbowSamusCooldownVerification
{
    /// <summary>
    /// Drives the rainbow attack from its charge to its end: lowering the neck gives Samus's shared cooldown eight
    /// updates, replacing a longer one; the beam's start clears it as the beam captures her; its end gives eight again as
    /// it releases her; no other step touches it.
    /// </summary>
    internal static void VerifyMotherBrainRainbowSamusCooldown()
    {
        var cameraCheckpoint = new RainbowBeamCameraCheckpoint();
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SamusCompatibility.ForNewSamus(), new BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Health = 999,
            Position = FixedPosition<RoomPixels>.FromWhole(new(220, 124)),
        };
        var attack = MotherBrainLateFightFixtures.NewSequence();
        attack.State.MoveBrainTo(new(64, 96));
        attack.Encounter.Body.Position = FixedPosition<RoomPixels>.FromWhole(new(64, 100));
        attack.Encounter.RainbowPhase = MotherBrainRainbowPhase.WaitForCharge;
        attack.Encounter.PhaseTimer.LastFor(1);
        RainbowBeamAttackFrame Frame() => MotherBrainLateFightFixtures.QuietFrame(cameraCheckpoint, 0, 0);

        MotherBrainRainbowBeamAttackStepResult neckLowered = attack.Step(samus, Frame());
        AssertEqual(MotherBrainRainbowPhase.StartFiring, attack.Encounter.RainbowPhase,
            "the charge's end lowers the neck and runs on into the start of firing");
        AssertEqual<int?>(MotherBrainBeamDefinitions.NeckLoweredSamusCooldown, neckLowered.SamusProjectileCooldown,
            "lowering the neck gives Samus the beam's cooldown");

        // The live encounter gives the requested cooldown to Samus's shared one, replacing a longer cooldown.
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        body.Bind(EnemyDefinitionId.MotherBrainBody, body.Definition);
        var state = new MotherBrainEnemyState(body, enemies.Places.InUpdateOrder[1], new MotherBrainWalkCounter(), new MotherBrainBurstCycle(), new MotherBrainBrainAnimation()) { Form = MotherBrainForm.SecondPhase };
        enemies.MotherBrainEncounter.BeginAsSetUp(state);
        var bombs = new SamusBombProjectileSystem(GameplayAudioRequests.Standalone(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps.PowerBombGlowColors, global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
        bombs.SetSharedCooldown(20);
        enemies.MotherBrainBehavior.RainbowAndDeath.ApplyLiveMotherBrainRainbowState(state, attack, neckLowered, samus, bombs);
        AssertEqual(MotherBrainBeamDefinitions.NeckLoweredSamusCooldown, bombs.CooldownTimer,
            "the encounter replaces Samus's longer cooldown with the beam's");

        bool started = false;
        while ((attack.Encounter.Stage == MotherBrainBodyStage.RainbowBeam && attack.Encounter.RainbowPhase == MotherBrainRainbowPhase.StartFiring))
        {
            MotherBrainRainbowBeamAttackStepResult charging = attack.Step(samus, Frame());
            if ((attack.Encounter.Stage == MotherBrainBodyStage.RainbowBeam && attack.Encounter.RainbowPhase == MotherBrainRainbowPhase.StartFiring))
                AssertEqual<int?>(null, charging.SamusProjectileCooldown, "the widening beam gives Samus no cooldown");
            else
            {
                started = true;
                AssertEqual<int?>(MotherBrainBeamDefinitions.BeamStartedSamusCooldown, charging.SamusProjectileCooldown,
                    "the beam's start clears Samus's cooldown");
            }
        }
        AssertTrue(started && attack.Encounter.RainbowPhase == MotherBrainRainbowPhase.MoveSamusTowardWall,
            "the start of firing captures Samus");
        AssertTrue(samus.Control.InputLocked, "the beam's start locks Samus");

        MotherBrainRainbowBeamAttackStepResult? ended = null;
        while ((attack.Encounter.Stage != MotherBrainBodyStage.RainbowBeam || attack.Encounter.RainbowPhase != MotherBrainRainbowPhase.LetSamusFall))
        {
            MotherBrainRainbowBeamAttackStepResult firing = attack.Step(samus, Frame());
            if ((attack.Encounter.Stage == MotherBrainBodyStage.RainbowBeam && attack.Encounter.RainbowPhase == MotherBrainRainbowPhase.LetSamusFall))
                ended = firing;
            else
                AssertEqual<int?>(null, firing.SamusProjectileCooldown, $"the held beam gives Samus no cooldown in {attack.Encounter.RainbowPhase}");
        }
        AssertTrue(!samus.Control.InputLocked, "the beam's end releases Samus");
        AssertEqual<int?>(MotherBrainBeamDefinitions.BeamEndedSamusCooldown, ended?.SamusProjectileCooldown,
            "the beam's end gives Samus its cooldown as it releases her");

        MotherBrainRainbowBeamAttackStepResult fall = attack.Step(samus, Frame());
        AssertEqual<int?>(null, fall.SamusProjectileCooldown, "letting Samus fall gives her no cooldown");
        Console.WriteLine("  Mother Brain: the rainbow beam gives Samus's cooldown as its neck lowers, as it starts and as it ends.");
    }
}
