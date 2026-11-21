using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.RenderVerification.PpuFeatures;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Compatibility;

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
        // Every cone the game draws (#2009): each aim with each half-width the X-ray scope reaches, which hold the Morph
        // Ball eye beam's and Mother Brain's rainbow beam's, from the middle of the screen; and cones from the farthest
        // apexes a cone takes, whose offsets are the largest its whole-number test meets. Every device lights exactly
        // the pixels each cone's rule covers.
        var cones = new List<WindowCone>();
        TrigonometryAngle widest = XrayScopeDefinitions.Cone.WidestHalfWidth.ToTrigonometryAngle();
        for (TrigonometryAngle halfWidth = TrigonometryAngle.Zero; halfWidth <= widest; halfWidth = halfWidth.TurnedBySteps(1))
        for (int step = 0; step < RotationAngle.StepsPerTurn; step++)
        {
            var aim = new TrigonometryAngle(step);
            cones.Add(new WindowCone(Offset16<ScreenPixels>.Of(ScreenGeometry.ScreenWidthPixels / 2, ScreenGeometry.ScreenHeightPixels / 2),
                aim - halfWidth, aim + halfWidth));
        }
        foreach (Offset16<ScreenPixels> apex in new[] { Offset16<ScreenPixels>.Of(short.MinValue, short.MinValue),
            Offset16<ScreenPixels>.Of(short.MaxValue, short.MinValue), Offset16<ScreenPixels>.Of(short.MinValue, short.MaxValue),
            Offset16<ScreenPixels>.Of(short.MaxValue, short.MaxValue) })
        {
            // One frame of the widest cones, aimed two steps apart either side of the way to the screen's middle, so
            // some cover the screen and some miss it.
            TrigonometryAngle toScreen = TowardScreenMiddle(apex);
            for (int mark = 0; mark < ConeMarks; mark++)
            {
                TrigonometryAngle aim = toScreen.TurnedBySteps(2 * mark - (ConeMarks - 1));
                cones.Add(new WindowCone(apex, aim - widest, aim + widest));
            }
        }
        for (int first = 0; first < cones.Count; first += ConeMarks)
            CheckConeEdges(cones.GetRange(first, Math.Min(ConeMarks, cones.Count - first)));
        // The haze in the tints the import installs (#1596).
        var hazeTints = new SuperMetroid.Core.Fx.Artwork.CeresHazeTints(
            SuperMetroid.AssetExtraction.Frontend.Map.SceneColourFiles.StockCeresHazeBlue,
            SuperMetroid.AssetExtraction.Frontend.Map.SceneColourFiles.StockCeresHazeRed);
        Check(GameplayEffectCapture.CaptureCeresHaze(hazeTints, CeresHazeTint.WhileRidleyLives, new CeresHazeStrength(CeresHazeRenderDefinitions.MaximumComponent)));
        Check(GameplayEffectCapture.CaptureCeresHaze(hazeTints, CeresHazeTint.AfterRidleyDies, new CeresHazeStrength(CeresHazeRenderDefinitions.MaximumComponent)));
        foreach (ushort height in new ushort[] { 32, 64 })
        foreach (int first in new[] { 0, 32, 223, 224 })
        foreach (BlendOperation operation in Enum.GetValues<BlendOperation>())
        foreach (ushort offset in new ushort[] { 0, 255, 511, ushort.MaxValue })
        {
            // Each line scrolls on both axes by a described sixteen-sample ramp (#1468).
            var math = new OverlayBlendLayer(new AtmosphereOverlay(FxOverlayFixtures.Rows(bg3, height == 32 ? 32 : 0, height),
                    new SampledScrollWave(offset, [.. Enumerable.Range(0, 16).Select(sample => -3 * sample)], 0, 0, 224)), first, operation,
                new SampledScrollWave(offset, [.. Enumerable.Range(0, 16).Select(sample => 7 * sample)], 0, 0, 224));
            count++;
            var frame = new DisplayPacket(0, new LayeredScene(memory,
                new DisplayLayer[] { background, math }, DisplayBrightness.Full));
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
                    new ScreenTintLayer(new FadeLevelColour(1, 2, 3).ToColour()), new AddedPicturedPlaneLayer(subscreen, coverage) }, DisplayBrightness.StepsAbove(13)));
            PixelComparison.Verify(frame, DisplayRendering.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: glow {count}, over a plane {masked}, geometry {width}x{height}");
        }
        for (int rows = 3; rows <= 6; rows++)
        for (int radius = 0; radius <= 24; radius++)
        {
            count++;
            var frame = new DisplayPacket(0, new LayeredScene(memory,
                new DisplayLayer[] { background,
                    global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Rendering.MessageBoxLayer>([typeof(global::SuperMetroid.Core.Assets.SpriteSheetFrame), typeof(int)], new object?[] { SuperMetroid.Core.Rooms.Backgrounds.RoomBackgroundsFixtures.SeededMessagePicture(random, palette, rows), radius }) }, DisplayBrightness.StepsAbove(13)));
            PixelComparison.Verify(frame, DisplayRendering.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: message {count}, rows {rows}, radius {radius}");
        }
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {count} exact color-effect/message comparisons passed.");

        void Check(ColourAddWindowLayer windows)
        {
            count++;
            var frame = new DisplayPacket(0, new LayeredScene(memory,
                new DisplayLayer[] { background,
                    windows, new ScreenTintLayer(new FadeLevelColour(1, 2, 3).ToColour()), windows }, DisplayBrightness.Combine(DisplayBrightness.StepsAbove(13), DisplayBrightness.StepsAbove(11))));
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
            for (int y = 0; y < ScreenGeometry.ScreenHeightPixels; y++)
            for (int x = 0; x < ScreenGeometry.ScreenWidthPixels; x++)
            {
                bool shown = drawn[y * ScreenGeometry.ScreenWidthPixels + x].R != 0, covered = OvalCovers(oval, x, y);
                if (shown != covered)
                    throw new InvalidOperationException($"{device.Kind}: {oval} {(shown ? "lights" : "leaves dark")} ({x},{y}), " +
                        $"which lies {(covered ? "within or on" : "outside")} its edge.");
            }
        }

        // Draws up to ConeMarks cones over a dark screen, each adding its own mark, and checks each pixel shows exactly
        // the marks of the cones that cover it.
        void CheckConeEdges(IReadOnlyList<WindowCone> group)
        {
            count++;
            var layers = new DisplayLayer[group.Count];
            for (int index = 0; index < group.Count; index++)
                layers[index] = new ColourAddWindowLayer(ScreenWindow.Of(group[index]), ConeMark(index));
            var frame = new DisplayPacket(0, new LayeredScene(dark, layers, DisplayBrightness.Full));
            RgbaPixel[] drawn = renderer.RenderForReadback(frame);
            ConeEdgeDirection[] firsts = [.. group.Select(cone => cone.FirstEdgeDirection)];
            ConeEdgeDirection[] seconds = [.. group.Select(cone => cone.SecondEdgeDirection)];
            for (int y = 0; y < ScreenGeometry.ScreenHeightPixels; y++)
            for (int x = 0; x < ScreenGeometry.ScreenWidthPixels; x++)
            {
                RgbaPixel pixel = drawn[y * ScreenGeometry.ScreenWidthPixels + x];
                int shown = ShownMarks(pixel.R) | ShownMarks(pixel.G) << ConeMarksPerChannel
                    | ShownMarks(pixel.B) << 2 * ConeMarksPerChannel;
                for (int index = 0; index < group.Count; index++)
                {
                    bool lit = (shown >> index & 1) != 0, covered = ConeCovers(group[index].Apex, firsts[index], seconds[index], x, y);
                    if (lit != covered)
                        throw new InvalidOperationException($"{device.Kind}: {group[index]} {(lit ? "lights" : "leaves dark")} " +
                            $"({x},{y}), which lies {(covered ? "within" : "outside")} it.");
                }
            }
        }
    }

    /// <summary>The cones one frame checks together: one mark for each level bit of each colour channel.</summary>
    private const int ConeMarks = 3 * ConeMarksPerChannel;

    /// <summary>The marks one colour channel holds: one for each bit of its five-bit level.</summary>
    private const int ConeMarksPerChannel = 5;

    /// <summary>
    /// The colour the cone drawn <paramref name="index"/>th in a frame adds: a level of one bit, alone in its channel, so
    /// the added colours of the cones covering a pixel sum without carrying into one another.
    /// </summary>
    private static RgbColor ConeMark(int index)
    {
        int level = 1 << index % ConeMarksPerChannel, channel = index / ConeMarksPerChannel;
        return new FadeLevelColour(channel == 0 ? level : 0, channel == 1 ? level : 0, channel == 2 ? level : 0).ToColour();
    }

    /// <summary>
    /// The marks an eight-bit channel shows, as bits of its five-bit level. Each mark's eight-bit share is more than all
    /// the smaller marks' together, so the largest marks are taken off first.
    /// </summary>
    /// <exception cref="InvalidOperationException">The channel holds a value no set of marks adds up to.</exception>
    private static int ShownMarks(byte channel)
    {
        int left = channel, marks = 0;
        for (int bit = ConeMarksPerChannel - 1; bit >= 0; bit--)
        {
            int share = EightBitLevel(1 << bit);
            if (left < share) continue;
            left -= share;
            marks |= 1 << bit;
        }
        return left == 0 ? marks : throw new InvalidOperationException($"Channel value {channel} is no sum of cone marks.");
    }

    /// <summary>A five-bit level as the eight-bit channel the backend adds for it.</summary>
    private static int EightBitLevel(int level) => level << 3 | level >> 2;

    /// <summary>The step, clockwise from straight up, nearest the way from <paramref name="apex"/> to the screen's middle.</summary>
    private static TrigonometryAngle TowardScreenMiddle(Offset16<ScreenPixels> apex)
    {
        double turn = Math.Atan2(ScreenGeometry.ScreenWidthPixels / 2 - apex.X, apex.Y - ScreenGeometry.ScreenHeightPixels / 2) / (2 * Math.PI);
        return TrigonometryAngle.Wrapped((int)Math.Round(turn * RotationAngle.StepsPerTurn));
    }

    /// <summary>
    /// Whether the pixel at <paramref name="x"/>, <paramref name="y"/> lies within the cone from <paramref name="apex"/>
    /// between edges leaving it toward <paramref name="first"/> and <paramref name="second"/>, worked in whole numbers: at
    /// most the cone's allowance outside either edge, and not behind its apex.
    /// </summary>
    private static bool ConeCovers(Offset16<ScreenPixels> apex, ConeEdgeDirection first, ConeEdgeDirection second, int x, int y)
    {
        long across = x - apex.X, down = y - apex.Y;
        return first.Across * down - first.Down * across >= -ConeEdgeDirection.HalfPixelAllowance
            && across * second.Down - down * second.Across >= -ConeEdgeDirection.HalfPixelAllowance
            && across * (first.Across + second.Across) + down * (first.Down + second.Down) >= 0;
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
