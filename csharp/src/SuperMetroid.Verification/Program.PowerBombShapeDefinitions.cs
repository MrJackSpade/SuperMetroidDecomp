using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyCompiledPowerBombShape(SuperMetroidAddressSpace rom)
    {
        byte[] widths = new byte[32], tops = new byte[32];
        for (int i = 0; i < 32; i++)
        {
            widths[i] = rom.ReadByte(0x88a266 + i);
            tops[i] = rom.ReadByte(0x88a286 + i);
            AssertEqual(widths[i], PowerBombShapeDefinitions.Widths[i], "Power Bomb compiled width byte");
            AssertEqual(tops[i], PowerBombShapeDefinitions.TopOffsets[i], "Power Bomb compiled top byte");
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
            .CreateDelegate<Func<ISnesAddressSpace, SamusPowerBombExplosionState, int, int>>();
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
                    AssertEqual(expected[Math.Abs(y)], read(forbidden, explosion, y), "Power Bomb complete signed band profile without bus");
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

    private sealed class PowerBombShapeReadGuard : ISnesAddressSpace
    {
        public byte ReadByte(int address) => throw new InvalidOperationException($"Unexpected shape ROM read: {address:X6}.");
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Unexpected shape bus write.");
    }
}
