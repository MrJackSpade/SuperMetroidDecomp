using SuperMetroid.Core.Game;

internal static partial class Program
{
    /// <summary>
    /// A knockback ending while Samus winds up a shinespark installs normal movement
    /// (<c>$91:F31D</c>) over the windup handler, leaving her in pose $C7/$C8 with no
    /// countdown. A direction then still launches: <c>$91:FACA</c> installs the launch
    /// handler over whatever is current. The 13% movie launches a left spark this way.
    /// </summary>
    private static void VerifyKnockbackShinesparkLaunch()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState { Pose = SamusPoseId.ShinesparkWindupLeftPose, XPosition = 0x6c, YPosition = 0x98 };
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
}
