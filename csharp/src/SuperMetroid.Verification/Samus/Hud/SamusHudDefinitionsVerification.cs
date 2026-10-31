using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Samus.Grapple.GrappleFiringDefinitionsVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Hud;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Verification.Samus.Hud;

/// <summary>Checks compiled Samus HUD posture and movement policy against retail definitions and live consumers.</summary>
internal static class SamusHudDefinitionsVerification
{
    /// <summary>Compares every native handler and posture branch, then checks Grapple admission and charged-shot preservation.</summary>
    internal static void VerifySamusHudDefinitions(SuperMetroidAddressSpace rom)
    {
        bool Transition(SamusPoseId pose, bool active) => pose >= SamusHudDefinitions.StandardTransitionStart ||
            (pose < SamusHudDefinitions.NonFiringTransitionStart &&
                (rom.ReadCartridgeByte(SamusCartridgeTablesConstants.HudTransitionFlags + (int)pose -
                    (int)SamusHudDefinitions.FirstTransitionPose) == 0 || active));
        SamusPoseId metadataPose = SamusPoseId.FacingRightNormalPose;
        var metadata = NewGrappleFiringReadGuard(rom, () => metadataPose);
        var bus = NewHudPolicyReadGuard(metadata);
        for (int pose = 0; pose <= byte.MaxValue; pose++)
        foreach (bool active in new[] { false, true })
        {
            if (pose < (int)SamusHudDefinitions.NonFiringTransitionStart)
                AssertEqual(rom.ReadCartridgeByte(SamusCartridgeTablesConstants.HudTransitionFlags + pose -
                        (int)SamusHudDefinitions.FirstTransitionPose) == 0,
                    SamusHudObservations.PostureAdmitsWeapons((SamusPoseId)pose, InstalledFixtures.SamusOverreads.PostureAdmission),
                    "Complete bounded posture-index admission: the byte read is zero");
            AssertEqual(Transition((SamusPoseId)pose, active),
                SamusHudInput.PostureTransitionAdmitsWeapons((SamusPoseId)pose, active, InstalledFixtures.SamusOverreads.PostureAdmission),
                "Every posture pose and active-Grapple branch");
        }
        var samus = new SamusState(InstalledFixtures.AnimationContinuation);
        Action<SamusState, ushort> setTurn = (state, turn) =>
            state.PendingTransitionShot = turn == 0 ? null : new SamusTransitionShot(turn & byte.MaxValue);
        for (byte movement = 0; movement < 28; movement++)
        {
            ushort handler = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x90dd05 + movement * 2));
            AssertEqual(handler, (ushort)SamusHudObservations.MovementHandler((SamusMovementType)movement), "All 28 native HUD handler identities");
            for (int pose = 0; pose < 253; pose++)
            foreach (bool locked in new[] { false, true })
            foreach (ushort selected in new ushort[] { 3, 4 })
            foreach (ushort turn in new ushort[] { 0, 1, ushort.MaxValue })
            {
                if (rom.ReadCartridgeByte(SamusSourceDefinitions.PoseDefinitions.Value + pose * 8 + 1) != movement) continue;
                metadataPose = samus.Pose = (SamusPoseId)pose;
                if (locked) samus.Control.LockHandlers(); else samus.Control.Unlock();
                samus.SelectedHudItem = HudItems.Decode(selected); setTurn(samus, turn);
                bool expected = !locked && pose is not (0 or 0x9b) && selected == 4 &&
                    (handler is 0xdd3d or 0xdd6f or 0xddd8 || handler == 0xdd74 && turn != 0 ||
                        handler == 0xdd8c && Transition((SamusPoseId)pose, false));
                AssertEqual(expected, SamusGrappleHudInput.IsSelectedAndAdmitted(samus, InstalledFixtures.SamusOverreads.PostureAdmission), "Actual Grapple HUD admission");
            }
        }
        // Exercise the other consumer, the real beam producer. Earn a partial charge
        // normally, then test whether the native HUD class holds it or advances it.
        var level = CreateRoom(new(64, 64), new RoomTerrainBlock[4096], new RoomBlockBehavior[4096]);
        for (int poseIndex = 0; poseIndex < 253; poseIndex++)
        foreach (ushort turn in new ushort[] { 0, 1 })
        {
            SamusPoseId pose = (SamusPoseId)poseIndex;
            byte movement = rom.ReadCartridgeByte(SamusSourceDefinitions.PoseDefinitions.Value + (int)pose * 8 + 1);
            metadataPose = SamusPoseId.FacingRightNormalPose;
            samus = new SamusState(InstalledFixtures.AnimationContinuation) { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(512, 512)), EquippedBeams = SamusBeamFlags.Charge };
            var shots = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None, InstalledFixtures.SamusOverreads);
            var bombs = new SamusBombProjectileSystem();
            for (int frame = 0; frame < 10; frame++)
                shots.StepFrame(new SamusTerrain { Level = level }, samus, new ProjectileButtonInput { ControllerInput = ControllerButton.X, ControllerNewInput = 0 }, new(0, 0), bombs);
            AssertEqual(10, shots.Charge.FlareCounter, "Fixture charge earned through real producer");
            metadataPose = samus.Pose = pose;
            setTurn(samus, turn);
            ushort handler = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x90dd05 + movement * 2));
            bool ball = pose is >= SamusPoseId.MorphBallGroundRightPose and <= SamusPoseId.MorphBallMovingLeftPose or SamusPoseId.MorphBallFallingRightPose or SamusPoseId.MorphBallFallingLeftPose or SamusPoseId.MorphBallGroundLeftPose or >= SamusPoseId.SpringBallGroundRightPose and <= SamusPoseId.SpringBallJumpLeftPose;
            bool preserves = ball || pose is SamusPoseId.ForwardFacingPowerSuitPose or SamusPoseId.ForwardFacingSuitedPose || handler == 0xddb6 || handler == 0xdd74 && turn == 0 ||
                handler == 0xdd8c && !Transition(pose, false);
            shots.StepFrame(new SamusTerrain { Level = level }, samus, new ProjectileButtonInput { ControllerInput = ControllerButton.X, ControllerNewInput = 0 }, new(0, 0), bombs);
            // An admitted nonzero turn handoff forces release before held-input charge
            // processing; with no new Shoot edge this partial charge emits no projectile.
            AssertEqual(preserves ? 10 : turn != 0 ? 0 : 11, shots.Charge.FlareCounter, $"Actual projectile HUD charge preservation movement={movement:X2} pose={(int)pose:X2} turn={turn}");
            AssertTrue(shots.Slots.All(slot => !slot.IsActive), "Held partial charge does not emit shot");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => SamusHudObservations.PostureAdmitsWeapons(
                SamusHudDefinitions.NonFiringTransitionStart, InstalledFixtures.SamusOverreads.PostureAdmission),
            "posture observation rejects the native prefiltered range");
        Console.WriteLine("Samus HUD definitions: 28 handler words, all 219 bounded posture-index observations, 512 transition cases, 3036 native-pose Grapple admission cases and 506 real charge-preservation cases pass with policy reads forbidden.");
    }

    /// <summary>Forbids converted movement-handler and posture-flag ROM reads during HUD policy checks.</summary>
    private static MutableCartridgeReadGuard NewHudPolicyReadGuard(ISnesAddressSpace source)
    {
        int postureStart = SamusCartridgeTablesConstants.HudTransitionFlags -
            (int)SamusHudDefinitions.FirstTransitionPose;
        int postureEnd = SamusCartridgeTablesConstants.HudTransitionFlags +
            (int)SamusHudDefinitions.NonFiringTransitionStart -
            (int)SamusHudDefinitions.FirstTransitionPose;
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => address >= SamusCartridgeTablesConstants.HudMovementHandlers && address < 0x90dd3d ||
                address >= postureStart && address < postureEnd,
            address => $"Compiled HUD policy read ROM ${address:X6}.");
        return guard;
    }
}
