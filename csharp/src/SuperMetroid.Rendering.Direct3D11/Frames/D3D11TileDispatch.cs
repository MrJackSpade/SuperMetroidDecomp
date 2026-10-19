using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>The tile shader's per-dispatch constants: BG map and character bases, scroll and map size, priority and transparency, brightness and fixed colour, OBJ selection, scanline range and windows (#633).</summary>
internal sealed record D3D11TileDispatch
{
    /// <summary>Map.</summary>
    public uint Map { get; init; } = 0;

    /// <summary>Characters.</summary>
    public uint Characters { get; init; } = 0;

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


    /// <summary>Where a BG pass reads its tiles: the VRAM tilemap at <see cref="Map"/>, or the room's BG1 or BG2.</summary>
    public D3D11RoomSource RoomSource { get; init; } = D3D11RoomSource.VramTilemap;

    /// <summary>First scanline.</summary>
    public uint FirstScanline { get; init; } = 0;

    /// <summary>End scanline.</summary>
    public uint EndScanline { get; init; } = 224;

    /// <summary>Windows.</summary>
    public SnesWindowRegisters Windows { get; init; } = default;

    /// <summary>Window mask.</summary>
    public SnesMainScreenLayers WindowMask { get; init; } = SnesMainScreenLayers.None;

    /// <summary>Window target.</summary>
    public SnesWindowTarget WindowTarget { get; init; } = SnesWindowTarget.Bg1;
}
