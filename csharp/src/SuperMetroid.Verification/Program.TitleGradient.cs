using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyTitleGradientTables(SuperMetroidAddressSpace bus)
    {
        // Independently transcribed boundaries from $8C:BC7D and $88:EB95.
        var lines = TitleGradient.Decode(bus, 0);
        AssertEqual(new TitleGradientLine(15, 15, 15, 0xa1), lines[0], "title gradient starts subtracting fifteen");
        AssertEqual(new TitleGradientLine(14, 14, 14, 0xa1), lines[4], "title gradient advances after four scanlines");
        AssertEqual(new TitleGradientLine(0, 0, 0, 0xa1), lines[121], "last subtractive title line");
        AssertEqual(new TitleGradientLine(0, 0, 0, 0x31), lines[122], "first additive title line");
        AssertEqual(new TitleGradientLine(0, 1, 1, 0x31), lines[132], "lower title cyan band begins");
        AssertEqual(new TitleGradientLine(0, 2, 2, 0x31), lines[144], "second lower cyan band begins");
        for (ushort zoom = 0; zoom < 256; zoom++)
        {
            var actual = TitleGradient.Decode(bus, zoom);
            var nibble = TitleGradient.Decode(bus, (ushort)(zoom & 0xf0));
            if (!actual.AsSpan().SequenceEqual(nibble))
                throw new InvalidDataException("Title gradient index used low zoom bits.");
            AssertEqual((byte)0xa1, actual[121].Control, "zoom preserves subtractive boundary");
            AssertEqual((byte)0x31, actual[122].Control, "zoom preserves additive boundary");
        }
        Console.WriteLine("  Title gradient: 256 zoom selections, native cyan bands and subtract/add boundary agree.");
    }
}
