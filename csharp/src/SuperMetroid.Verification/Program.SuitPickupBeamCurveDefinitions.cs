using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks all compiled suit-pickup beam contour offsets against ROM and verifies index bounds.</summary>
    /// <param name="rom">Retail address space containing the original contour curve.</param>
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

    /// <summary>Rejects runtime reads from the suit-pickup beam curve after its offsets are compiled.</summary>
    /// <param name="source">Address space that serves unrelated reads and receives writes.</param>
    private sealed class SuitPickupBeamCurveReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Number of attempted reads from the migrated beam-curve table.</summary>
        public int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes importer reads through the same curve-range check.</summary>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Blocks reads within the compiled curve and delegates all other addresses.</summary>
        /// <param name="address">Full cartridge address requested by the caller.</param>
        /// <returns>The source byte when the address is outside the curve table.</returns>
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

        /// <summary>Forwards writes unchanged to the wrapped address space.</summary>
        /// <param name="address">Full cartridge address to write.</param>
        /// <param name="value">Byte written to that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
