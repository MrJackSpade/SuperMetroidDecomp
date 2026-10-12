using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;

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
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState { Pose = SamusPoseId.ShinesparkWindupLeftPose, Position = FixedPosition<RoomPixels>.FromWhole(new(0x6c, 0x98)) };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        AssertTrue(samus.Shinespark.TryStoreFromSpeedBooster(0x0400), "a shine is stored");
        samus.Shinespark.BeginWindup(samus);
        samus.Shinespark.RelinquishMovementHandler();
        AssertEqual(ShinesparkPhase.Inactive, samus.Shinespark.Phase, "normal movement replaced the windup handler");

        samus.ApplyShinesparkDirectionTransition(bus, SamusPoseId.ShinesparkHorizontalLeftPose);

        AssertEqual(ShinesparkPhase.Horizontal, samus.Shinespark.Phase, "the left launch installs the horizontal handler");
        AssertEqual(SamusPoseId.ShinesparkHorizontalLeftPose, samus.Pose, "Samus takes the left launch pose");
        Console.WriteLine("  Knockback shinespark launch: a direction launches from a windup pose without its handler.");
    }

    [VerificationCommand("--knockback-shinespark-launch")]
    internal static int KnockbackShinesparkLaunchCommand()
    {
        Suite(nameof(VerifyKnockbackShinesparkLaunch), () => VerifyKnockbackShinesparkLaunch());
        return 0;
    }
}
