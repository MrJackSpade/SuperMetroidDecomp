using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.FrameRendering;
namespace SuperMetroid.Verification.Rendering;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Checks the Power Bomb's blast oval: its authored radii and aspect against the cartridge, its capture and its drawing.</summary>
internal static class PowerBombShapeVerification
{
    /// <summary>The cartridge's unscaled curve entries the blast's widest and tallest extents come from.</summary>
    private const int UnscaledCurveEntries = 32;

    /// <summary>
    /// The blast is an oval three quarters as tall as wide (#1468): checks that the cartridge's unscaled curve, whose last
    /// width is the blast's widest extent and whose first top offset is its tallest, has that aspect to within one part
    /// in a hundred.
    /// </summary>
    internal static void VerifyPowerBombOvalAspect(SuperMetroidAddressSpace rom)
    {
        int widest = rom.ReadCartridgeByte(PowerBombShapeReferenceData.Width + UnscaledCurveEntries - 1);
        int tallest = rom.ReadCartridgeByte(PowerBombShapeReferenceData.TopOffset);
        double aspect = (double)PowerBombExplosionDefinitions.OvalHeightParts / PowerBombExplosionDefinitions.OvalWidthParts;
        AssertTrue(Math.Abs((double)tallest / widest - aspect) < 0.01,
            $"the cartridge's blast curve, {widest} wide by {tallest} tall, has the oval's aspect");
    }

    /// <summary>
    /// Checks each walked oval's radius against the cartridge's pre-scaled shape it replaces: the shape's middle line is
    /// 255/256 of the radius wide on each side. Also rejects a cursor past the end of either stream.
    /// </summary>
    internal static void VerifyPowerBombPreScaledShapes(SuperMetroidAddressSpace rom)
    {
        AssertEqual(17, PowerBombShapeCursor.CountOf(PowerBombShapeStream.White), "seventeen white shapes");
        AssertEqual(4, PowerBombShapeCursor.CountOf(PowerBombShapeStream.Yellow), "four yellow shapes");
        foreach ((PowerBombShapeStream stream, int table) in new[]
        {
            new VerifyPowerBombPreScaledShapesCase(PowerBombShapeStream.White, PowerBombShapeReferenceData.PreScaledWhite),
            new VerifyPowerBombPreScaledShapesCase(PowerBombShapeStream.Yellow, PowerBombShapeReferenceData.PreScaledYellow),
        })
        {
            for (int index = 0; index < PowerBombShapeCursor.CountOf(stream); index++)
            {
                int radius = new PowerBombShapeCursor(stream, index).HorizontalRadius();
                AssertEqual((int)rom.ReadCartridgeByte(table + index * PowerBombShapeReferenceData.ShapeLines), radius * 255 / 256,
                    $"Power Bomb {stream} shape {index} has the cartridge shape's middle width");
            }
            AssertThrows<InvalidOperationException>(
                () => new PowerBombShapeCursor(stream, PowerBombShapeCursor.CountOf(stream)).HorizontalRadius(),
                $"a {stream} shape past the stream is rejected");
        }
    }

    /// <summary>
    /// Checks the captured oval for every whole radius of the unscaled phases: centred on the blast, the radius wide and
    /// three quarters of it tall, below the HUD, or nothing for a radius too small to show. Then draws blasts on and off
    /// the screen and checks the centre is lit, the HUD and the pixels well beyond the oval are not.
    /// </summary>
    internal static void VerifyCompiledPowerBombShape(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyPowerBombOvalAspect), () => VerifyPowerBombOvalAspect(rom));
        Suite(nameof(VerifyPowerBombPreScaledShapes), () => VerifyPowerBombPreScaledShapes(rom));
        var explosion = new SamusPowerBombExplosionState();
        explosion.Spawn(new(100, 120));
        int cases = 0;
        foreach (var phase in new[] { PowerBombExplosionPhase.PreExplosionWhite, PowerBombExplosionPhase.ExplosionYellow })
        {
            explosion.RenderedPhase = phase;
            for (int radius = 0; radius < 256; radius++)
            {
                explosion.RenderedPreExplosionRadius = PowerBombRadius.FromRaw(EightEight.FromWhole(radius) | 123);
                explosion.RenderedExplosionRadius = PowerBombRadius.FromRaw(EightEight.FromWhole(radius) | 234);
                ColourAddWindowLayer layer = GameplayEffectCapture.CapturePowerBombGlow(PowerBombRenderSnapshot.Capture(explosion), new Position16<RoomPixels>(0, 0))
                    ?? throw new InvalidOperationException("An active Power Bomb captures its window.");
                int tall = radius * 3 / 4;
                ScreenWindow expected = tall == 0 ? ScreenWindow.Closed
                    : ScreenWindow.Of(WindowOval.Ellipse(Offset16<ScreenPixels>.Of(100, 120), radius, tall), DisplayScreen.HudHeight);
                AssertEqual(expected, layer.Window, $"Power Bomb {phase} oval of whole radius {radius}");
                cases++;
            }
        }
        int frames = 0;
        foreach (int radius in new[] { 32, 128, 255 })
        foreach (short centerX in new short[] { -16, 128, 272 })
        foreach (short centerY in new short[] { -16, 128, 240 })
        {
            explosion.Spawn(new(unchecked((ushort)centerX), unchecked((ushort)centerY)));
            explosion.RenderedPhase = PowerBombExplosionPhase.ExplosionYellow;
            explosion.RenderedExplosionRadius = PowerBombRadius.FromRaw(EightEight.FromWhole(radius));
            explosion.FixedColor = explosion.FixedColor.WithRed(31);
            ColourAddWindowLayer layer = GameplayEffectCapture.CapturePowerBombGlow(PowerBombRenderSnapshot.Capture(explosion), new Position16<RoomPixels>(0, 0))
                ?? throw new InvalidOperationException("An active Power Bomb captures its window.");
            RgbaPixel[] drawn = DisplayRendering.Render(new LayeredScene(FramePaint.FromNative(new RgbColor[SnesCgram.ColorCount], []),
                new DisplayLayer[] { layer }, DisplayBrightness.Full));
            int tall = radius * 3 / 4;
            for (int y = 0; y < DisplayScreen.Height; y++)
            for (int x = 0; x < DisplayScreen.Width; x++)
            {
                double dx = (double)(x - centerX) / radius, dy = (double)(y - centerY) / tall;
                double distance = dx * dx + dy * dy;
                bool lit = drawn[y * DisplayScreen.Width + x].R != 0;
                if (y < DisplayScreen.HudHeight || distance > 1.1)
                    AssertTrue(!lit, $"Power Bomb radius {radius} at {centerX},{centerY} leaves {x},{y} outside its oval or in the HUD unlit");
                else if (distance < 0.9)
                    AssertTrue(lit, $"Power Bomb radius {radius} at {centerX},{centerY} lights {x},{y} inside its oval");
            }
            frames++;
        }
        Console.WriteLine($"Power Bomb oval: authored radii and aspect match the cartridge, {cases} captured ovals and {frames} drawn blasts agree.");
    }

    /// <summary>Runs the Power Bomb shape algorithm and pixel fixture.</summary>
    [VerificationCommand("--power-bomb-shape-fixture")]
    internal static int PowerBombShapeFixtureCommand()
    {
        Suite(nameof(VerifyCompiledPowerBombShape), () => VerifyCompiledPowerBombShape(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
