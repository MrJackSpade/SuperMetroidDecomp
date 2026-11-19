using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.TestSupport.FrameRendering;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rendering;

using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks that the display's background scroll is signed (#2305): a background picture that repeats shows, from a scroll
/// left of and above its origin, what it shows one repeat of its own size further on, so the capture adds plain offsets
/// and the backend does the repeating.
/// </summary>
internal static class SignedBackgroundScrollVerification
{
    /// <summary>
    /// Draws a seeded scrolling sky and a seeded library background at scrolls left of and above their origins, and compares
    /// each frame with the frame drawn one repeat of the picture's own size further right and down.
    /// </summary>
    internal static void VerifySignedBackgroundScroll()
    {
        var random = new Random(2305);
        RgbColor[] colours = [.. Enumerable.Range(0, 256).Select(_ => RgbColor.FromWord((ushort)random.Next(32768)))];
        RoomBackgroundImagePlane sky = RoomBackgroundsFixtures.SeededSky(random, colours);
        RoomBackgroundImagePlane library = RoomBackgroundsFixtures.SeededBackground(random, colours);
        RoomBackgrounds blank = RoomBackgroundsFixtures.Blank();
        AssertRepeats("the scrolling sky", blank with { Background = new SuperMetroid.Core.Gameplay.SkyBackground(new SkyPictures(sky)) }, sky.Area, colours);
        AssertRepeats("a library background", blank with { Background = new SuperMetroid.Core.Gameplay.LibraryImageBackground(library) }, library.Area, colours);
        Console.WriteLine("Signed background scroll: the sky and a library background repeat by their own size above and left of their origins.");
    }

    /// <summary>
    /// Asserts that <paramref name="rooms"/>' background, scrolled left of and above its origin, shows what it shows one
    /// <paramref name="period"/> further on, and that the frames show the picture rather than nothing.
    /// </summary>
    private static void AssertRepeats(string name, RoomBackgrounds rooms, Extent16<BackgroundPixels> period, RgbColor[] colours)
    {
        foreach (int before in new[] { 1, 37, 200 })
        {
            RgbaPixel[] negative = Render(rooms, new Offset16<BackgroundPixels>(-before, -before), colours);
            RgbaPixel[] repeated = Render(rooms, new Offset16<BackgroundPixels>(period.X - before, period.Y - before), colours);
            AssertTrue(negative.Distinct().Count() > 1, $"{name} scrolled {before} above and left of its origin shows its picture");
            for (int pixel = 0; pixel < negative.Length; pixel++)
                AssertEqual(repeated[pixel], negative[pixel],
                    $"{name} scrolled {before} above and left of its origin, pixel {pixel % 256},{pixel / 256}");
        }
    }

    /// <summary>The pixels of the background alone, scrolled to <paramref name="scroll"/>.</summary>
    private static RgbaPixel[] Render(RoomBackgrounds rooms, Offset16<BackgroundPixels> scroll, RgbColor[] colours) =>
        DisplayRendering.Render(new LayeredScene(FramePaint.FromNativeGameplay(colours, []),
            new DisplayLayer[] { new GameplaySceneLayer(new GameplaySceneView(Offset16<RoomPixels>.Zero, scroll, ScreenLayers.Background), rooms) },
            DisplayBrightness.Full));

    /// <summary>Runs the signed background scroll check.</summary>
    [VerificationCommand("--signed-background-scroll")]
    internal static int SignedBackgroundScrollCommand()
    {
        Suite(nameof(VerifySignedBackgroundScroll), () => VerifySignedBackgroundScroll());
        return 0;
    }
}
