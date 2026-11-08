using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Assets;

/// <summary>
/// Checks the plain colour, the game's colour levels and the import's colour word (#627, #1544): every stored word decodes
/// to its levels and shows each level spread by bit replication, so pictures imported from the cartridge show as before;
/// a plain colour's channels are bounded and its blend, scale and addition are plain fractions; levels round-trip and the
/// nearest levels of any plain colour are the closest the scale comes to it.
/// </summary>
internal static class RgbColorDomainVerification
{
    /// <summary>
    /// #1544: the import's colour word, the levels and the plain colour agree. Every 15-bit word round-trips through its
    /// plain colour and decodes to its levels; bit 15 is rejected except where it is dropped; each level shows its
    /// bit-replicated eight-bit value; a colour off the scale is rejected by its exact levels and stored at its nearest;
    /// plain colour channels are 0-255 and the colour operations are the fractions they name.
    /// </summary>
    internal static void VerifyRgbColorDomain()
    {
        for (int word = 0; word <= 0x7fff; word++)
        {
            RgbColor color = RgbColor.FromWord((ushort)word);
            AssertEqual(word, color.ToWord(), $"word ${word:X4} round-trips");
            FadeLevelColour levels = color.FadeLevels;
            AssertEqual(word & 31, levels.Red, $"word ${word:X4} red level");
            AssertEqual(word >> 5 & 31, levels.Green, $"word ${word:X4} green level");
            AssertEqual(word >> 10 & 31, levels.Blue, $"word ${word:X4} blue level");
            AssertEqual(color, levels.ToColour(), $"word ${word:X4} levels show its colour");
            AssertEqual(levels, FadeLevelColour.Nearest(color), $"word ${word:X4} is its own nearest levels");
            AssertEqual(color, RgbColor.FromWordIgnoringTopBit((ushort)(word | 0x8000)), $"a dropped bit 15 leaves ${word:X4}");
        }
        for (int word = 0x8000; word <= 0xffff; word += 0x111)
            AssertThrows<ArgumentOutOfRangeException>(() => RgbColor.FromWord((ushort)word), $"stored word ${word:X4} sets bit 15");
        AssertEqual(RgbColor.White, RgbColor.FromWord(0x7fff), "white is every level full");
        AssertEqual(RgbColor.Black, RgbColor.FromWord(0), "black is every level none");

        // Each level shows the eight-bit value bit replication gives it, so imported pictures keep their pixels.
        for (int level = 0; level <= FadeLevelColour.FullLevel; level++)
        {
            RgbaPixel rgba = new FadeLevelColour(level, level, level).ToColour().ToRgbaPixel();
            int expanded = level << 3 | level >> 2;
            AssertEqual(expanded, FadeLevelColour.ChannelOf(level), $"level {level} spreads by bit replication");
            AssertTrue(rgba.R == expanded && rgba.G == expanded && rgba.B == expanded && rgba.A == 255,
                $"level {level} shows its plain value in every component, opaquely");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => new FadeLevelColour(32, 0, 0), "red level above full");
        AssertThrows<ArgumentOutOfRangeException>(() => new FadeLevelColour(0, -1, 0), "negative green level");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = RgbColor.White.FadeLevels[(ColorChannel)3], "undefined channel");

        // A plain colour off the scale has no exact levels and is stored at its nearest.
        for (int channel = 0; channel <= RgbColor.FullChannel; channel++)
        {
            var plain = new RgbColor(channel, 0, 0);
            int nearest = FadeLevelColour.Nearest(plain).Red;
            for (int level = 0; level <= FadeLevelColour.FullLevel; level++)
                AssertTrue(Math.Abs(FadeLevelColour.ChannelOf(level) - channel) >= Math.Abs(FadeLevelColour.ChannelOf(nearest) - channel),
                    $"channel {channel}'s nearest level {nearest} is no farther than level {level}");
            AssertEqual(nearest, RgbColor.FromWord(plain.ToWord()).FadeLevels.Red, $"channel {channel} is stored at its nearest level");
            if (FadeLevelColour.ChannelOf(nearest) != channel)
                AssertThrows<ArgumentException>(() => _ = plain.FadeLevels, $"channel {channel} has no exact level");
        }

        // Plain colours: bounded channels and plain fractions.
        AssertThrows<ArgumentOutOfRangeException>(() => new RgbColor(256, 0, 0), "red above full");
        AssertThrows<ArgumentOutOfRangeException>(() => new RgbColor(0, -1, 0), "negative green");
        var from = new RgbColor(10, 100, 250);
        var to = new RgbColor(250, 0, 10);
        AssertEqual(from, from.Blend(to, 0, 7), "a blend none of the way is the colour");
        AssertEqual(to, from.Blend(to, 7, 7), "a blend all of the way is the far colour");
        AssertEqual(new RgbColor(130, 50, 130), from.Blend(to, 1, 2), "a blend halfway");
        AssertEqual(new RgbColor(44, 86, 216), from.Blend(to, 1, 7), "a blend rounds toward the colour");
        AssertEqual(RgbColor.Black, from.Scaled(0, 5), "a colour scaled to none is black");
        AssertEqual(new RgbColor(4, 40, 100), from.Scaled(2, 5), "a scaled colour rounds down");
        AssertEqual(new RgbColor(255, 100, 255), from.Plus(to), "an added colour holds each channel at full");

        // CGRAM: the port boundary keeps fifteen bits, typed writes store exactly.
        RgbColor sample = new FadeLevelColour(3, 9, 27).ToColour();
        var cgram = new SnesCgram();
        cgram.LoadBytes([0xff, 0xff, 0x34, 0x12]);
        AssertEqual(RgbColor.White, cgram.Colors[0], "CGRAM byte load drops bit 15");
        AssertEqual(0x1234, cgram.Colors[1], "CGRAM byte load keeps the color word");
        cgram.SetColor(2, sample);
        AssertEqual(sample, cgram.Colors[2], "typed CGRAM write");
        AssertEqual(sample.ToRgbaPixel(), cgram.GetRgba(2), "CGRAM RGBA is the color's pixel");

        // A production fade toward a colour shows its exact target once it completes (native $82:DA02 ramp), whatever
        // colour it started from (#1512).
        var fade = new GradualFadeSteps(denominator: 6, new SharedFadeStepCount());
        for (int step = 0; step < 16 && !fade.Step(); step++) { }
        AssertTrue(cgram.Colors.ToArray().All(colour => fade.Toward(RgbColor.White).Apply(colour) == RgbColor.White),
            "typed gradual fade shows its exact target");

        Console.WriteLine("RgbColor domain: all 32768 words, levels, nearest levels, plain channel bounds, colour operations, and CGRAM agree.");
    }
}
