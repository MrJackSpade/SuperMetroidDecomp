using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Hardware;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that migrated PLM instructions and speed rows consume live WRAM through typed boundaries.</summary>
internal static class MechanicsWorkRamBoundaryVerification
{
    /// <summary>Checks the statically identified PLM/speed WRAM conversions, never searches gameplay for cartridge reads.</summary>
    internal static void VerifyMechanicsWorkRamBoundary()
    {
        const BindingFlags privateStatic = BindingFlags.NonPublic | BindingFlags.Static;
        AssertTrue(!MetadataShape.DeclaresMethod(typeof(RoomObjectSystem), "ReadNativeBankByte", privateStatic) &&
            !MetadataShape.DeclaresMethod(typeof(RoomObjectSystem), "ReadBank84Word", privateStatic),
            "PLMs must not retain generic bank-data readers");
        AssertTrue(!MetadataShape.DeclaresMethod(typeof(SamusHorizontalSpeedState), "ReadMappedByte", privateStatic),
            "horizontal speed must not retain a mapped CPU-byte reader");
        // Samus's speeds are keyed by medium and movement (#1734), so no row past the speeds can be named; the indexed
        // speeds check covers every medium's and movement's speeds against the cartridge.
        Console.WriteLine("Mechanics WRAM boundary: generic PLM/speed readers absent.");
    }

    /// <summary>Runs the focused PLM and horizontal-speed WRAM ownership check.</summary>
    [VerificationCommand("--mechanics-workram-boundary")]
    internal static int MechanicsWorkramBoundaryCommand()
    {
        Suite(nameof(VerifyMechanicsWorkRamBoundary), () => VerifyMechanicsWorkRamBoundary());
        return 0;
    }
}
