using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using static SamusImpulseDefinitionsVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

/// <summary>Checks both morph-ball families' landing transitions and compiled rebound speeds against cartridge data.</summary>
internal static class SamusBallBounceDefinitionsVerification
{
    /// <summary>Exercises signed landing speed, bounce phase, facing, and spring-ball branches while guarding physics ROM reads.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus ball bounce definitions")]
    internal static void VerifySamusBallBounceDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort whole = CartridgeImportSource.Require(rom).ReadCartridgeWord(SamusMovementRomDataVerticalMotionConstants.BallBounceSpeed);
        ushort fraction = CartridgeImportSource.Require(rom).ReadCartridgeWord(SamusMovementRomDataVerticalMotionConstants.BallBounceSubspeed);
        AssertEqual(whole, SamusVerticalMotionDefinitions.BallBounceSpeed, "Native ball rebound whole definition");
        AssertEqual(fraction, SamusVerticalMotionDefinitions.BallBounceSubspeed, "Native ball rebound fraction definition");
        var guard = NewImpulsePoseReadGuard(rom);
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
            samus.PoseMetadata.RefreshCollisionRadii(guard);
            samus.Presentation.InitializeAnimation(guard);
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
                samus.Kinematics.YSpeed = (ushort)raw;
                samus.Kinematics.YSubspeed = ushort.MaxValue;
                samus.Kinematics.YDirection = SamusVerticalDirection.Down;
                samus.HorizontalSpeed.BaseSpeed = 2;
                samus.HorizontalSpeed.BaseSubspeed = 0x8000;
                samus.HorizontalSpeed.AccelerationMode = 2;
                bool grounded = spring
                    ? samus.PoseTransitions.ApplySpringBallLanding(guard, controllerInput: 0)
                    : samus.PoseTransitions.ApplyMorphBallLanding(guard);
                bool first = phase == 0 && unchecked((short)(raw - 3)) >= 0;
                bool rebound = first || phase == 1;
                AssertEqual(!rebound, grounded, "Native rebound versus grounded return");
                AssertEqual(rebound ? (ushort)(first ? whole : whole - 1) : (ushort)0,
                    samus.Kinematics.YSpeed, "Native rebound whole decrement");
                AssertEqual(rebound ? fraction : (ushort)0, samus.Kinematics.YSubspeed, "Native rebound fraction");
                AssertEqual(rebound ? SamusVerticalDirection.Up : SamusVerticalDirection.None, samus.Kinematics.YDirection, "Native rebound direction");
                SamusBallBounceState published = !rebound ? SamusBallBounceState.None
                    : first
                        ? spring ? SamusBallBounceState.SpringBallFirstRebound : SamusBallBounceState.FirstRebound
                        : spring ? SamusBallBounceState.SpringBallSecondRebound : SamusBallBounceState.SecondRebound;
                AssertEqual(published, samus.MorphBallBounceState, "Native ball family/phase publication");
                AssertEqual(rebound ? pose : groundedPose, samus.Pose, "Rebound retains airborne pose until grounded");
                AssertEqual(rebound ? 0x00028000u : 0u, samus.HorizontalSpeed.BaseFixed,
                    "Both ball families clear horizontal base momentum on final landing");
                AssertEqual(rebound ? (ushort)2 : (ushort)0, samus.HorizontalSpeed.AccelerationMode,
                    "Both ball families clear horizontal acceleration on final landing");
                AssertEqual((ushort)128, samus.Position.Y.Whole, "Landing handler does not move the collision-resolved center");
                cases++;
            }
        }
        Console.WriteLine($"Samus ball bounce definitions: {cases} actual signed-speed/phase/facing/family landings match native branches with physics reads forbidden.");
    }
}
