using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// BG2 scroll-register replacements written by HDMA for gameplay lines 32..223. The horizontal and
/// vertical registers are fed by independent HDMA channels, so either table may be absent; an absent
/// table leaves that register at its latched value on every line.
/// </summary>
[SpatialComponents("HDMA adapter: BG2HOFS and BG2VOFS are written by independent, separately optional HDMA channels.")]
public readonly struct Bg2HdmaScrollTables
{
    private readonly ushort[]? horizontal;
    private readonly ushort[]? vertical;

    /// <summary>Copies the optional per-line register tables.</summary>
    /// <param name="horizontal">Empty or exactly 192 BG2 horizontal register values; index zero is physical line 32.</param>
    /// <param name="vertical">Empty or exactly 192 BG2 vertical register values; literal offsets, not precomputed source rows.</param>
    /// <exception cref="ArgumentException">A nonempty table does not contain exactly 192 values.</exception>
    public Bg2HdmaScrollTables(ReadOnlySpan<ushort> horizontal, ReadOnlySpan<ushort> vertical)
    {
        if ((!horizontal.IsEmpty && horizontal.Length != GameplayLines) || (!vertical.IsEmpty && vertical.Length != GameplayLines))
            throw new ArgumentException("Gameplay HDMA tables must contain exactly 192 register values.");
        this.horizontal = horizontal.ToArray();
        this.vertical = vertical.ToArray();
    }

    /// <summary>The number of gameplay scanlines a nonempty table covers.</summary>
    public static int GameplayLines => SnesPpuLayout.ScreenHeightPixels - SnesPpuLayout.GameplayHudHeightPixels;

    /// <summary>The horizontal register table; empty when that channel is inactive.</summary>
    public ReadOnlySpan<ushort> Horizontal => horizontal;

    /// <summary>The vertical register table; empty when that channel is inactive.</summary>
    public ReadOnlySpan<ushort> Vertical => vertical;

    /// <summary>The BG2 scroll registers on gameplay line <paramref name="line"/> (0..191): each axis's
    /// table entry, or <paramref name="latched"/> for an axis whose channel is inactive.</summary>
    public Position16<Bg2Pixels> At(int line, Position16<Bg2Pixels> latched) => new(
        horizontal is { Length: > 0 } ? horizontal[line] : latched.X,
        vertical is { Length: > 0 } ? vertical[line] : latched.Y);
}
