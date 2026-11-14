using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Samus.Grapple;

using SuperMetroid.Verification.Harness;

/// <summary>Checks that a grapple connection ends Samus's speed boost (#1704).</summary>
internal static class GrappleConnectionSpeedBoostVerification
{
    /// <summary>
    /// A Samus running at full boost whose beam connects to a block, facing right and facing left: the connection's
    /// shared tail cancels the boost as the original's does. Her running momentum and boost counter end, so she is no
    /// longer boosting and deals no boost contact damage, and the speed echoes leave at eight pixels an update the way
    /// the connected pose faces; her base, extra-run and vertical speeds are cleared and her acceleration mode is kept.
    /// </summary>
    internal static void VerifyGrappleConnectionCancelsSpeedBoost()
    {
        foreach ((SamusPoseId running, SamusProjectileDirection fired, SamusFacingDirection facing, int echoVelocity) in new[]
        {
            (SamusPoseId.MovingRightGunExtendedPose, SamusProjectileDirection.Right, SamusFacingDirection.Right, 8),
            (SamusPoseId.MovingLeftGunExtendedPose, SamusProjectileDirection.Left, SamusFacingDirection.Left, -8),
        })
        {
            SamusState samus = InstalledFixtures.CreateSamus();
            samus.Pose = running;
            samus.Position = FixedPosition<RoomPixels>.FromWhole(new(512, 512));
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            samus.HorizontalSpeed.HasRunningMomentum = true;
            samus.HorizontalSpeed.SpeedBoostCounter = SamusSpeedBoostCounter.AtFullStage;
            samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Turning;
            samus.HorizontalSpeed.BaseSpeed = new UnsignedSixteenSixteen(2, 0x4000);
            samus.HorizontalSpeed.ExtraRunSpeed = new UnsignedSixteenSixteen(4, 0x8000);
            AssertTrue(samus.HorizontalSpeed.IsActivelySpeedBoosting, $"{running}: the fixture boosts before it grapples");

            SamusGrappleState grapple = samus.Grapple;
            grapple.FireDirection = fired;
            grapple.Anchor = new(fired == SamusProjectileDirection.Right ? 560 : 464, 504);
            grapple.RopeLength = 48;
            samus.LiquidPhysics.BeginFrameSoundRequests();
            GrappleMovementResult connected = SamusGrappleMovement.ConnectAcceptedFiring(samus, grapple, new(512, 512));

            AssertTrue(connected.Phase is GrapplePhase.ConnectedLocked or GrapplePhase.ConnectedSwinging && connected.OwnsMovement,
                $"{running}: the beam connects and its pose is applied");
            AssertEqual(facing, SamusPoseMetadata.ReadFacingDirection(samus), $"{running}: the connected pose keeps her facing");
            AssertTrue(!samus.HorizontalSpeed.HasRunningMomentum, $"{running}: the connection ends her running momentum");
            AssertEqual(default(SamusSpeedBoostCounter), samus.HorizontalSpeed.SpeedBoostCounter,
                $"{running}: the connection clears the boost counter");
            AssertTrue(!samus.HorizontalSpeed.IsActivelySpeedBoosting, $"{running}: Samus no longer boosts once connected");
            AssertEqual(SpeedEchoCursor.Departing, samus.HorizontalSpeed.SpeedEchoCursor, $"{running}: the speed echoes depart");
            AssertEqual(new Offset16<RoomPixels>(echoVelocity, 0), samus.HorizontalSpeed.FirstSpeedEchoVelocity,
                $"{running}: the first echo leaves the way the connected pose faces");
            AssertEqual(new Offset16<RoomPixels>(echoVelocity, 0), samus.HorizontalSpeed.SecondSpeedEchoVelocity,
                $"{running}: the second echo leaves the way the connected pose faces");
            AssertEqual(default(UnsignedSixteenSixteen), samus.HorizontalSpeed.BaseSpeed, $"{running}: the connection clears the base speed");
            AssertEqual(default(UnsignedSixteenSixteen), samus.HorizontalSpeed.ExtraRunSpeed,
                $"{running}: the connection clears the extra run speed");
            AssertEqual(SamusAccelerationMode.Turning, samus.HorizontalSpeed.AccelerationMode,
                $"{running}: the connection keeps the acceleration mode");
        }
        Console.WriteLine("  Grapple connection: a boosting Samus's connection ends the boost and sends the echoes off her way.");
    }

    /// <summary>Runs the grapple connection's speed-boost check.</summary>
    [VerificationCommand("--grapple-connection-speed-boost")]
    internal static int GrappleConnectionSpeedBoostCommand()
    {
        Suite(nameof(VerifyGrappleConnectionCancelsSpeedBoost), () => VerifyGrappleConnectionCancelsSpeedBoost());
        return 0;
    }
}
