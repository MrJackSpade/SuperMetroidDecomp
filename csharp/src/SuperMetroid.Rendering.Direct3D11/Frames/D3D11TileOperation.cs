namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>Exclusive compute operations shared with Tiles.hlsl.</summary>
internal enum D3D11TileOperation : uint
{
    /// <summary>Initializes the output with the current backdrop color.</summary>
    Backdrop = 0,
    /// <summary>Composes a four-bit background layer.</summary>
    Bg4 = 1,
    /// <summary>Composes a two-bit background layer.</summary>
    Bg2 = 2,
    /// <summary>Applies one requested brightness level.</summary>
    Brightness = 3,
    /// <summary>Adds a fixed color to the current composition.</summary>
    FixedAdd = 4,
    /// <summary>Decodes object pixels and priorities from the uploaded OAM snapshot.</summary>
    ResolveObj = 5,
    /// <summary>Inserts selected object pixels into the current layer order.</summary>
    InsertObj = 6,
    /// <summary>Composes the affine Mode 7 background.</summary>
    Mode7 = 7,
    /// <summary>Adds per-scanline color contributions.</summary>
    ScanlineAdd = 8,
    /// <summary>Adds the selected background colors to the current composition.</summary>
    BgAdd = 9,
    /// <summary>Subtracts the selected background colors from the current composition.</summary>
    BgSubtract = 10,
    /// <summary>Adds the subscreen contribution to the main-screen output.</summary>
    SubscreenAdd = 11,
    /// <summary>Composes the message-box tilemap over the scene.</summary>
    Message = 12,
    /// <summary>Composes the X-ray gameplay overlay.</summary>
    XrayGameplay = 14
}
