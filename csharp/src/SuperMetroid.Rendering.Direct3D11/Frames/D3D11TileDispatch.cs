using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>A layer pass's constants: what it draws and where, its scroll and map size, priority, transparency, brightness, colour and line range (#633).</summary>
internal sealed record D3D11TileDispatch
{
    /// <summary>Map.</summary>
    public uint Map { get; init; } = 0;


    /// <summary>Scroll.</summary>
    public Position16<BgPlanePixels> Scroll { get; init; } = default;

    /// <summary>Map size.</summary>
    public Extent16<TilemapCells>? MapSize { get; init; } = null;

    /// <summary>Priority.</summary>
    public uint Priority { get; init; } = 0;

    /// <summary>Transparent zero.</summary>
    public uint TransparentZero { get; init; } = 1;

    /// <summary>Level.</summary>
    public uint Level { get; init; } = 15;

    /// <summary>Red.</summary>
    public uint Red { get; init; } = 0;

    /// <summary>Green.</summary>
    public uint Green { get; init; } = 0;

    /// <summary>Blue.</summary>
    public uint Blue { get; init; } = 0;


    /// <summary>What a layer pass draws.</summary>
    public D3D11RoomSource RoomSource { get; init; } = D3D11RoomSource.None;

    /// <summary>The room level buffer word of a <see cref="D3D11RoomSource.ScreenCells"/> layer's character tile table (#1392).</summary>
    public uint ScreenTable { get; init; } = 0;

    /// <summary>First scanline.</summary>
    public uint FirstScanline { get; init; } = 0;

    /// <summary>End scanline.</summary>
    public uint EndScanline { get; init; } = 224;
}
