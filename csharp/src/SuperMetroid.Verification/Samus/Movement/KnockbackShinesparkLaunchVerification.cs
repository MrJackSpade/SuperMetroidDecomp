using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Movement;
using SuperMetroid.Verification.Harness;

/// <summary>Exercises a shinespark direction launch after knockback has replaced Samus's windup movement handler.</summary>
internal static class KnockbackShinesparkLaunchVerification
{
    /// <summary>
    /// A knockback ending while Samus winds up a shinespark installs normal movement
    /// (<c>$91:F31D</c>) over the windup handler, leaving her in pose $C7/$C8 with no
    /// countdown. A direction then still launches: <c>$91:FACA</c> installs the launch
    /// handler over whatever is current. The 13% movie launches a left spark this way.
    /// </summary>
    internal static void VerifyKnockbackShinesparkLaunch()
    {
        var samus = new SamusState { Pose = SamusPoseId.ShinesparkWindupLeftPose, Position = FixedPosition<RoomPixels>.FromWhole(new(0x6c, 0x98)) };
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        AssertTrue(samus.Shinespark.TryStoreFromSpeedBooster(new SamusSpeedBoostCounter(0x0400)), "a shine is stored");
        samus.Shinespark.BeginWindup(samus);
        samus.Shinespark.RelinquishMovementHandler();
        AssertEqual(ShinesparkPhase.Inactive, samus.Shinespark.Phase, "normal movement replaced the windup handler");

        SamusAerialPoseTransitions.ApplyShinesparkDirectionTransition(samus, SamusPoseId.ShinesparkHorizontalLeftPose);

        AssertEqual(ShinesparkPhase.Horizontal, samus.Shinespark.Phase, "the left launch installs the horizontal handler");
        AssertEqual(SamusPoseId.ShinesparkHorizontalLeftPose, samus.Pose, "Samus takes the left launch pose");
        Console.WriteLine("  Knockback shinespark launch: a direction launches from a windup pose without its handler.");
    }

    /// <summary>Runs the focused 13% movie knockback-to-left-shinespark transition check.</summary>
    [VerificationCommand("--knockback-shinespark-launch")]
    internal static int KnockbackShinesparkLaunchCommand()
    {
        Suite(nameof(VerifyKnockbackShinesparkLaunch), () => VerifyKnockbackShinesparkLaunch());
        return 0;
    }
}
