using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuiteTimingVerification;

/// <summary>Checks compiled Power Bomb width and top-offset curves through scanline rendering.</summary>
internal static class PowerBombShapeDefinitionsVerification
{
    /// <summary>Compares all 32 quantized sine widths with native bytes and rejects invalid indices.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for power bomb width algorithm")]
    internal static void VerifyPowerBombWidthAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (int i = 0; i < 32; i++)
            AssertEqual(rom.ReadCartridgeByte(PowerBombShapeReferenceData.Width + i), PowerBombShapeDefinitions.Width(i),
                "Power Bomb quantized sine width");
        foreach (int invalid in new[] { int.MinValue, -1, 32, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => PowerBombShapeDefinitions.Width(invalid),
                "Power Bomb width bounds");
    }

    /// <summary>Compares all 32 quantized vertical boundaries with native bytes and rejects invalid indices.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for power bomb top offset algorithm")]
    internal static void VerifyPowerBombTopOffsetAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (int i = 0; i < 32; i++)
            AssertEqual(rom.ReadCartridgeByte(PowerBombShapeReferenceData.TopOffset + i), PowerBombShapeDefinitions.TopOffset(i),
                "Power Bomb doubly quantized vertical boundary");
        foreach (int invalid in new[] { int.MinValue, -1, 32, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => PowerBombShapeDefinitions.TopOffset(invalid),
                "Power Bomb top offset bounds");
    }

    /// <summary>Checks signed scanline profiles and clipped rendered pixels across radii and centers.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for power bomb shape")]
    internal static void VerifyCompiledPowerBombShape(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyPowerBombWidthAlgorithm), () => VerifyPowerBombWidthAlgorithm(rom));
        Suite(nameof(VerifyPowerBombTopOffsetAlgorithm), () => VerifyPowerBombTopOffsetAlgorithm(rom));
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
            int outer = SnesEightEight.Multiply(radius, tops[0]);
            // Independent native-style fill: later bands overwrite shared endpoints.
            for (int i = 0; i < 32; i++)
            {
                int inner = SnesEightEight.Multiply(radius, tops[i]);
                for (int y = inner; y <= outer; y++) result[y] = SnesEightEight.Multiply(radius, widths[i]);
                outer = inner;
            }
            for (int y = 0; y <= outer; y++) result[y] = SnesEightEight.Multiply(radius, widths[31]);
            return result;
        }
        var explosion = new SamusPowerBombExplosionState();
        var read = new Func<SamusPowerBombExplosionState, int, int>((state, y) => SnesGameplayFrameRenderer.ReadPowerBombHalfWidth(PowerBombRenderSnapshot.Capture(state), y));
        foreach (var phase in new[] { PowerBombExplosionPhase.PreExplosionWhite, PowerBombExplosionPhase.ExplosionYellow })
        {
            explosion.RenderedPhase = phase;
            for (int radius = 0; radius < 256; radius++)
            {
                explosion.RenderedPreExplosionRadius = (ushort)(SnesEightEight.FromWhole(radius) | 123);
                explosion.RenderedExplosionRadius = (ushort)(SnesEightEight.FromWhole(radius) | 234);
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
            explosion.RenderedExplosionRadius = (ushort)SnesEightEight.FromWhole(radius);
            explosion.FixedColorRed = (byte)31;
            var frame = Enumerable.Repeat(new Rgba32(0, 0, 0, 255), 256 * 224).ToArray();
            SnesGameplayFrameRenderer.ApplyPowerBombColorMath(frame, PowerBombRenderSnapshot.Capture(explosion), new Position16<RoomPixels>(0, 0));
            int[] profile = Profile(radius);
            for (int y = 0; y < 224; y++)
            for (int x = 0; x < 256; x++)
            {
                int distance = Math.Abs(y - centerY);
                int halfWidth = distance < profile.Length ? profile[distance] : -1;
                bool lit = y >= SnesGameplayFrameRenderer.HudHeight && halfWidth >= 0 && Math.Abs(x - centerX) <= halfWidth;
                AssertEqual(new Rgba32(lit ? (byte)255 : (byte)0, 0, 0, 255), frame[y * 256 + x], "Power Bomb native-profile pixels and clipping");
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
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
