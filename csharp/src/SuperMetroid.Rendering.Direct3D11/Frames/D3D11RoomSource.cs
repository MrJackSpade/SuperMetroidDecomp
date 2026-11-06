namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>What a Tiles.hlsl layer pass draws; the values are the shader's Source constants.</summary>
internal enum D3D11RoomSource : uint
{
    /// <summary>Nothing: a pass that draws no layer.</summary>
    None = 0,
    /// <summary>The room's foreground: a door's source screen, pictures written over it, then its level plane.</summary>
    Foreground = 1,
    /// <summary>The room's background plane.</summary>
    Background = 2,
    /// <summary>The scrolling sky's rows (#1347).</summary>
    Sky = 3,
    /// <summary>The effect overlay: the room's liquid, fog or haze (#1348).</summary>
    FxOverlay = 4,
    /// <summary>A boss body, which the background shows (#1352).</summary>
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
