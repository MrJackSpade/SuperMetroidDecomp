using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Movement;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Spring Ball's in-air rebound path when Jump is released.</summary>
internal static class SpringBallBounceVerification
{
    /// <summary>Confirms $90:A6F1 chooses morphed bounce at $90:91D1 while the bounce state is nonzero, preserving the one-pixel rise before gravity.</summary>
    internal static void VerifySpringBallBounce()
    {
        var room = CreateRoom(new(16, 32), new RoomTerrainBlock[16 * 32], new RoomBlockBehavior[16 * 32]);
        foreach (SamusBallBounce? bounceState in new[] { SamusBallBounce.SpringBallFirst, (SamusBallBounce?)null })
        {
            var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(0x80, 0x169)),
                MorphBallBounceState = bounceState,
            }.InPose(SamusPoseId.SpringBallJumpRightPose);
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0xffff);
            samus.Kinematics.VerticalVelocity = SamusVerticalVelocityConstants.Of(new UnsignedSixteenSixteen(1, 0), SamusVerticalDirection.Up);
            samus.Kinematics.YAcceleration = samus.Kinematics.YAcceleration.WithFraction(0x1c00);

            SamusMorphBallMovement.StepSpringBallInAir(SamusMovement.SpringBallInAir, SamusTerrain.InRoom(room, SamusRoomObjectFixtures.Uninstalled), samus, GameplayAction.Right, 0);
            string context = $"Spring Ball in air with bounce state {bounceState}";
            if (bounceState != (SamusBallBounce?)null)
            {
                AssertEqual((ushort)0x168, samus.Position.Y.Whole, $"{context} rises by the rebound speed");
                AssertEqual((ushort)0xffff, samus.Kinematics.Position.Y.Fraction, $"{context} Y fraction");
                AssertEqual(SamusVerticalDirection.Up, samus.Kinematics.VerticalVelocity.Direction, $"{context} keeps rising");
                AssertEqual(0, samus.Kinematics.VerticalVelocity.Speed.Whole, $"{context} Y speed");
                AssertEqual(0xe400, samus.Kinematics.VerticalVelocity.Speed.Fraction, $"{context} Y subspeed after gravity");
            }
            else
            {
                AssertEqual(SamusVerticalDirection.Down, samus.Kinematics.VerticalVelocity.Direction, $"{context} released Jump cuts the rise");
            }
        }
        Console.WriteLine("Spring Ball bounce: a rebound in the air uses the bouncing routine, not the jump cutoff.");
    }

    /// <summary>Runs the focused 100% movie Spring Ball rebound check.</summary>
    [VerificationCommand("--spring-ball-bounce")]
    internal static int SpringBallBounceCommand()
    {
        VerifySpringBallBounce();
        return 0;
    }
}
