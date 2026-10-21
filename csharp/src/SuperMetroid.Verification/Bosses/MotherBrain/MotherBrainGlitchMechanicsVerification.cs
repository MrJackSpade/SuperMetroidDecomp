using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidFixtureHeadVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Checks the drained-pose, wall-jump, command-cancel, and Baby touch mechanics in the Mother Brain glitch movie.</summary>
internal static class MotherBrainGlitchMechanicsVerification
{
    /// <summary>
    /// #1275 Mother Brain glitch movie. The drained controllers store a pose without
    /// InitializeSamusPose_1, so an airborne Samus keeps her cached spin-jump movement type
    /// (and can wall-jump out); the Baby's latch touch runs before its main AI with the strict
    /// $A0:A07A test; Samus command $19 cancels the pending pose transition.
    /// </summary>
    internal static void VerifyMotherBrainGlitchMechanics()
    {

        // $91:E571 writes only $0A1C: the spin jump's $0A1E/$0A1F stay in force.
        var spinning = new SamusState { Pose = SamusPoseId.SpinJumpRightPose, Position = FixedPosition<RoomPixels>.FromWhole(new(0x80, 0x80)) };
        SamusPoseMetadata.RefreshCollisionRadii(spinning);
        SamusPresentation.InitializeAnimation(spinning);
        spinning.Drained.PutStanding(spinning);
        AssertEqual(SamusPoseId.DrainedStandingRightPose, spinning.Pose, "drained controller installs the standing drained pose");
        AssertEqual(SamusPoseId.SpinJumpRightPose, spinning.InitializedPose, "drained controller skips InitializeSamusPose_1");
        AssertEqual(SamusMovementType.SpinJumping, SamusPoseMetadata.ReadMovementType(spinning), "cached $0A1F keeps the spin-jump movement type");

        // $90:9DB2 tests only the Screw Attack poses; any other pose rewinds to frame $0A.
        SamusAerialPoseTransitions.ApplyWallContactAnimationRewind(spinning);
        AssertEqual(0x0a, spinning.AnimationFrame, "wall-contact rewind runs for the drained pose");
        // $91:EABE is gated by the spin mover, i.e. the cached type, not the live pose.
        SamusAerialPoseTransitions.ApplyWallJumpTrigger(spinning);
        AssertEqual(SamusPoseId.WallJumpRightPose, spinning.Pose, "a drained spinning Samus wall-jumps out");
        AssertEqual(SamusPoseId.WallJumpRightPose, spinning.InitializedPose, "the wall jump initializes its pose");

        // $90:F3FB returns carry set; Run_Samus_Command then cancels the pending transition.
        var frozen = new SamusState { Pose = SamusPoseId.FacingRightNormalPose };
        SamusDrainedState.FreezeForHyperBeamAcquisition(frozen);
        AssertTrue(frozen.PendingPoseTransitionCancelled, "Samus command $19 cancels the pending pose transition");

        // $A0:A07A: touch requires |dx| - SamusXRadius < enemy X radius (strict), skips an
        // invincible Samus, and uses the Baby's position before this frame's main AI moves it.
        ushort LatchVelocity(int startDx, ushort invincibility)
        {
            var samus = new SamusState { Health = 99, Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x00a0)), InvincibilityTimer = WrappingCountdown.FromRaw(invincibility) };
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            var baby = new BabyMetroidCutsceneState();
            baby.Initialize(previousOccupant: FixedPosition<RoomPixels>.Zero);
            baby.Phase = BabyMetroidCutscenePhase.LatchOntoSamus;
            baby.Position = FixedPosition<RoomPixels>.FromWhole(new((ushort)(0x0080 + startDx), 0x008c)).WithFractionsOf(baby.Position);
            // Moving left a whole pixel per frame: after this frame's mover the gap shrinks by one.
            baby.Velocity = new EightEightVector<RoomPixels>(new SnesSignedEightEight(0xff00), new SnesSignedEightEight(0));
            var motherBrain = new MotherBrainRainbowBeamAttackSequence();
            baby.Step(samus, motherBrain, HeadOf(motherBrain), layer1: Position16<RoomPixels>.Zero);
            return baby.Velocity.X.RawValue;
        }
        // Samus's X radius is five for every humanoid pose.
        int touchingDx = 5 + BabyMetroidCutsceneState.XHitboxRadius - 1;
        ushort untouched = LatchVelocity(touchingDx, invincibility: 1);
        AssertTrue(LatchVelocity(touchingDx, invincibility: 0) != untouched, "the Baby touches a vulnerable Samus inside the strict gap");
        AssertEqual(untouched, LatchVelocity(touchingDx + 1, invincibility: 0),
            "a gap equal to the summed radii is not a touch, even though the mover then closes it");
        Console.WriteLine("Mother Brain glitch mechanics: uninitialized drained pose, wall-jump escape, command $19 cancel and strict pre-AI Baby touch agree.");
    }
}
