using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>Checks both morph-ball families' landing transitions and compiled rebound speeds against cartridge data.</summary>
internal static class SamusBallBounceDefinitionsVerification
{
    /// <summary>Exercises signed landing speed, bounce phase, facing, and spring-ball branches while guarding physics ROM reads.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus ball bounce definitions")]
    internal static void VerifySamusBallBounceDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort whole = CartridgeImportSource.Require(rom).ReadCartridgeWord(SamusMovementDefinitionsVerticalMotionConstants.BallBounceSpeed);
        ushort fraction = CartridgeImportSource.Require(rom).ReadCartridgeWord(SamusMovementDefinitionsVerticalMotionConstants.BallBounceSubspeed);
        AssertEqual(whole, SamusVerticalMotionDefinitions.BallBounceSpeed.Whole, "Native ball rebound whole definition");
        AssertEqual(fraction, SamusVerticalMotionDefinitions.BallBounceSpeed.Fraction, "Native ball rebound fraction definition");
        int cases = 0;
        foreach (bool spring in new[] { false, true })
        foreach (bool left in new[] { false, true })
        {
            SamusPoseId pose = spring
                ? left ? SamusPoseId.SpringBallFallingLeftPose : SamusPoseId.SpringBallFallingRightPose
                : left ? SamusPoseId.MorphBallFallingLeftPose : SamusPoseId.MorphBallFallingRightPose;
            SamusPoseId groundedPose = spring
                ? left ? SamusPoseId.SpringBallGroundLeftPose : SamusPoseId.SpringBallGroundRightPose
                : left ? SamusPoseId.MorphBallGroundLeftPose : SamusPoseId.MorphBallGroundRightPose;
            var samus = new SamusState { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)) };
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);
            for (int phase = 0; phase < 3; phase++)
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                samus.Pose = pose;
                samus.MorphBallBounceState = phase switch
                {
                    0 => SamusBallBounceState.None,
                    1 => spring ? SamusBallBounceState.SpringBallFirstRebound : SamusBallBounceState.FirstRebound,
                    _ => spring ? SamusBallBounceState.SpringBallSecondRebound : SamusBallBounceState.SecondRebound,
                };
                samus.Kinematics.YSpeed = new UnsignedSixteenSixteen((ushort)raw, ushort.MaxValue);
                samus.Kinematics.YDirection = SamusVerticalDirection.Down;
                samus.HorizontalSpeed.BaseSpeed = new UnsignedSixteenSixteen(2, 0x8000);
                samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Decelerating;
                bool grounded = spring
                    ? SamusPosturePoseTransitions.ApplySpringBallLanding(samus, controllerInput: 0)
                    : SamusPosturePoseTransitions.ApplyMorphBallLanding(samus);
                bool first = phase == 0 && unchecked((short)(raw - 3)) >= 0;
                bool rebound = first || phase == 1;
                AssertEqual(!rebound, grounded, "Native rebound versus grounded return");
                AssertEqual(rebound ? (ushort)(first ? whole : whole - 1) : (ushort)0,
                    samus.Kinematics.YSpeed.Whole, "Native rebound whole decrement");
                AssertEqual(rebound ? fraction : (ushort)0, samus.Kinematics.YSpeed.Fraction, "Native rebound fraction");
                AssertEqual(rebound ? SamusVerticalDirection.Up : SamusVerticalDirection.None, samus.Kinematics.YDirection, "Native rebound direction");
                SamusBallBounceState published = !rebound ? SamusBallBounceState.None
                    : first
                        ? spring ? SamusBallBounceState.SpringBallFirstRebound : SamusBallBounceState.FirstRebound
                        : spring ? SamusBallBounceState.SpringBallSecondRebound : SamusBallBounceState.SecondRebound;
                AssertEqual(published, samus.MorphBallBounceState, "Native ball family/phase publication");
                AssertEqual(rebound ? pose : groundedPose, samus.Pose, "Rebound retains airborne pose until grounded");
                AssertEqual(rebound ? 0x00028000u : 0u, samus.HorizontalSpeed.BaseFixed,
                    "Both ball families clear horizontal base momentum on final landing");
                AssertEqual(rebound ? SamusAccelerationMode.Decelerating : SamusAccelerationMode.Accelerating, samus.HorizontalSpeed.AccelerationMode,
                    "Both ball families clear horizontal acceleration on final landing");
                AssertEqual((ushort)128, samus.Position.Y.Whole, "Landing handler does not move the collision-resolved center");
                cases++;
            }
        }
        Console.WriteLine($"Samus ball bounce definitions: {cases} actual signed-speed/phase/facing/family landings match native branches with physics reads forbidden.");
    }
}
