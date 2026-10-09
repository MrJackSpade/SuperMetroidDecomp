using SuperMetroid.Core.Game;

internal static partial class Program
{
    /// <summary>
    /// A damage boost's exit to a normal-jump pose runs the ordinary initializer $91:F543, which
    /// reselects the acceleration mode from extra run speed: none left means ordinary
    /// acceleration (mode zero). In the 13% movie Samus leaves a damage boost with base speed
    /// only; native drops the boost's deceleration mode, while the port kept it and slowed her.
    /// </summary>
    private static void VerifyDamageBoostJumpInitializer()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState { Pose = SamusPoseIds.DamageBoostLeftPose, XPosition = 0x00b6, YPosition = 0x0082 };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.HorizontalSpeed.BaseSpeed = 5;
        samus.HorizontalSpeed.AccelerationMode = SamusHorizontalAccelerationModes.Decelerating;

        SamusKnockbackMovement.ApplyDamageBoostPoseTransition(bus, samus, SamusPoseIds.NeutralJumpLeftPose, 0);
        AssertEqual(SamusPoseIds.NeutralJumpLeftPose, samus.Pose, "the boost exits to neutral jump $4E");
        AssertEqual(SamusHorizontalAccelerationModes.Accelerating, samus.HorizontalSpeed.AccelerationMode,
            "with no extra run speed the initializer selects ordinary acceleration");
        Console.WriteLine("  Damage-boost jump initializer: the exit reselects the acceleration mode.");
    }
}
