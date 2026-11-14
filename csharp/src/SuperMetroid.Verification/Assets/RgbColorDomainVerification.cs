using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Frontend;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Assets;

/// <summary>
/// Checks the plain colour, the game's fade scale and the import's colour word (#627, #1544): every stored word decodes
/// to its levels and shows each level spread by bit replication, so pictures imported from the cartridge show as before;
/// a plain colour's channels are bounded; levels round-trip, a colour off the scale has no levels and no stored word, and
/// the fades step on the levels as the game does.
/// </summary>
internal static class RgbColorDomainVerification
{
    /// <summary>
    /// #1544: the import's colour word, the levels and the plain colour agree. Every 15-bit word round-trips through its
    /// plain colour and decodes to its levels; bit 15 is rejected except where it is dropped; each level shows its
    /// bit-replicated eight-bit value; a colour off the scale has no levels and no stored word; plain colour channels are
    /// 0-255; a level fade, the opening's cross-fade and the white wash show the colours of the levels they step to.
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
            AssertEqual(expanded, new FadeLevelColour(level, 0, 0).ToColour().Red, $"level {level} spreads by bit replication");
            AssertTrue(rgba.R == expanded && rgba.G == expanded && rgba.B == expanded && rgba.A == 255,
                $"level {level} shows its plain value in every component, opaquely");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => new FadeLevelColour(32, 0, 0), "red level above full");
        AssertThrows<ArgumentOutOfRangeException>(() => new FadeLevelColour(0, -1, 0), "negative green level");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = RgbColor.White.FadeLevels[(ColorChannel)3], "undefined channel");

        // A plain colour off the scale has no levels and no stored word: every colour the game shows lies on the scale.
        var levelChannels = new HashSet<int>(Enumerable.Range(0, FadeLevelColour.FullLevel + 1).Select(level => new FadeLevelColour(level, 0, 0).ToColour().Red));
        for (int channel = 0; channel <= RgbColor.FullChannel; channel++)
        {
            if (levelChannels.Contains(channel))
                continue;
            var plain = new RgbColor(channel, 0, 0);
            AssertThrows<ArgumentException>(() => _ = plain.FadeLevels, $"channel {channel} has no level");
            AssertThrows<ArgumentException>(() => _ = plain.ToWord(), $"channel {channel} has no stored word");
        }

        // Plain colours: bounded channels.
        AssertThrows<ArgumentOutOfRangeException>(() => new RgbColor(256, 0, 0), "red above full");
        AssertThrows<ArgumentOutOfRangeException>(() => new RgbColor(0, -1, 0), "negative green");

        // The fades step on the levels: half of full white's level 31 is level 15, which shows 123, not half of 255.
        AssertEqual(new FadeLevelColour(15, 15, 15).ToColour(), new ColourLevel(1, 2).Apply(RgbColor.White),
            "the cross-fade halves each level, rounding down");
        AssertEqual(new RgbColor(123, 123, 123), new ColourLevel(1, 2).Apply(RgbColor.White), "level 15 shows 123");
        AssertEqual(new RgbColor(33, 33, 33), WhiteWash.Colour(4), "a white wash with four steps left shows level four");

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

        // A fade's length is any count of steps (#1627): one longer than a word's range keeps its first steps' slow
        // movement, reaches its target on the step after its last and completes on the call after that.
        const int longFade = 70000;
        AssertEqual(longFade, new GradualFadeSteps(longFade, new SharedFadeStepCount()).Denominator, "a fade keeps a length past a word's range");
        AssertEqual(0, GradualColourFade.Component(1, 0, 31, longFade), "the long fade's first moving step moves less than a level");
        AssertEqual(31, GradualColourFade.Component(longFade + 1, 0, 31, longFade), "the step after the long fade's last reaches the target");
        AssertEqual(RgbColor.White, new GradualColourFade(RgbColor.White, GradualFadeDirection.Toward, longFade,
            [.. Enumerable.Range(0, longFade + 2)]).Apply(RgbColor.Black), "the long fade ends on its target");
        AssertThrows<ArgumentOutOfRangeException>(() => new GradualFadeSteps(0, new SharedFadeStepCount()), "a fade takes at least one step");

        Console.WriteLine("RgbColor domain: all 32768 words, levels, off-scale rejection, plain channel bounds, level fades, and CGRAM agree.");
    }
}
