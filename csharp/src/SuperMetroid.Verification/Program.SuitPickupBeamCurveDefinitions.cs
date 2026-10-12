using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using static CollectiblesVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Represents suit pickup beam curve definitions verification.</summary>
internal static class SuitPickupBeamCurveDefinitionsVerification
{
    /// <summary>verify suit pickup beam curve definitions.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for suit pickup beam curve definitions")]
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

    /// <summary>new suit pickup beam curve read guard.</summary>
    internal static CartridgeReadGuard NewSuitPickupBeamCurveReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address >= SuitPickupBeamCurveDefinitions.NativeCurveAddress &&
                address < SuitPickupBeamCurveDefinitions.NativeCurveAddress +
                    SuitPickupBeamCurveDefinitions.OffsetCount,
                address => $"Suit-pickup transformation attempted curve read ${address:X6}.");

    /// <summary>suit pickup beam curve definitions command.</summary>
    [VerificationCommand("--suit-pickup-beam-curve-definitions")]
    internal static int SuitPickupBeamCurveDefinitionsCommand()
    {
        Suite(nameof(VerifySuitPickupBeamCurveDefinitions), () => VerifySuitPickupBeamCurveDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        Suite(nameof(VerifyPermanentCollectibles), () => VerifyPermanentCollectibles());
        return 0;
    }
}
