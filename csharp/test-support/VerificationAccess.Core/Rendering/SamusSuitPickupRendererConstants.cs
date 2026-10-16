using SuperMetroid.Core.Hardware;
﻿using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Rendering;

/// <summary>Cartridge values and helpers of <see cref="SamusSuitPickupRenderer"/> that only verification reads.</summary>
internal static class SamusSuitPickupRendererConstants
{
    /// <summary>The SNES main-screen pixel width used to index the destination frame.</summary>
    internal const int Width = SnesPpuLayout.ScreenWidthPixels;
    /// <summary>
    /// Applies `$91:D692`'s COLDATA addition inside the light beam's rasterized window.
    /// </summary>
    /// <remarks>
    /// The hardware performs this while scanning the already-selected main-screen BG/OBJ
    /// pixels. Applying the same saturating five-bit addition after the desktop compositor
    /// is equivalent for this effect and avoids teaching every room renderer about a
    /// transient Samus-owned HDMA channel.
    /// </remarks>
    public static void Composite(Span<Rgba32> frame, SamusSuitPickupFlash? flash)
    {
        Ensure.LengthEqual(frame, Width * SamusSuitPickupRenderer.Height);
        if (flash is not { } active)
            return;

        byte red = SamusSuitPickupRenderer.ExpandFiveBit(active.Red);
        byte green = SamusSuitPickupRenderer.ExpandFiveBit(active.Green);
        byte blue = SamusSuitPickupRenderer.ExpandFiveBit(active.Blue);
        ushort[] windows = SuitPickupBeamWindowBuilder.Build(active.Beam);
        for (int y = 0; y < SamusSuitPickupRenderer.Height; y++)
        {
            ushort endpoints = windows[y];
            int left = SnesLittleEndian.LowByte(endpoints);
            int right = SnesLittleEndian.HighByte(endpoints);
            if (left > right)
                continue;

            int row = y * Width;
            for (int x = left; x <= right; x++)
            {
                Rgba32 source = frame[row + x];
                frame[row + x] = new Rgba32(
                    SaturatingAdd(source.R, red),
                    SaturatingAdd(source.G, green),
                    SaturatingAdd(source.B, blue),
                    source.A);
            }
        }
    }
    /// <summary>Clamps the combined source and fixed-color channels before storing a pixel.</summary>
    internal static byte SaturatingAdd(byte left, byte right) =>
        unchecked((byte)Math.Min(byte.MaxValue, left + right));
}
