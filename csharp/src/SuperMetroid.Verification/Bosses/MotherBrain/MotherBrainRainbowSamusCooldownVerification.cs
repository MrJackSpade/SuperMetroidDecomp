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
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SamusCompatibility.ForNewSamus(), new BeamChargeState())
        {
            Health = 999,
            Position = FixedPosition<RoomPixels>.FromWhole(new(220, 124)),
        };
        var attack = new MotherBrainRainbowBeamAttackSequence
        {
            State = { BrainPosition = new(64, 96) },
        };
        attack.State.Body.Position = new(64, 100);
        attack.State.Phase = MotherBrainBodyFunction.SecondPhaseRainbowWaitForCharge;
        attack.State.FunctionTimer = 0;
        RainbowBeamAttackFrame Frame() => new() { CameraCheckpoint = cameraCheckpoint, EnemyFrameCounter = 0, MainEnemyExecutionCounter = 0 };

        MotherBrainRainbowBeamAttackStepResult neckLowered = attack.Step(samus, Frame());
        AssertEqual(MotherBrainBodyFunction.SecondPhaseRainbowStartFiring, neckLowered.PhaseAfter,
            "the charge's end lowers the neck and runs on into the start of firing");
        AssertEqual<int?>(MotherBrainBeamDefinitions.NeckLoweredSamusCooldown, neckLowered.SamusProjectileCooldown,
            "lowering the neck gives Samus the beam's cooldown");

        // The live encounter gives the requested cooldown to Samus's shared one, replacing a longer cooldown.
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(EnemyDefinitionId.MotherBrainBody, body.Definition);
        var state = new MotherBrainEnemyState(body) { Head = enemies.SlotTable.Slots[1], Form = MotherBrainForm.SecondPhase };
        enemies.MotherBrainEncounter.MotherBrain = state;
        var bombs = new SamusBombProjectileSystem();
        bombs.SetSharedCooldown(20);
        enemies.MotherBrainBehavior.RainbowAndDeath.ApplyLiveMotherBrainRainbowState(state, attack, neckLowered, samus, bombs);
        AssertEqual((ushort)MotherBrainBeamDefinitions.NeckLoweredSamusCooldown, bombs.CooldownTimer.RawValue,
            "the encounter replaces Samus's longer cooldown with the beam's");

        MotherBrainRainbowBeamAttackStepResult? started = null;
        while (attack.State.Phase == MotherBrainBodyFunction.SecondPhaseRainbowStartFiring)
        {
            MotherBrainRainbowBeamAttackStepResult charging = attack.Step(samus, Frame());
            if (attack.State.Phase == MotherBrainBodyFunction.SecondPhaseRainbowStartFiring)
                AssertEqual<int?>(null, charging.SamusProjectileCooldown, "the widening beam gives Samus no cooldown");
            else
                started = charging;
        }
        AssertEqual<MotherBrainBodyFunction?>(MotherBrainBodyFunction.SecondPhaseRainbowMoveSamusTowardWall, started?.PhaseAfter,
            "the start of firing captures Samus");
        AssertEqual<int?>(MotherBrainBeamDefinitions.BeamStartedSamusCooldown, started?.SamusProjectileCooldown,
            "the beam's start clears Samus's cooldown");
        AssertTrue(samus.Control.AlphaLocked, "the beam's start locks Samus");

        MotherBrainRainbowBeamAttackStepResult? ended = null;
        while (attack.State.Phase != MotherBrainBodyFunction.SecondPhaseRainbowLetSamusFall)
        {
            MotherBrainRainbowBeamAttackStepResult firing = attack.Step(samus, Frame());
            if (attack.State.Phase == MotherBrainBodyFunction.SecondPhaseRainbowLetSamusFall)
                ended = firing;
            else
                AssertEqual<int?>(null, firing.SamusProjectileCooldown, $"the held beam gives Samus no cooldown in {attack.State.Phase}");
        }
        AssertTrue(!samus.Control.AlphaLocked, "the beam's end releases Samus");
        AssertEqual<int?>(MotherBrainBeamDefinitions.BeamEndedSamusCooldown, ended?.SamusProjectileCooldown,
            "the beam's end gives Samus its cooldown as it releases her");

        MotherBrainRainbowBeamAttackStepResult fall = attack.Step(samus, Frame());
        AssertEqual<int?>(null, fall.SamusProjectileCooldown, "letting Samus fall gives her no cooldown");
        Console.WriteLine("  Mother Brain: the rainbow beam gives Samus's cooldown as its neck lowers, as it starts and as it ends.");
    }
}
