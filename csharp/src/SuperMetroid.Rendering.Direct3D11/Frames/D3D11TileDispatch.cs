using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>A layer pass's constants: what it draws and where, its scroll, the part it draws, transparency, brightness, colour and line range (#633).</summary>
internal sealed record D3D11TileDispatch
{
    /// <summary>Map.</summary>
    public uint Map { get; init; } = 0;


    /// <summary>Scroll.</summary>
    public Position16<PlanePixels> Scroll { get; init; } = default;

    /// <summary>
    /// The part of the layer the pass draws (#1514): zero for all of it, otherwise one more than the actor depth a sprite
    /// insertion inserts (<see cref="D3D11FrameRenderer.DepthOf"/>) or than whether a room layer pass draws its front tiles.
    /// </summary>
    public uint DrawnPart { get; init; } = 0;

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

    /// <summary>First scanline.</summary>
    public uint FirstScanline { get; init; } = 0;

    /// <summary>End scanline.</summary>
    public uint EndScanline { get; init; } = 224;

    /// <summary>The screen pixels a layer pass draws within (#1514), or null for the whole screen.</summary>
    public ScreenWindow? Window { get; init; } = null;
}
