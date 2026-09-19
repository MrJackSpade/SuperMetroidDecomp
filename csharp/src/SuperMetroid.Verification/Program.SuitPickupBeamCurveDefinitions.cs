using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySuitPickupBeamCurveDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < SuitPickupBeamCurveDefinitions.OffsetCount; index++)
        {
            AssertEqual(
                rom.ReadByte(SuitPickupBeamCurveDefinitions.NativeCurveAddress + index),
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

    private sealed class SuitPickupBeamCurveReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (address >= SuitPickupBeamCurveDefinitions.NativeCurveAddress &&
                address < SuitPickupBeamCurveDefinitions.NativeCurveAddress +
                    SuitPickupBeamCurveDefinitions.OffsetCount)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Suit-pickup transformation attempted curve read ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
