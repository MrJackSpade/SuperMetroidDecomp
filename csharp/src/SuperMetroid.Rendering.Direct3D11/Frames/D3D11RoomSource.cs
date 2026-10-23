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
    /// <summary>The BG3 plane: the HUD and the room effect (#1348).</summary>
    Bg3 = 4,
    /// <summary>The frame's Mode 7 floor: a Ceres room's map (#1349) or a frontend scene's (#1403).</summary>
    Mode7Map = 5,
    /// <summary>A boss body's BG2 tilemap (#1352).</summary>
    BossBg2 = 6,
    /// <summary>The X-ray reveal's blocks, drawn from block pictures (#1392).</summary>
    XrayReveal = 7,
    /// <summary>A room's library background, drawn from its image and its rebound cells' pictures (#1392).</summary>
    Bg2Image = 8,

    /// <summary>A frontend layer's VRAM tilemap, drawn from its characters' pre-coloured tiles in its table (#1392).</summary>
    ScreenTiles = 9,
}
