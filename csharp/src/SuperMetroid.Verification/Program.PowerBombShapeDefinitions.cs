using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using System.Reflection;
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
        void Set(string property, object value) => typeof(SamusPowerBombExplosionState).GetProperty(property)!.SetValue(explosion, value);
        var read = typeof(SnesGameplayFrameRenderer).GetMethod("ReadPowerBombHalfWidth", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Func<SamusPowerBombExplosionState, int, int>>();
        var forbidden = NewPowerBombShapeReadGuard();
        foreach (var phase in new[] { PowerBombExplosionPhase.PreExplosionWhite, PowerBombExplosionPhase.ExplosionYellow })
        {
            Set(nameof(explosion.RenderedPhase), phase);
            for (int radius = 0; radius < 256; radius++)
            {
                Set(nameof(explosion.RenderedPreExplosionRadius), (ushort)((radius << 8) | 123));
                Set(nameof(explosion.RenderedExplosionRadius), (ushort)((radius << 8) | 234));
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
            Set(nameof(explosion.RenderedPhase), PowerBombExplosionPhase.ExplosionYellow);
            Set(nameof(explosion.RenderedExplosionRadius), (ushort)(radius << 8));
            Set(nameof(explosion.FixedColorRed), (byte)31);
            var frame = Enumerable.Repeat(new Rgba32(0, 0, 0, 255), 256 * 224).ToArray();
            SnesGameplayFrameRenderer.ApplyPowerBombColorMath(frame, forbidden, explosion, new Position16<RoomPixels>(0, 0));
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

    /// <summary>Rejects all bus access while the installed shape renderer calculates pixels.</summary>
    private static CartridgeReadGuard NewPowerBombShapeReadGuard() =>
        CartridgeReadGuard.Unbacked()
            .ForbidAll(address => $"Unexpected shape ROM read: {address:X6}.")
            .RejectWrites(_ => "Unexpected shape bus write.");

    /// <summary>Runs the Power Bomb shape algorithm and pixel fixture.</summary>
    [VerificationCommand("--power-bomb-shape-fixture")]
    internal static int PowerBombShapeFixtureCommand()
    {
        Suite(nameof(VerifyCompiledPowerBombShape), () => VerifyCompiledPowerBombShape(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}

/// <summary>Native bank $88 shape-table addresses used as independent comparison data.</summary>
internal static class PowerBombShapeReferenceData
{
    /// <summary>$88:A266, PowerBombExplosion_ShapeDefinitionTable_Unscaled_width: 32 bottom-to-center bytes.</summary>
    internal const int Width = 0x88a266;
    /// <summary>$88:A286, PowerBombExplosion_ShapeDefinitionTable_Unscaled_topOffset: 32 inclusive boundaries.</summary>
    internal const int TopOffset = 0x88a286;
}
