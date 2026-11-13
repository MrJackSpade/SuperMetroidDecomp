using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>A layer pass's constants: what it draws and where, its scroll, the depth it draws, transparency, brightness, colour and line range (#633).</summary>
internal sealed record D3D11TileDispatch
{
    /// <summary>
    /// The room level word a pictured plane's or a picture grid's table starts at (#1467), as Tiles.hlsl's TableWord
    /// reads it; zero for a pass that reads no table.
    /// </summary>
    public uint Table { get; init; } = 0;


    /// <summary>Scroll.</summary>
    public Position16<PlanePixels> Scroll { get; init; } = default;

    /// <summary>
    /// The depth of what the pass draws (#1514, #1634): zero for everything, otherwise one more than the actor depth a sprite
    /// insertion inserts (<see cref="D3D11FrameRenderer.DepthOf"/>), or, for a room layer pass, which part of its pictures it
    /// draws: one for the part behind, two for the part in front.
    /// </summary>
    public uint DrawnDepth { get; init; } = 0;

    /// <summary>Whether a layer pass leaves its pictures' clear pixels showing what lies behind (#1544).</summary>
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

    /// <summary>First scanline.</summary>
    public uint FirstScanline { get; init; } = 0;

    /// <summary>End scanline.</summary>
    public uint EndScanline { get; init; } = 224;

    /// <summary>The screen pixels a layer pass draws within (#1514), or null for the whole screen.</summary>
    public ScreenWindow? Window { get; init; } = null;
}
