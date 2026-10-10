using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks the eleven compiled Space Colony caption letters against native instruction timing, screen placement, and character tiles.</summary>
    /// <param name="rom">Cartridge address space containing the caption instruction and drawing data.</param>
    private static void VerifySpaceColonyCaption(ISnesAddressSpace rom)
    {
        int ReadWord(int address) => rom.ReadByte(address) | rom.ReadByte(address + 1) << 8;
        AssertEqual(11, SpaceColonyCaptionDefinitions.LetterCount, "native caption length");
        for (int index = 0; index < 11; index++)
        {
            int instruction = 0x8cd629 + 6 * index;
            var actual = SpaceColonyCaptionDefinitions.Letter(index);
            AssertEqual(16, ReadWord(instruction), "native letter duration unchanged");
            AssertEqual((int)rom.ReadByte(instruction + 2), actual.Column, "native caption column");
            AssertEqual((byte)24, rom.ReadByte(instruction + 3), "native caption row unchanged");
            int drawing = 0x8c0000 | ReadWord(instruction + 4);
            AssertEqual(0x0101, ReadWord(drawing + 2), "one-cell character drawing");
            AssertEqual((ushort)ReadWord(drawing + 4), actual.Tile, "native caption tile and attributes");
        }
        foreach (int invalid in new[] { -1, 11, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => SpaceColonyCaptionDefinitions.Letter(invalid), "caption domain");
    }
}
