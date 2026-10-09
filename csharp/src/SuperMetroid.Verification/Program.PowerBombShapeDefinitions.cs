using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    /// <summary>Compares all 32 compiled Power Bomb half-width entries with their retail ROM bytes and checks bounds behavior.</summary>
    /// <param name="rom">Retail address space containing the native width table.</param>
    private static void VerifyPowerBombWidthAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (int i = 0; i < 32; i++)
            AssertEqual(rom.ReadByte(PowerBombShapeReferenceData.Width + i), PowerBombShapeDefinitions.Width(i),
                "Power Bomb quantized sine width");
        foreach (int invalid in new[] { int.MinValue, -1, 32, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => PowerBombShapeDefinitions.Width(invalid),
                "Power Bomb width bounds");
    }

    /// <summary>Compares the 32 compiled Power Bomb band boundaries with retail data and verifies the table's index limits.</summary>
    /// <param name="rom">Retail address space containing the native top-offset table.</param>
    private static void VerifyPowerBombTopOffsetAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (int i = 0; i < 32; i++)
            AssertEqual(rom.ReadByte(PowerBombShapeReferenceData.TopOffset + i), PowerBombShapeDefinitions.TopOffset(i),
                "Power Bomb doubly quantized vertical boundary");
        foreach (int invalid in new[] { int.MinValue, -1, 32, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => PowerBombShapeDefinitions.TopOffset(invalid),
                "Power Bomb top offset bounds");
    }

    /// <summary>Checks rendered Power Bomb scanline widths and clipping against a native-style profile without permitting runtime ROM reads.</summary>
    /// <param name="rom">Retail address space used to capture the expected width and top-offset tables.</param>
    private static void VerifyCompiledPowerBombShape(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyPowerBombWidthAlgorithm), () => VerifyPowerBombWidthAlgorithm(rom));
        Suite(nameof(VerifyPowerBombTopOffsetAlgorithm), () => VerifyPowerBombTopOffsetAlgorithm(rom));
        byte[] widths = new byte[32], tops = new byte[32];
        for (int i = 0; i < 32; i++)
        {
            widths[i] = rom.ReadByte(PowerBombShapeReferenceData.Width + i);
            tops[i] = rom.ReadByte(PowerBombShapeReferenceData.TopOffset + i);
        }
        int[] Profile(int radius)
        {
            var result = Enumerable.Repeat(-1, 193).ToArray();
            if (radius == 0) return result;
            int outer = radius * tops[0] >> 8;
            // Independent native-style fill: later bands overwrite shared endpoints.
            for (int i = 0; i < 32; i++)
            {
                int inner = radius * tops[i] >> 8;
                for (int y = inner; y <= outer; y++) result[y] = radius * widths[i] >> 8;
                outer = inner;
            }
            for (int y = 0; y <= outer; y++) result[y] = radius * widths[31] >> 8;
            return result;
        }
        var explosion = new SamusPowerBombExplosionState();
        void Set(string property, object value) => typeof(SamusPowerBombExplosionState).GetProperty(property)!.SetValue(explosion, value);
        var read = typeof(SnesGameplayFrameRenderer).GetMethod("ReadPowerBombHalfWidth", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Func<SamusPowerBombExplosionState, int, int>>();
        var forbidden = new PowerBombShapeReadGuard();
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
            explosion.Spawn(unchecked((ushort)centerX), unchecked((ushort)centerY));
            Set(nameof(explosion.RenderedPhase), PowerBombExplosionPhase.ExplosionYellow);
            Set(nameof(explosion.RenderedExplosionRadius), (ushort)(radius << 8));
            Set(nameof(explosion.FixedColorRed), (byte)31);
            var frame = Enumerable.Repeat(new Rgba32(0, 0, 0, 255), 256 * 224).ToArray();
            SnesGameplayFrameRenderer.ApplyPowerBombColorMath(frame, forbidden, explosion, 0, 0);
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

    /// <summary>Address-space sentinel that fails any attempt to read or write through the Power Bomb shape renderer.</summary>
    private sealed class PowerBombShapeReadGuard : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Rejects cartridge imports so tests can confirm the renderer uses compiled shape metadata.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>This implementation never returns because all reads are unexpected.</returns>
        /// <exception cref="InvalidOperationException">A cartridge byte read was attempted.</exception>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects a runtime read, since shape rendering must not depend on ROM access.</summary>
        /// <param name="address">Address whose read would violate the test's no-bus contract.</param>
        /// <returns>This implementation never returns because all reads are unexpected.</returns>
        /// <exception cref="InvalidOperationException">A byte read was attempted.</exception>
        public static byte ReadByte(int address) => throw new InvalidOperationException($"Unexpected shape ROM read: {address:X6}.");

        /// <summary>Rejects writes because the shape-rendering probe is not expected to mutate an address space.</summary>
        /// <param name="address">Address whose write was attempted.</param>
        /// <param name="value">Byte value supplied by the caller.</param>
        /// <exception cref="InvalidOperationException">A bus write was attempted.</exception>
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Unexpected shape bus write.");
    }
}

/// <summary>Retail bank-$88 locations of the two 32-byte lookup tables used to construct the Power Bomb shape profile.</summary>
internal static class PowerBombShapeReferenceData
{
    /// <summary>$88:A266, PowerBombExplosion_ShapeDefinitionTable_Unscaled_width: 32 bottom-to-center bytes.</summary>
    internal const int Width = 0x88a266;
    /// <summary>$88:A286, PowerBombExplosion_ShapeDefinitionTable_Unscaled_topOffset: 32 inclusive boundaries.</summary>
    internal const int TopOffset = 0x88a286;
}
