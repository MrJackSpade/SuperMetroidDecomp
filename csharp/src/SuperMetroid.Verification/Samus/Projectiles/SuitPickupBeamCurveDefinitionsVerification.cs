using static SuperMetroid.Verification.Rooms.Plms.CollectiblesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the suit-pickup beam's rounded sides against the retail contour, and guarded runtime use.</summary>
internal static class SuitPickupBeamCurveDefinitionsVerification
{
    /// <summary>The offsets the retail contour holds for the beam's upper half, mirrored below its middle line.</summary>
    private const int ContourOffsets = SuitPickupBeamDefinitions.BeamLines / 2;

    /// <summary>
    /// The beam's rounded sides are now an oval the backend draws (#1468), not a per-line contour: checks that the
    /// authored reach at the middle line, <see cref="SuitPickupBeamDefinitions.CurveWidth"/>, is the retail contour's
    /// widest offset, and that the contour narrows to its smallest offset at the beam's top line.
    /// </summary>
    internal static void VerifySuitPickupBeamCurveDefinitions(SuperMetroidAddressSpace rom)
    {
        byte[] contour = new byte[ContourOffsets];
        for (int index = 0; index < contour.Length; index++)
            contour[index] = rom.ReadCartridgeByte(SuitPickupBeamDefinitions.BeamCurve + index);
        AssertEqual(SuitPickupBeamDefinitions.CurveWidth, (int)contour.Max(),
            "the rounded sides reach the retail contour's widest offset");
        AssertEqual(SuitPickupBeamDefinitions.CurveWidth, (int)contour[^1],
            "the retail contour is widest at the beam's middle line");
        AssertEqual(contour.Min(), contour[0], "the retail contour is narrowest at the beam's top line");
        Console.WriteLine("Suit-pickup beam curve: the authored oval's reach matches the retail contour's extremes.");
    }

    /// <summary>Rejects cartridge reads from the retail contour region during transformation.</summary>
    internal static CartridgeReadGuard NewSuitPickupBeamCurveReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address >= SuitPickupBeamDefinitions.BeamCurve &&
                address < SuitPickupBeamDefinitions.BeamCurve + ContourOffsets,
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
