using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Rendering;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Checks compiled Power Bomb width and top-offset curves through scanline rendering.</summary>
internal static class PowerBombShapeVerification
{
    /// <summary>Compares all 32 quantized sine widths with native bytes and rejects invalid indices.</summary>
    internal static void VerifyPowerBombWidthAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (int i = 0; i < 32; i++)
            AssertEqual(rom.ReadCartridgeByte(PowerBombShapeReferenceData.Width + i), PowerBombShape.Width(i),
                "Power Bomb quantized sine width");
        foreach (int invalid in new[] { int.MinValue, -1, 32, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => PowerBombShape.Width(invalid),
                "Power Bomb width bounds");
    }

    /// <summary>Compares all 32 quantized vertical boundaries with native bytes and rejects invalid indices.</summary>
    internal static void VerifyPowerBombTopOffsetAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (int i = 0; i < 32; i++)
            AssertEqual(rom.ReadCartridgeByte(PowerBombShapeReferenceData.TopOffset + i), PowerBombShape.TopOffset(i),
                "Power Bomb doubly quantized vertical boundary");
        foreach (int invalid in new[] { int.MinValue, -1, 32, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => PowerBombShape.TopOffset(invalid),
                "Power Bomb top offset bounds");
    }

    /// <summary>
    /// Reads every row of all 21 pre-scaled window shapes through their stream cursor and compares each byte with the
    /// cartridge table the cursor names, then rejects a cursor past the end of either stream.
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
            for (int row = 0; row < 192; row++)
                AssertEqual(rom.ReadCartridgeByte(table + index * 192 + row),
                    PowerBombShape.ReadPreScaledHalfWidth(new PowerBombShapeCursor(stream, index), row),
                    $"Power Bomb {stream} shape {index} row {row}");
            AssertThrows<InvalidOperationException>(
                () => PowerBombShape.ReadPreScaledHalfWidth(
                    new PowerBombShapeCursor(stream, PowerBombShapeCursor.CountOf(stream)), 0),
                $"a {stream} shape past the stream is rejected");
        }
    }

    /// <summary>Checks signed scanline profiles and clipped rendered pixels across radii and centers.</summary>
    internal static void VerifyCompiledPowerBombShape(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyPowerBombWidthAlgorithm), () => VerifyPowerBombWidthAlgorithm(rom));
        Suite(nameof(VerifyPowerBombTopOffsetAlgorithm), () => VerifyPowerBombTopOffsetAlgorithm(rom));
        Suite(nameof(VerifyPowerBombPreScaledShapes), () => VerifyPowerBombPreScaledShapes(rom));
        byte[] widths = new byte[32], tops = new byte[32];
        for (int i = 0; i < 32; i++)
        {
            widths[i] = rom.ReadCartridgeByte(PowerBombShapeReferenceData.Width + i);
            tops[i] = rom.ReadCartridgeByte(PowerBombShapeReferenceData.TopOffset + i);
        }
        int[] Profile(int radius)
        {
            var result = Enumerable.Repeat(-1, 193).ToArray();
            if (radius == 0) return result;
            int outer = EightEight.Multiply(radius, tops[0]);
            // Independent native-style fill: later bands overwrite shared endpoints.
            for (int i = 0; i < 32; i++)
            {
                int inner = EightEight.Multiply(radius, tops[i]);
                for (int y = inner; y <= outer; y++) result[y] = EightEight.Multiply(radius, widths[i]);
                outer = inner;
            }
            for (int y = 0; y <= outer; y++) result[y] = EightEight.Multiply(radius, widths[31]);
            return result;
        }
        var explosion = new SamusPowerBombExplosionState();
        var read = new Func<SamusPowerBombExplosionState, int, int>((state, y) => GameplayEffectCapture.ReadPowerBombHalfWidth(PowerBombRenderSnapshot.Capture(state), y));
        foreach (var phase in new[] { PowerBombExplosionPhase.PreExplosionWhite, PowerBombExplosionPhase.ExplosionYellow })
        {
            explosion.RenderedPhase = phase;
            for (int radius = 0; radius < 256; radius++)
            {
                explosion.RenderedPreExplosionRadius = PowerBombRadius.FromRaw(EightEight.FromWhole(radius) | 123);
                explosion.RenderedExplosionRadius = PowerBombRadius.FromRaw(EightEight.FromWhole(radius) | 234);
                int[] expected = Profile(radius);
                for (int y = -192; y <= 192; y++)
                    AssertEqual(expected[Math.Abs(y)], read(explosion, y), "Power Bomb complete signed band profile without bus");
            }
        }
        int frames = 0;
        foreach (int radius in new[] { 0, 1, 32, 128, 255 })
        foreach (short centerX in new short[] { -16, 128, 272 })
        foreach (short centerY in new short[] { -16, 128, 240 })
        {
            explosion.Spawn(new(unchecked((ushort)centerX), unchecked((ushort)centerY)));
            explosion.RenderedPhase = PowerBombExplosionPhase.ExplosionYellow;
            explosion.RenderedExplosionRadius = PowerBombRadius.FromRaw(EightEight.FromWhole(radius));
            explosion.FixedColor = explosion.FixedColor.WithRed(31);
            ScanlineColorAddRenderLayer windows = GameplayEffectCapture.CapturePowerBombColorMath(PowerBombRenderSnapshot.Capture(explosion), new Position16<RoomPixels>(0, 0))
                ?? throw new InvalidOperationException("An active Power Bomb captures its windows.");
            int[] profile = Profile(radius);
            for (int y = 0; y < 224; y++)
            {
                ColorAddWindow window = windows.Windows[y];
                for (int x = 0; x < 256; x++)
                {
                    int distance = Math.Abs(y - centerY);
                    int halfWidth = distance < profile.Length ? profile[distance] : -1;
                    bool lit = y >= DisplayScreen.HudHeight && halfWidth >= 0 && Math.Abs(x - centerX) <= halfWidth;
                    bool covered = x >= window.Left && x <= window.Right;
                    AssertEqual(lit, covered, "Power Bomb native-profile window and clipping");
                    if (covered)
                        AssertEqual((255, 0, 0), (window.Red, window.Green, window.Blue), "Power Bomb window adds its fixed colour");
                }
            }
            frames++;
        }
        Console.WriteLine($"Power Bomb compiled shape: 64 native bytes, 197120 signed scanline cases and {frames} complete clipped/HUD-preserving frames match without a bus.");
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
