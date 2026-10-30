namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>What a Tiles.hlsl layer pass draws; the values are the shader's Source constants.</summary>
internal enum D3D11RoomSource : uint
{
    /// <summary>Nothing: a pass that draws no layer.</summary>
    None = 0,
    /// <summary>The room's layer 1: a door's source screen, words written over it, then its level plane.</summary>
    Layer1 = 1,
    /// <summary>The room's layer-2 plane.</summary>
    Layer2 = 2,
    /// <summary>The scrolling sky's rows (#1347).</summary>
    Sky = 3,
    /// <summary>Layer 3: the room effect (#1348).</summary>
    Layer3 = 4,
    /// <summary>A boss body's tilemap, which layer 2 shows (#1352).</summary>
    BossBody = 6,
    /// <summary>The X-ray reveal's blocks, drawn from block pictures (#1392).</summary>
    XrayReveal = 7,
    /// <summary>A room's library background, drawn from its image and its rebound cells' pictures (#1392).</summary>
    LibraryBackground = 8,
    /// <summary>The HUD's placed pictures, at the HUD's place at the top of the screen (#1466).</summary>
    Hud = 10,
    /// <summary>A pictured plane's placed pictures, scrolled and repeating at the plane's edges (#1467).</summary>
    PicturedPlane = 11,
    /// <summary>A frontend picture grid's cells, its table's word in the pass's map word (#1467).</summary>
    PictureGrid = 12,
}
