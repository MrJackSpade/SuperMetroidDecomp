namespace SuperMetroid.Core.Rendering;

/// <summary>One immutable insertion into an explicitly ordered PPU priority ladder.</summary>
public abstract record RenderLayer;

/// <summary>A 256-pixel-wide window into a wrapping 32-row 2-bpp tilemap.</summary>
public sealed record Bg2BppViewportRenderLayer(ushort TilemapWord, ushort CharacterWord,
    ushort VerticalScroll, bool TransparentColorZero, bool? Priority) : RenderLayer;

/// <summary>Adds fixed five-bit RGB to the composed screen, saturating each component.</summary>
public sealed record FixedColorAddRenderLayer(byte Red, byte Green, byte Blue) : RenderLayer;

/// <summary>Inserts a Mode 7 plane at an explicit position in the OBJ priority ladder.</summary>
/// <param name="Registers">Unmodified integer projection and overflow controls.</param>
/// <param name="SubtractObjSubscreen">Subtracts the winning OBJ from BG1.</param>
/// <param name="AddBg1Subscreen">Owns BG1/OBJ main selection and adds BG1 to eligible main pixels, without halving. Backdrop and OBJ palettes zero through three are excluded.</param>
public sealed record Mode7RenderLayer(Mode7RenderRegisters Registers, bool SubtractObjSubscreen = false,
    bool AddBg1Subscreen = false) : RenderLayer;

/// <summary>Exclusive Mode 7 color operations in the renderer shader contract.</summary>
public enum Mode7ColorMathOperation
{
    /// <summary>Insert the background without color arithmetic.</summary>
    None = 0,
    /// <summary>BG1 main minus OBJ subscreen.</summary>
    SubtractObj = 1,
    /// <summary>BG1/OBJ main plus BG1 subscreen; only eligible layers/palettes participate.</summary>
    AddBg1 = 2,
}

/// <summary>Inserts only pixels whose winning OAM record has this priority.</summary>
public sealed record ObjPriorityRenderLayer(byte Priority, FixedColorAddRenderLayer? FixedColor = null) : RenderLayer;

/// <summary>Inserts the winning OAM pixel, or adds it as an OBJ subscreen using saturated five-bit color arithmetic.</summary>
public sealed record ObjRenderLayer(bool AddToScreen = false, FixedColorAddRenderLayer? FixedColor = null) : RenderLayer;

/// <summary>A scrolled 4-bpp plane using native tilemap and character word addresses.</summary>
public sealed record Bg4BppRenderLayer(ushort TilemapWord, ushort CharacterWord,
    ushort HorizontalScroll, ushort VerticalScroll, int MapWidthTiles,
    int MapHeightTiles, bool? Priority) : RenderLayer;

/// <summary>An unscrolled 32-column 2-bpp plane, including the retained pause HUD.</summary>
public sealed record Bg2BppRenderLayer(ushort TilemapWord, ushort CharacterWord,
    int RowCount, bool Priority) : RenderLayer;
