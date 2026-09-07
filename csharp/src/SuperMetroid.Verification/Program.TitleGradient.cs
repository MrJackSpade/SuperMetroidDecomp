using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;

internal static partial class Program
{
    private static void VerifyTitleGradientTables(SuperMetroidAddressSpace bus)
    {
        VerifyTitleGradientObjectEligibility();
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

    private static void VerifyTitleGradientObjectEligibility()
    {
        var oam = new OamBuffer();
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        byte[] tile = new byte[32];
        for (int row = 0; row < 8; row++) tile[row * 2] = 255;
        vram.LoadBytes(0, tile);
        var pixels = new Rgba32[256 * 224];
        var priorities = new byte[pixels.Length];
        var palettes = new byte[pixels.Length];
        var color = new Rgba32(0, 0, 0, 255);
        var additive = new TitleGradientLine(0, 1, 1, 0x31);
        for (byte palette = 0; palette < 8; palette++)
        {
            oam.BeginFrame();
            oam.AddRawSmallSprite(0, 0, (ushort)(palette << 9));
            oam.AddRawSmallSprite(0, 0, (ushort)((7 - palette) << 9));
            oam.FinalizeFrame();
            SnesObjRenderer.RenderResolved(oam, vram, cgram, 0, pixels, priorities, palettes: palettes);
            AssertEqual(palette, palettes[0], "winning OBJ palette survives equal-color overlap");
            AssertEqual(byte.MaxValue, palettes[8], "transparent pixel has no palette owner");
            AssertEqual(palette < 4 ? color : new Rgba32(0, 8, 8, 255),
                TitleGradientColorMath.Apply(color, additive, palettes[0]), "title OBJ palette eligibility");
            AssertEqual(color, TitleGradientColorMath.Apply(color, new(15, 15, 15, 0xa1), palette),
                "upper title subtraction excludes every OBJ palette");
        }
        AssertEqual(new Rgba32(247, 247, 247, 255),
            TitleGradientColorMath.Apply(new(255, 255, 255, 255), new(1, 1, 1, 0xa1)), "native five-bit subtraction");
    }
}
