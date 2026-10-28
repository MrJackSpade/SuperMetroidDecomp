using static SuperMetroid.Verification.Rooms.Plms.CollectiblesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the compiled suit-pickup beam contour against retail bytes and guarded runtime use.</summary>
internal static class SuitPickupBeamCurveDefinitionsVerification
{
    /// <summary>Compares all 128 native contour offsets and rejects indices beyond the contour.</summary>
    internal static void VerifySuitPickupBeamCurveDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < SuitPickupBeamCurveDefinitions.OffsetCount; index++)
        {
            AssertEqual(
                rom.ReadCartridgeByte(SuitPickupBeamCurveDefinitions.NativeCurveAddress + index),
                SuitPickupBeamCurveDefinitions.OffsetAt(index),
                $"suit-pickup beam offset {index}");
        }

        AssertThrows<InvalidDataException>(
            () => SuitPickupBeamCurveDefinitions.OffsetAt(-1),
            "suit-pickup beam curve rejects a negative index");
        AssertThrows<InvalidDataException>(
            () => SuitPickupBeamCurveDefinitions.OffsetAt(
                SuitPickupBeamCurveDefinitions.OffsetCount),
            "suit-pickup beam curve rejects a post-contour index");

        Console.WriteLine(
            "Suit-pickup beam curve: all 128 native offsets match and invalid indexes fail loudly.");
    }

    /// <summary>Rejects cartridge reads from the compiled curve region during transformation.</summary>
    internal static CartridgeReadGuard NewSuitPickupBeamCurveReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address >= SuitPickupBeamCurveDefinitions.NativeCurveAddress &&
                address < SuitPickupBeamCurveDefinitions.NativeCurveAddress +
                    SuitPickupBeamCurveDefinitions.OffsetCount,
                address => $"Suit-pickup transformation attempted curve read ${address:X6}.");

    /// <summary>Runs contour parity and permanent-collectible checks from the verification host.</summary>
    [VerificationCommand("--suit-pickup-beam-curve-definitions")]
    internal static int SuitPickupBeamCurveDefinitionsCommand()
    {
        Suite(nameof(VerifySuitPickupBeamCurveDefinitions), () => VerifySuitPickupBeamCurveDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        Suite(nameof(VerifyPermanentCollectibles), () => VerifyPermanentCollectibles());
        return 0;
    }
}
