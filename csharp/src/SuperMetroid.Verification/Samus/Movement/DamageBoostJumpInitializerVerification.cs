using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Movement;
using SuperMetroid.Verification.Harness;

/// <summary>Reproduces a damage-boost exit that must reselect ordinary
/// horizontal acceleration when no extra run speed remains.</summary>
internal static class DamageBoostJumpInitializerVerification
{
    /// <summary>
    /// A damage boost's exit to a normal-jump pose runs the ordinary initializer $91:F543, which
    /// reselects the acceleration mode from extra run speed: none left means ordinary
    /// acceleration (mode zero). In the 13% movie Samus leaves a damage boost with base speed
    /// only; native drops the boost's deceleration mode, while the port kept it and slowed her.
    /// </summary>
    internal static void VerifyDamageBoostJumpInitializer()
    {
        var samus = new SamusState { Pose = SamusPoseId.DamageBoostLeftPose, Position = FixedPosition<RoomPixels>.FromWhole(new(0x00b6, 0x0082)) };
        samus.PoseMetadata.RefreshCollisionRadii();
        samus.Presentation.InitializeAnimation();
        samus.HorizontalSpeed.BaseSpeed = samus.HorizontalSpeed.BaseSpeed.WithWhole(5);
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Decelerating;

        SamusKnockbackMovement.ApplyDamageBoostPoseTransition(samus, SamusPoseId.NeutralJumpLeftPose, 0);
        AssertEqual(SamusPoseId.NeutralJumpLeftPose, samus.Pose, "the boost exits to neutral jump $4E");
        AssertEqual(SamusAccelerationMode.Accelerating, samus.HorizontalSpeed.AccelerationMode,
            "with no extra run speed the initializer selects ordinary acceleration");
        Console.WriteLine("  Damage-boost jump initializer: the exit reselects the acceleration mode.");
    }

    /// <summary>Runs the focused damage-boost jump handoff from the CLI.</summary>
    [VerificationCommand("--damage-boost-jump-initializer")]
    internal static int DamageBoostJumpInitializerCommand()
    {
        Suite(nameof(VerifyDamageBoostJumpInitializer), () => VerifyDamageBoostJumpInitializer());
        return 0;
    }
}
