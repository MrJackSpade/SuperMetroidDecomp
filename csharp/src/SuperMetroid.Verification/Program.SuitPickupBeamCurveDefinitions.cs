using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for suit pickup beam curve definitions")]
    private static void VerifySuitPickupBeamCurveDefinitions(SuperMetroidAddressSpace rom)
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

    private static CartridgeReadGuard NewSuitPickupBeamCurveReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address >= SuitPickupBeamCurveDefinitions.NativeCurveAddress &&
                address < SuitPickupBeamCurveDefinitions.NativeCurveAddress +
                    SuitPickupBeamCurveDefinitions.OffsetCount,
                address => $"Suit-pickup transformation attempted curve read ${address:X6}.");
}
