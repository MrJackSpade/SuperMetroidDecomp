using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Poses;

/// <summary>
/// Verifies the fall an upright Samus takes when a bomb jump passes its apex with no floor below (#2553): the humanoid entry
/// of the falling pose setup ($91:E8F2) gives her the plain fall facing her way, where the port once threw.
/// </summary>
internal static class BombJumpUprightWalkOffVerification
{
    /// <summary>
    /// Confirms a standing Samus facing right falls in <see cref="SamusPoseId.FallingRightPose"/> and a crouching one facing
    /// left in <see cref="SamusPoseId.FallingLeftPose"/>, each through open air, keeping the bomb jump's vertical speed.
    /// </summary>
    internal static void VerifyBombJumpUprightWalkOff()
    {
        var air = new PlainTerrain(new Extent16<RoomBlocks>(16, 16), Enumerable.Repeat(PlainTerrainBlock.Air, 16 * 16).ToArray());
        foreach ((SamusPoseId upright, SamusPoseId fall) in new[]
        {
            (SamusPoseId.FacingRightNormalPose, SamusPoseId.FallingRightPose),
            (SamusPoseId.CrouchingLeftPose, SamusPoseId.FallingLeftPose),
        })
        {
            var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(0x80, 0x80)) }.InPose(upright);
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            var speed = samus.Kinematics.VerticalVelocity;

            SamusPosturePoseTransitions.ApplyBombJumpFallingPose(samus, air, frameCounter: 0);

            AssertEqual(fall, samus.Pose, $"{upright} walks off a bomb jump's apex into the plain fall");
            AssertEqual(speed, samus.Kinematics.VerticalVelocity, $"{upright}'s fall keeps the bomb jump's vertical speed");
        }
        Console.WriteLine("  BombJumpUprightWalkOff: upright poses fall facing their way.");
    }
}
