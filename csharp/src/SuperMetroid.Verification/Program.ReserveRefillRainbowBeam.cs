using System.Reflection;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    /// <summary>
    /// Samus commands $1B and $10, which lock and free Samus around an automatic reserve refill,
    /// both leave Mother Brain's rainbow-beam handler pair ($90:E8D9) alone ($90:F414/$90:F2E3).
    /// In the 13% movie a refill runs while the rainbow beam holds Samus; native keeps her held
    /// afterward, while the port freed her and she fell.
    /// </summary>
    private static void VerifyReserveRefillRainbowBeam()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState
        {
            Pose = SamusPoseId.FacingLeftNormalPose, XPosition = 0x00eb, YPosition = 0x007d,
            Health = 0, MaxHealth = 99, ReserveEnergy = 3, MaxReserveEnergy = 100, ReserveTankMode = 1,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        typeof(SamusDrainedState).GetMethod("SetupForRainbowBeam", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(samus.Drained, [bus, samus]);
        AssertTrue(samus.RainbowBeamHandlersInstalled, "the rainbow beam holds Samus");

        var recovery = new SamusReserveAutoRecoveryState();
        recovery.Begin(samus);
        AssertTrue(!samus.StationaryScriptControlLocked, "command $1B leaves the rainbow-beam pair in place");
        ushort frame = 0;
        while (!recovery.StepAfterNmi(samus, frame++).Completed) { }

        AssertTrue(samus.RainbowBeamHandlersInstalled, "command $10 leaves the rainbow beam holding Samus");
        Console.WriteLine("  Reserve refill under the rainbow beam: the beam's lock survives the refill.");
    }
}
