using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.RenderVerification.PpuFeatures;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction.Artwork;

/// <summary>Compares Direct3D on the graphics device and the WARP reference for color windows, subscreen math, and message masks.</summary>
internal static class ColorWindowSmokeTests
{
    /// <summary>Exercises exact pixel comparisons across scanline windows and representative BG and message geometry.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var random = new Random(32114);
        var vram = new byte[SnesVram.ByteCount]; random.NextBytes(vram);
        RgbColor[] palette = Enumerable.Range(0, 256).Select(_ => RgbColor.FromWord((ushort)random.Next(32768))).ToArray();
        var memory = FramePaint.FromNativeGameplay(palette, []);
        SuperMetroid.Core.Assets.LayeredPicture bg3 = FxOverlayFixtures.PlaneFromVram(vram, palette, 0x7ff8);
        var background = new PicturedPlaneLayer(VramPictureFixtures.Plane(vram, palette, 0x4000, 0x6000, TileInkDepth.SixteenColours, new Extent16<CharacterCells>(32, 32)),
            new Offset16<PlanePixels>(3, 11));
        PicturedPlane subscreen = VramPictureFixtures.Plane(vram, palette, 0x5800, 0x4000, TileInkDepth.FourColours, new Extent16<CharacterCells>(32, 32));
        int count = 0;
        // Seeded windows of every shape kind (#1468), whole and stepped colours.
        for (int sample = 0; sample < 64; sample++)
        {
            int firstEdge = random.Next(RotationAngle.StepsPerTurn);
            int left = random.Next(-32, 256), top = random.Next(-32, 224);
            WindowShape[] shapes =
            [
                new WindowCone(Offset16<ScreenPixels>.Of(random.Next(-64, 320), random.Next(-64, 288)),
                    new TrigonometryAngle(firstEdge), TrigonometryAngle.Wrapped(firstEdge + random.Next(RotationAngle.StepsPerTurn / 2))),
                WindowRectangle.Spanning(left, top, left + random.Next(64), top + random.Next(64)),
                new WindowOval(Offset16<ScreenPixels>.Of(random.Next(-64, 320), random.Next(-64, 288)), random.Next(64),
                    new Extent16<ScreenPixels>(checked((ushort)(1 + random.Next(128))), checked((ushort)(1 + random.Next(128))))),
            ];
            int firstLine = random.Next(224);
            var window = new ScreenWindow(shapes.AsSpan(0, 1 + sample % shapes.Length), firstLine, random.Next(firstLine, 225));
            var colour = new FadeLevelColour(random.Next(32), random.Next(32), random.Next(32)).ToColour();
            Check(new ColourAddWindowLayer(window, colour, sample % 2 == 0 ? null
                : new LineColourSteps(random.Next(224), 1 + random.Next(16), 1 + random.Next(15), random.Next(-1, 2))));
        }
        // Ovals whose edges pass exactly through pixels (#2001): every device lights exactly the pixels within or on each
        // oval, among them the Power Bomb blast's ovals around the Ceres station blast's centre, whose 181 by 135 oval's
        // left tip the graphics card once left dark, and a column with rounded sides like the suit pickup's beam.
        var dark = FramePaint.FromNative(new RgbColor[SnesCgram.ColorCount], []);
        RgbColor lit = new FadeLevelColour(31, 0, 0).ToColour();
        for (int radius = 1; radius < 256; radius++)
        {
            int tall = radius * 3 / 4;
            if (tall > 0)
                CheckOvalEdge(WindowOval.Ellipse(Offset16<ScreenPixels>.Of(193, 136),
                    new Extent16<ScreenPixels>(checked((ushort)radius), checked((ushort)tall))));
        }
        CheckOvalEdge(new WindowOval(Offset16<ScreenPixels>.Of(40, 112), 100, new Extent16<ScreenPixels>(24, 112)));
        // The haze in the tints the import installs (#1596).
        var hazeTints = new SuperMetroid.Core.Fx.Artwork.CeresHazeTints(
            SuperMetroid.AssetExtraction.Frontend.Map.SceneColourFiles.StockCeresHazeBlue,
            SuperMetroid.AssetExtraction.Frontend.Map.SceneColourFiles.StockCeresHazeRed);
        Check(GameplayEffectCapture.CaptureCeresHaze(hazeTints, false));
        Check(GameplayEffectCapture.CaptureCeresHaze(hazeTints, true));
        foreach (ushort height in new ushort[] { 32, 64 })
        foreach (int first in new[] { 0, 32, 223, 224 })
        foreach (BlendOperation operation in Enum.GetValues<BlendOperation>())
        foreach (ushort offset in new ushort[] { 0, 255, 511, ushort.MaxValue })
        {
            // Each line scrolls on both axes by a described sixteen-sample ramp (#1468).
            var math = new OverlayBlendLayer(new AtmosphereOverlay(FxOverlayFixtures.Rows(bg3, height == 32 ? 32 : 0, height),
                    new SampledScrollWave(offset, [.. Enumerable.Range(0, 16).Select(sample => -sample)], 3 * 256, 0, 0, 224)), first, operation,
                new SampledScrollWave(offset, [.. Enumerable.Range(0, 16)], 7 * 256, 0, 0, 224));
            count++;
            var frame = new DisplayPacket(0, new LayeredScene(memory,
                new DisplayLayer[] { background, math }, 15));
            PixelComparison.Verify(frame, DisplayRendering.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: BG color math {count}, {operation}, height {height}, first {first}, offset {offset}");
        }
        foreach (ushort width in new ushort[] { 32, 64 })
        foreach (ushort height in new ushort[] { 32, 64 })
        foreach (bool masked in new[] { false, true })
        {
            var coverage = masked ? new PicturedPlaneLayer(VramPictureFixtures.Plane(vram, palette, 0x7ffe, 0x6000, TileInkDepth.SixteenColours,
                new Extent16<CharacterCells>(width, height)), new Offset16<PlanePixels>(-1, 511)) : null;
            count++;
            var frame = new DisplayPacket(0, new LayeredScene(memory,
                new DisplayLayer[] { background,
                    new ScreenTintLayer(new FadeLevelColour(1, 2, 3).ToColour()), new AddedPicturedPlaneLayer(subscreen, coverage) }, 13));
            PixelComparison.Verify(frame, DisplayRendering.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: glow {count}, over a plane {masked}, geometry {width}x{height}");
        }
        for (int rows = 3; rows <= 6; rows++)
        for (int radius = 0; radius <= 24; radius++)
        {
            count++;
            var frame = new DisplayPacket(0, new LayeredScene(memory,
                new DisplayLayer[] { background,
                    new MessageBoxLayer(SuperMetroid.Core.Rooms.Backgrounds.RoomBackgroundsFixtures.SeededMessagePicture(random, palette, rows), radius) }, 13));
            PixelComparison.Verify(frame, DisplayRendering.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: message {count}, rows {rows}, radius {radius}");
        }
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {count} exact color-effect/message comparisons passed.");

        void Check(ColourAddWindowLayer windows)
        {
            count++;
            var frame = new DisplayPacket(0, new LayeredScene(memory,
                new DisplayLayer[] { background,
                    windows, new ScreenTintLayer(new FadeLevelColour(1, 2, 3).ToColour()), windows }, DisplayBrightness.Combine(13, 11)));
            PixelComparison.Verify(frame, DisplayRendering.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: color-window sample {count}");
        }

        // Draws the oval alone over a dark screen and checks each pixel is lit exactly when it lies within or on the oval.
        void CheckOvalEdge(WindowOval oval)
        {
            count++;
            var frame = new DisplayPacket(0, new LayeredScene(dark,
                new DisplayLayer[] { new ColourAddWindowLayer(ScreenWindow.Of(oval), lit) }, DisplayBrightness.Full));
            RgbaPixel[] drawn = renderer.RenderForReadback(frame);
            for (int y = 0; y < DisplayScreen.Height; y++)
            for (int x = 0; x < DisplayScreen.Width; x++)
            {
                bool shown = drawn[y * DisplayScreen.Width + x].R != 0, covered = OvalCovers(oval, x, y);
                if (shown != covered)
                    throw new InvalidOperationException($"{device.Kind}: {oval} {(shown ? "lights" : "leaves dark")} ({x},{y}), " +
                        $"which lies {(covered ? "within or on" : "outside")} its edge.");
            }
        }
    }

    /// <summary>
    /// Whether the pixel at <paramref name="x"/>, <paramref name="y"/> lies within or on <paramref name="oval"/>, worked in
    /// whole numbers: its distance beside the core and from the middle line, scaled by the radii, is at most one.
    /// </summary>
    private static bool OvalCovers(WindowOval oval, int x, int y)
    {
        Int128 dx = Math.Max(0, Math.Max(oval.CoreStart.X - x, x - (oval.CoreStart.X + oval.CoreWidth)));
        Int128 dy = y - oval.CoreStart.Y;
        Int128 rx = oval.Radii.X, ry = oval.Radii.Y;
        return dx * dx * ry * ry + dy * dy * rx * rx <= rx * rx * ry * ry;
    }
}
