using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Verification.Rooms.Scrolling;

/// <summary>
/// Checks the background's parallax rate (#2046): a typed fraction of the foreground's movement with no parity rule, the
/// scroll byte's low bit split off at import, and the background's position trailing the foreground exactly as the
/// cartridge's multiply by the byte's even part over 256 did.
/// </summary>
internal static class ParallaxRateVerification
{
    /// <summary>The largest foreground position on an axis.</summary>
    private const int LargestForegroundPosition = 65535;

    /// <summary>
    /// A rate is any whole number of 256ths strictly between nothing and the whole, equal however it is written; every
    /// scroll byte decodes to its kind, an odd byte's low bit choosing only what the background shows; and for every
    /// parallax byte and every foreground position the background lands where the cartridge's multiply put it.
    /// </summary>
    internal static void VerifyParallaxRate()
    {
        AssertEqual(ParallaxRate.Of(1, 2), ParallaxRate.Of(128, 256), "a rate is the same fraction however it is written");
        AssertEqual("3/4", ParallaxRate.Of(192, 256).ToString(), "a rate reads in lowest terms");
        AssertEqual(1, BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(1, 256)).BackgroundPositionFor(256),
            "an odd number of 256ths is a rate: no parity rule");
        AssertThrows<ArgumentOutOfRangeException>(() => ParallaxRate.Of(0, 4), "no movement is not a parallax rate");
        AssertThrows<ArgumentOutOfRangeException>(() => ParallaxRate.Of(4, 4), "the foreground's whole movement is not a parallax rate");
        AssertThrows<ArgumentOutOfRangeException>(() => ParallaxRate.Of(1, 3), "a rate is a whole number of 256ths");

        AssertEqual(BackgroundAxisScroll.FollowsForeground, BackgroundScrollCodec.Decode(0, 0).Horizontal, "byte zero copies the foreground");
        AssertEqual(BackgroundAxisScroll.Fixed, BackgroundScrollCodec.Decode(1, 1).Horizontal, "byte one fixes the axis");
        AssertEqual((int?)null, BackgroundAxisScroll.Fixed.BackgroundPositionFor(0x1234), "a fixed axis keeps the background where it is");
        AssertEqual(0x1234, BackgroundAxisScroll.FollowsForeground.BackgroundPositionFor(0x1234), "a following axis copies the foreground");
        for (int mode = 2; mode <= byte.MaxValue; mode++)
        {
            BackgroundScrollModes decoded = BackgroundScrollCodec.Decode((byte)mode, (byte)mode);
            bool streams = mode % 2 == 0;
            AssertEqual(streams ? RoomBackgroundSource.Level : RoomBackgroundSource.Placed, decoded.Source,
                $"scroll byte {mode}'s low bit chooses what the background shows");
            AssertEqual(BackgroundAxisScroll.ParallaxAt(ParallaxRate.Of(mode & ~1, 256)), decoded.Horizontal,
                $"scroll byte {mode} trails at its even part over 256");
            int multiplier = mode & ~1;
            for (int foreground = 0; foreground <= LargestForegroundPosition; foreground++)
            {
                int expected = foreground * multiplier >> 8;
                if (decoded.Horizontal.BackgroundPositionFor(foreground) != expected)
                    throw new InvalidOperationException(
                        $"Scroll byte {mode} places the background at {decoded.Horizontal.BackgroundPositionFor(foreground)} for foreground {foreground}, not {expected}.");
            }
        }

        Console.WriteLine("  Parallax rate: typed fractions with no parity rule; every scroll byte and foreground position trails as the cartridge's multiply did.");
    }
}
