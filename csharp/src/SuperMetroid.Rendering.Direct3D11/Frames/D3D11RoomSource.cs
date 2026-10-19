namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>Where a Tiles.hlsl BG pass reads its tiles; the values are the shader's RoomSource constants.</summary>
internal enum D3D11RoomSource : uint
{
    /// <summary>The tilemap in captured VRAM.</summary>
    VramTilemap = 0,
    /// <summary>The room's BG1: a door's source screen, words written over it, then its level plane.</summary>
    Bg1 = 1,
    /// <summary>The room's layer-2 plane.</summary>
    Bg2 = 2,
    /// <summary>The scrolling sky's rows (#1347).</summary>
    Sky = 3,
}
