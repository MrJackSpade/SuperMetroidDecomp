using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using static RetailRuntimeFixtureVerification;

/// <summary>Checks Samus's movement on the frame Mother Brain releases the rainbow beam lock.</summary>
internal static class RainbowReleaseKnockbackVerification
{
    /// <summary>Confirms command one restores beta $E725 in time for knockback pose $54's mover on that same frame.</summary>
    /// <remarks>#1269: the locked beta $E8D9 holds Samus at Y $7D; the unlocked frame must let her fall as in the 100% movie.</remarks>
    internal static void VerifyRainbowReleaseKnockback()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.Position = samus.Position.WithWhole(new(0x00eb, 0x007d));
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0);
        samus.Kinematics.YSpeed = samus.Kinematics.YSubspeed = 0;

        var rainbow = new MotherBrainRainbowBeamAttackSequence();
        rainbow.StartActiveBeam(runtime.AddressSpace, samus);
        AssertEqual(DrainedSamusPhase.RainbowBeamLocked, samus.Drained.Phase, "the rainbow beam locks Samus");
        AssertEqual(SamusPoseId.KnockbackLeftPose, samus.Pose, "the lock installs knockback pose $54");

        runtime.StepFrame(0);
        AssertEqual((ushort)0x007d, samus.Position.Y.Whole, "the locked beta moves nothing");

        samus.InputLocked = false; // Samus command one, as the rainbow sequence's finish runs it.
        runtime.StepFrame(0);
        AssertTrue(samus.Position.Y.Whole > 0x007d, "the restored beta runs the knockback mover and Samus falls");
        Console.WriteLine("Rainbow release knockback: command one restores the type-$0A mover on its own frame.");
    }

    /// <summary>Runs the focused rainbow-release knockback verification command.</summary>
    [VerificationCommand("--rainbow-release-knockback")]
    internal static int RainbowReleaseKnockbackCommand()
    {
        VerifyRainbowReleaseKnockback();
        return 0;
    }
}
