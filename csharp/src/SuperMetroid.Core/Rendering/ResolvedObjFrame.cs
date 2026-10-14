using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// One software-PPU OBJ raster after SNES OAM-order ownership has been resolved.
/// </summary>
/// <param name="Pixels">Winning opaque color at each pixel, or transparent RGBA.</param>
/// <param name="Priorities">Winning OBJ priority zero through three, or $FF when empty.</param>
public readonly record struct ResolvedObjFrame(
    Rgba32[] Pixels,
    byte[] Priorities);
