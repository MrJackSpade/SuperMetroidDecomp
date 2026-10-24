namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>The composition passes Tiles.hlsl runs, one per dispatch; the values are the shader's Op constants.</summary>
internal enum D3D11TileOperation : uint
{
    /// <summary>Fills the screen with the backdrop colour.</summary>
    Backdrop = 0,
    /// <summary>Draws a tile layer over the screen, mosaic-grouped where the pass asks.</summary>
    Layer = 1,
    /// <summary>Applies one brightness level.</summary>
    Brightness = 3,
    /// <summary>Adds a fixed colour to the screen.</summary>
    AddColour = 4,
    /// <summary>Inserts the sprite layer's pixels of one priority, or all of them, into the screen.</summary>
    InsertSprites = 6,
    /// <summary>Draws the perspective floor from its image.</summary>
    Floor = 7,
    /// <summary>Adds a colour within each line's window.</summary>
    LineColourAdd = 8,
    /// <summary>Adds a layer's colours to the screen.</summary>
    AddLayer = 9,
    /// <summary>Subtracts a layer's colours from the screen.</summary>
    SubtractLayer = 10,
    /// <summary>Adds a layer's colours where the layer that covers it shows.</summary>
    AddLayerBehind = 11,
    /// <summary>Draws the message box's text.</summary>
    MessageText = 12,
    /// <summary>Composes a room under the X-ray's reveal and blend.</summary>
    XrayRoom = 14,
}
