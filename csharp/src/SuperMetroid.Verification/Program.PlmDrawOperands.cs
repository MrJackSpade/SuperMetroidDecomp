using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction;

internal static partial class Program
{
    /// <summary>Looks up a PLM program word by address in a compiled definition catalog.</summary>
    /// <param name="address">Program-bank address of the word to retrieve.</param>
    /// <param name="value">Receives the compiled word when the catalog contains the address.</param>
    /// <returns><see langword="true"/> when a compiled word is available at <paramref name="address"/>.</returns>
    private delegate bool TryCompiledPlmWord(ushort address, out ushort value);

    /// <summary>Reads a little-endian word from the retail PLM program bank at the requested offset.</summary>
    /// <param name="source">Imported cartridge address space containing the original PLM program bytes.</param>
    /// <param name="offset">Offset within the PLM program bank where the word begins.</param>
    /// <returns>The two bytes at the offset combined as an unsigned word.</returns>
    private static ushort ReadImportedPlmWord(SuperMetroidAddressSpace source, ushort offset)
    {
        int bank = RoomPlmMemoryLayout.ProgramBank << 16;
        return (ushort)(source.ReadByte(bank | offset) |
            source.ReadByte(bank | unchecked((ushort)(offset + sizeof(byte)))) << 8);
    }

    /// <summary>
    /// #1161: confirm the 107 statically identified missing operands against the
    /// pinned import fixture. This neither runs gameplay nor discovers new tasks.
    /// Control enumeration intentionally remains distinct from draw operands.
    /// </summary>
    private static void ConfirmCompiledPlmDrawOperands(IEnumerable<ushort> controls,
        TryCompiledPlmWord readControl, TryCompiledPlmWord readDraw,
        Func<ushort, ushort> readNative, int expectedCount, string family)
    {
        int count = 0;
        foreach (ushort address in controls)
        {
            AssertTrue(readControl(address, out ushort control), $"{family} declared control exists");
            if ((control & RoomPlmMemoryLayoutTooling.RoutineWordMask) != 0) continue;
            ushort operand = checked((ushort)(address + sizeof(ushort)));
            AssertTrue(readDraw(operand, out ushort draw), $"{family} timer ${address:X4} has a compiled draw operand");
            AssertEqual(readNative(operand), draw, $"{family} draw operand ${operand:X4} matches pinned import");
            count++;
        }
        AssertEqual(expectedCount, count, $"{family} corrected draw operand count");
    }
}
