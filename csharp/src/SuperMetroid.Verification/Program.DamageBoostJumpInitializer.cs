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

/// <summary>Represents damage boost jump initializer verification.</summary>
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
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState { Pose = SamusPoseId.DamageBoostLeftPose, Position = FixedPosition<RoomPixels>.FromWhole(new(0x00b6, 0x0082)) };
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        samus.HorizontalSpeed.BaseSpeed = 5;
        samus.HorizontalSpeed.AccelerationMode = SamusHorizontalAccelerationModes.Decelerating;

        SamusKnockbackMovement.ApplyDamageBoostPoseTransition(bus, samus, SamusPoseId.NeutralJumpLeftPose, 0);
        AssertEqual(SamusPoseId.NeutralJumpLeftPose, samus.Pose, "the boost exits to neutral jump $4E");
        AssertEqual(SamusHorizontalAccelerationModes.Accelerating, samus.HorizontalSpeed.AccelerationMode,
            "with no extra run speed the initializer selects ordinary acceleration");
        Console.WriteLine("  Damage-boost jump initializer: the exit reselects the acceleration mode.");
    }

    /// <summary>damage boost jump initializer command.</summary>
    [VerificationCommand("--damage-boost-jump-initializer")]
    internal static int DamageBoostJumpInitializerCommand()
    {
        Suite(nameof(VerifyDamageBoostJumpInitializer), () => VerifyDamageBoostJumpInitializer());
        return 0;
    }
}
