using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    // #1269: Mother Brain's rainbow lock is beta $E8D9, which moves nothing. Command one
    // ($90:F117) restores beta $E725 during enemy AI, so that same frame's beta dispatches
    // knockback pose $54's type-$0A mover ($90:A5FC) and Samus falls. The port kept routing
    // the unlocked frame through the drained branch: in the 100% movie Samus stayed at Y $7D
    // for a frame while native fell to $7E.
    private static void VerifyRainbowReleaseKnockback()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        SamusState samus = runtime.Samus!;
        samus.XPosition = 0x00eb;
        samus.YPosition = 0x007d;
        samus.Kinematics.YSubposition = 0;
        samus.Kinematics.YSpeed = samus.Kinematics.YSubspeed = 0;

        var rainbow = new MotherBrainRainbowBeamAttackSequence();
        rainbow.StartActiveBeam(runtime.AddressSpace, samus);
        AssertEqual(DrainedSamusPhase.RainbowBeamLocked, samus.Drained.Phase, "the rainbow beam locks Samus");
        AssertEqual(SamusPoseIds.KnockbackLeftPose, samus.Pose, "the lock installs knockback pose $54");

        runtime.StepFrame(0);
        AssertEqual((ushort)0x007d, samus.YPosition, "the locked beta moves nothing");

        samus.InputLocked = false; // Samus command one, as the rainbow sequence's finish runs it.
        runtime.StepFrame(0);
        AssertTrue(samus.YPosition > 0x007d, "the restored beta runs the knockback mover and Samus falls");
        Console.WriteLine("Rainbow release knockback: command one restores the type-$0A mover on its own frame.");
    }
}
