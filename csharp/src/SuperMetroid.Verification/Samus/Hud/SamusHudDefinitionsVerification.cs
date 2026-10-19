using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Hardware;
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
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus.Hud;

/// <summary>Checks compiled Samus HUD posture and movement policy against retail definitions and live consumers.</summary>
internal static class SamusHudDefinitionsVerification
{
    /// <summary>Compares every native handler and posture branch, then checks Grapple admission and charged-shot preservation.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus HUD definitions")]
    internal static void VerifySamusHudDefinitions(SuperMetroidAddressSpace rom)
    {
        bool Transition(SamusPoseId pose, bool active) => pose >= SamusHudRomData.StandardTransitionStart ||
            (pose < SamusHudRomData.NonFiringTransitionStart &&
                (rom.ReadCartridgeByte(SamusHudRomData.TransitionFlags + (int)pose -
                    (int)SamusHudRomData.FirstTransitionPose) == 0 || active));
        SamusPoseId metadataPose = SamusPoseId.FacingRightNormalPose;
        var metadata = NewGrappleFiringReadGuard(rom, () => metadataPose);
        var bus = NewHudPolicyReadGuard(metadata);
        for (int pose = 0; pose <= byte.MaxValue; pose++)
        foreach (bool active in new[] { false, true })
        {
            if (pose < (int)SamusHudRomData.NonFiringTransitionStart)
                AssertEqual(rom.ReadCartridgeByte(SamusHudRomData.TransitionFlags + pose -
                        (int)SamusHudRomData.FirstTransitionPose),
                    SamusHudDefinitions.PostureObservation((SamusPoseId)pose, InstalledFixtures.SamusOverreads),
                    "Complete bounded posture-index observation");
            AssertEqual(Transition((SamusPoseId)pose, active),
                SamusHudInput.PostureTransitionAdmitsWeapons((SamusPoseId)pose, active, InstalledFixtures.SamusOverreads),
                "Every posture pose and active-Grapple branch");
        }
        var samus = new SamusState();
        Action<SamusState, ushort> setTurn = (state, direction) => state.PoseTransitionShotDirection = direction;
        for (byte movement = 0; movement < 28; movement++)
        {
            ushort handler = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x90dd05 + movement * 2));
            AssertEqual(handler, (ushort)SamusHudDefinitions.MovementHandler((SamusMovementType)movement), "All 28 native HUD handler identities");
            for (int pose = 0; pose < 253; pose++)
            foreach (bool locked in new[] { false, true })
            foreach (ushort selected in new ushort[] { 3, 4 })
            foreach (ushort turn in new ushort[] { 0, 1, ushort.MaxValue })
            {
                if (rom.ReadCartridgeByte(SamusMovementRomData.Poses.Definitions + pose * 8 + 1) != movement) continue;
                metadataPose = samus.Pose = (SamusPoseId)pose;
                samus.InputLocked = locked; samus.SelectedHudItem = HudItems.Decode(selected); setTurn(samus, turn);
                bool expected = !locked && pose is not (0 or 0x9b) && selected == 4 &&
                    (handler is 0xdd3d or 0xdd6f or 0xddd8 || handler == 0xdd74 && turn != 0 ||
                        handler == 0xdd8c && Transition((SamusPoseId)pose, false));
                AssertEqual(expected, SamusGrappleHudInput.IsSelectedAndAdmitted(samus, InstalledFixtures.SamusOverreads), "Actual Grapple HUD admission");
            }
        }
        // Exercise the other consumer, the real beam producer. Earn a partial charge
        // normally, then test whether the native HUD class holds it or advances it.
        var level = CreateRoom(new(64, 64), new ushort[4096], new byte[4096]);
        for (int poseIndex = 0; poseIndex < 253; poseIndex++)
        foreach (ushort turn in new ushort[] { 0, 1 })
        {
            SamusPoseId pose = (SamusPoseId)poseIndex;
            byte movement = rom.ReadCartridgeByte(SamusMovementRomData.Poses.Definitions + (int)pose * 8 + 1);
            metadataPose = SamusPoseId.FacingRightNormalPose;
            samus = new SamusState { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(512, 512)), EquippedBeams = SamusBeamFlags.Charge };
            var shots = new SamusProjectileSystem();
            var bombs = new SamusBombProjectileSystem();
            for (int frame = 0; frame < 10; frame++)
                shots.StepFrame(new SamusTerrain { Level = level }, samus, new ProjectileButtonInput { ControllerInput = SnesButton.X, ControllerNewInput = 0 }, new(0, 0), bombs);
            AssertEqual(10, shots.Charge.FlareCounter, "Fixture charge earned through real producer");
            metadataPose = samus.Pose = pose;
            setTurn(samus, turn);
            ushort handler = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x90dd05 + movement * 2));
            bool ball = pose is >= SamusPoseId.MorphBallGroundRightPose and <= SamusPoseId.MorphBallMovingLeftPose or SamusPoseId.MorphBallFallingRightPose or SamusPoseId.MorphBallFallingLeftPose or SamusPoseId.MorphBallGroundLeftPose or >= SamusPoseId.SpringBallGroundRightPose and <= SamusPoseId.SpringBallJumpLeftPose;
            bool preserves = ball || pose is SamusPoseId.ForwardFacingPowerSuitPose or SamusPoseId.ForwardFacingSuitedPose || handler == 0xddb6 || handler == 0xdd74 && turn == 0 ||
                handler == 0xdd8c && !Transition(pose, false);
            shots.StepFrame(new SamusTerrain { Level = level }, samus, new ProjectileButtonInput { ControllerInput = SnesButton.X, ControllerNewInput = 0 }, new(0, 0), bombs);
            // An admitted nonzero turn handoff forces release before held-input charge
            // processing; with no new Shoot edge this partial charge emits no projectile.
            AssertEqual(preserves ? 10 : turn != 0 ? 0 : 11, shots.Charge.FlareCounter, $"Actual projectile HUD charge preservation movement={movement:X2} pose={(int)pose:X2} turn={turn}");
            AssertTrue(shots.Slots.All(slot => !slot.IsActive), "Held partial charge does not emit shot");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => SamusHudDefinitions.PostureObservation(
                SamusHudRomData.NonFiringTransitionStart, InstalledFixtures.SamusOverreads),
            "posture observation rejects the native prefiltered range");
        Console.WriteLine("Samus HUD definitions: 28 handler words, all 219 bounded posture-index observations, 512 transition cases, 3036 native-pose Grapple admission cases and 506 real charge-preservation cases pass with policy reads forbidden.");
    }

    /// <summary>Forbids converted movement-handler and posture-flag ROM reads during HUD policy checks.</summary>
    private static MutableCartridgeReadGuard NewHudPolicyReadGuard(ISnesAddressSpace source)
    {
        int postureStart = SamusHudRomData.TransitionFlags -
            (int)SamusHudRomData.FirstTransitionPose;
        int postureEnd = SamusHudRomData.TransitionFlags +
            (int)SamusHudRomData.NonFiringTransitionStart -
            (int)SamusHudRomData.FirstTransitionPose;
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => address >= SamusHudRomData.MovementHandlers && address < 0x90dd3d ||
                address >= postureStart && address < postureEnd,
            address => $"Compiled HUD policy read ROM ${address:X6}.");
        return guard;
    }
}
