using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Verification.Hardware;

/// <summary>Checks the SNES color value type across word encoding, channel operations, CGRAM, and a production fade.</summary>
internal static class Bgr555DomainVerification
{
    /// <summary>
    /// #627: SNES colors are the packed value type <see cref="Bgr555"/>. Every 15-bit word
    /// round-trips with its channels, bit 15 is rejected except at the CGRAM data port, channel
    /// construction is bounded, RGBA expansion replicates bits, channel operations compose, and
    /// older debugger payloads of color words restore through the shape-preserving rule.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyBgr555Domain()
    {
        for (int word = 0; word <= 0x7fff; word++)
        {
            Bgr555 color = Bgr555.FromWord((ushort)word);
            AssertEqual(word, color.ToWord(), $"word ${word:X4} round-trips");
            AssertEqual(word & 31, color.Red, $"word ${word:X4} red");
            AssertEqual(word >> 5 & 31, color.Green, $"word ${word:X4} green");
            AssertEqual(word >> 10 & 31, color.Blue, $"word ${word:X4} blue");
            AssertEqual(color, new Bgr555(color.Red, color.Green, color.Blue), $"word ${word:X4} channel construction");
            AssertEqual(color, Bgr555.FromCgramPortWord((ushort)(word | 0x8000)), $"CGRAM port drops bit 15 of ${word:X4}");
            AssertEqual(color.Red, color[ColorChannel.Red], "red indexer");
            AssertEqual(color.Green, color[ColorChannel.Green], "green indexer");
            AssertEqual(color.Blue, color[ColorChannel.Blue], "blue indexer");
        }
        for (int word = 0x8000; word <= 0xffff; word += 0x111)
            AssertThrows<ArgumentOutOfRangeException>(() => Bgr555.FromWord((ushort)word), $"stored word ${word:X4} sets bit 15");
        AssertThrows<ArgumentOutOfRangeException>(() => new Bgr555(32, 0, 0), "red above 31");
        AssertThrows<ArgumentOutOfRangeException>(() => new Bgr555(0, -1, 0), "negative green");
        AssertThrows<ArgumentOutOfRangeException>(() => new Bgr555(0, 0, 32), "blue above 31");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = Bgr555.White[(ColorChannel)3], "undefined channel");
        AssertEqual(Bgr555.White, Bgr555.FromWord(0x7fff), "white is every channel at 31");
        AssertEqual(Bgr555.Black, Bgr555.FromWord(0), "black is every channel zero");

        for (int channel = 0; channel <= 31; channel++)
        {
            Rgba32 rgba = new Bgr555(channel, channel, channel).ToRgba32();
            int expanded = channel << 3 | channel >> 2;
            AssertEqual(expanded, rgba.R, $"channel {channel} expands by bit replication");
            AssertTrue(rgba.R == rgba.G && rgba.G == rgba.B && rgba.A == 255, $"channel {channel} expands every component opaquely");
        }
        AssertEqual(new Bgr555(0, 31, 7), Bgr555.Saturating(-4, 40, 7), "saturating construction clamps each channel");
        Bgr555 sample = new(3, 9, 27);
        AssertEqual(new Bgr555(6, 18, 27), sample.Map((channel, value) => channel == ColorChannel.Blue ? value : value * 2), "Map sees each channel");
        AssertEqual(new Bgr555(4, 9, 27), sample.Zip(new Bgr555(5, 9, 27), (_, a, b) => (a + b) / 2), "Zip pairs channels");
        AssertEqual(new Bgr555(3, 9, 1), sample.With(ColorChannel.Blue, 1), "With replaces one channel");
        AssertEqual(new Bgr555(8, 9, 27), sample.WithRed(8), "WithRed");
        AssertThrows<ArgumentOutOfRangeException>(() => sample.Map((_, value) => value + 31), "Map results must be channels");

        // CGRAM: the port boundary keeps fifteen bits, typed writes store exactly.
        var cgram = new SnesCgram();
        cgram.LoadBytes([0xff, 0xff, 0x34, 0x12]);
        AssertEqual(Bgr555.White, cgram.Colors[0], "CGRAM byte load drops bit 15");
        AssertEqual(0x1234, cgram.Colors[1], "CGRAM byte load keeps the color word");
        cgram.SetColor(2, sample);
        AssertEqual(sample, cgram.Colors[2], "typed CGRAM write");
        AssertEqual(sample.ToRgba32(), cgram.GetRgba(2), "CGRAM RGBA is the color's expansion");

        // A production fade on typed colors reaches its exact target (native $82:DA02 ramp).
        var colours = new ScreenPalette();
        colours.Load(cgram.Colors);
        var target = new ColourImage(colours.All);
        target.Fill(Bgr555.White);
        var fade = new GradualPaletteTransition(target, denominator: 6, new GradualColorChangeCounter());
        for (int step = 0; step < 16 && !fade.Step(); step++) { }
        AssertTrue(colours.All.Colors.IndexOfAnyExcept(Bgr555.White) < 0, "typed palette fade installs its exact target");

        Console.WriteLine("Bgr555 domain: all 32768 words, port masking, channel bounds, RGBA expansion, operations, and CGRAM agree.");
    }
}
