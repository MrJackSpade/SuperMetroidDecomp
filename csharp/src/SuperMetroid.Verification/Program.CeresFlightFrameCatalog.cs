using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCeresFlightFrameCatalog(ISnesAddressSpace rom)
    {
        // Independent native instruction operands in the published catalog order.
        int[] operands = [0xcda5, 0xce4d, 0xcc49, 0xcc51, 0xcc59, 0xcc5d];
        string[] names = ["stars", "large-asteroid", "station-under-attack", "small-asteroid", "vortex-even", "vortex-odd"];
        var frames = CeresFlightSpriteDefinitions.Frames;
        AssertEqual(operands.Length, frames.Count, "flight catalog count");
        var enumerated = frames.ToArray();
        for (int index = 0; index < operands.Length; index++)
        {
            int operand = 0x8b0000 | operands[index];
            ushort pointer = (ushort)(rom.ReadByte(operand) | rom.ReadByte(operand + 1) << 8);
            int record = 0x8c0000 | pointer;
            int count = rom.ReadByte(record) | rom.ReadByte(record + 1) << 8;
            AssertEqual(pointer, frames[index].Pointer, "native flight frame selection");
            AssertEqual(count, frames[index].StockPartCount, "native flight OAM count");
            AssertEqual(names[index], frames[index].Name, "published flight frame key");
            AssertEqual(frames[index], enumerated[index], "flight catalog enumeration order");
        }
        foreach (int invalid in new[] { -1, frames.Count, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = frames[invalid], "flight catalog bounds");
    }
}
