using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that automatic reserve recovery does not release Samus from Mother Brain's rainbow beam.</summary>
internal static class ReserveRefillRainbowBeamVerification
{
    /// <summary>
    /// Samus commands $1B and $10, which lock and free Samus around an automatic reserve refill,
    /// both leave Mother Brain's rainbow-beam handler pair ($90:E8D9) alone ($90:F414/$90:F2E3).
    /// In the 13% movie a refill runs while the rainbow beam holds Samus; native keeps her held
    /// afterward, while the port freed her and she fell.
    /// </summary>
    internal static void VerifyReserveRefillRainbowBeam()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState
        {
            Pose = SamusPoseId.FacingLeftNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(0x00eb, 0x007d)),
            Health = 0, MaxHealth = 99, ReserveEnergy = 3, MaxReserveEnergy = 100, ReserveTankMode = 1,
        };
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        samus.Drained.SetupForRainbowBeam(bus, samus);
        AssertTrue(samus.ScriptLocks.RainbowBeamHandlersInstalled, "the rainbow beam holds Samus");

        var recovery = new SamusReserveAutoRecoveryState();
        recovery.Begin(samus);
        AssertTrue(!samus.ScriptLocks.StationaryScriptControlLocked, "command $1B leaves the rainbow-beam pair in place");
        ushort frame = 0;
        while (!recovery.StepAfterNmi(samus, frame++).Completed) { }

        AssertTrue(samus.ScriptLocks.RainbowBeamHandlersInstalled, "command $10 leaves the rainbow beam holding Samus");
        Console.WriteLine("  Reserve refill under the rainbow beam: the beam's lock survives the refill.");
    }

    /// <summary>Runs the focused 13% movie reserve-refill and rainbow-beam handler check.</summary>
    [VerificationCommand("--reserve-refill-rainbow-beam")]
    internal static int ReserveRefillRainbowBeamCommand()
    {
        Suite(nameof(VerifyReserveRefillRainbowBeam), () => VerifyReserveRefillRainbowBeam());
        return 0;
    }
}
