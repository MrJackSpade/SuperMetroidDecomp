namespace SuperMetroid.Core.Rendering;

/// <summary>One immutable insertion into an explicitly ordered PPU priority ladder.</summary>
public abstract record RenderLayer;

/// <summary>A 256-pixel-wide window into a wrapping 32-row 2-bpp tilemap.</summary>
/// <param name="TilemapWord">VRAM word address of the source map.</param>
/// <param name="CharacterWord">VRAM word address of the 2-bpp character data.</param>
/// <param name="VerticalScroll">Wrapped vertical register offset.</param>
/// <param name="TransparentColorZero">Whether palette entry zero leaves the lower layer visible.</param>
/// <param name="Priority">Optional forced tile priority; null uses each map entry's priority bit.</param>
public sealed record Bg2BppViewportRenderLayer(ushort TilemapWord, ushort CharacterWord,
    ushort VerticalScroll, bool TransparentColorZero, bool? Priority) : RenderLayer;

/// <summary>Adds fixed five-bit RGB to the composed screen, saturating each component.</summary>
/// <param name="Red">Five-bit red component to add.</param>
/// <param name="Green">Five-bit green component to add.</param>
/// <param name="Blue">Five-bit blue component to add.</param>
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
/// <param name="Priority">Winning OAM priority group to select.</param>
/// <param name="FixedColor">Optional saturated color addition applied to selected pixels.</param>
public sealed record ObjPriorityRenderLayer(byte Priority, FixedColorAddRenderLayer? FixedColor = null) : RenderLayer;

/// <summary>Inserts the winning OAM pixel, or adds it as an OBJ subscreen using saturated five-bit color arithmetic.</summary>
/// <param name="AddToScreen">Treats winning OBJ pixels as the additive subscreen instead of replacing the main pixel.</param>
/// <param name="FixedColor">Optional fixed five-bit RGB value added after OBJ composition.</param>
public sealed record ObjRenderLayer(bool AddToScreen = false, FixedColorAddRenderLayer? FixedColor = null) : RenderLayer;

/// <summary>A scrolled 4-bpp plane using native tilemap and character word addresses.</summary>
/// <param name="TilemapWord">VRAM word address of the source map.</param>
/// <param name="CharacterWord">VRAM word address of the 4-bpp character data.</param>
/// <param name="HorizontalScroll">Wrapped horizontal register offset.</param>
/// <param name="VerticalScroll">Wrapped vertical register offset.</param>
/// <param name="MapWidthTiles">Map width used for tile-coordinate wrapping.</param>
/// <param name="MapHeightTiles">Map height used for tile-coordinate wrapping.</param>
/// <param name="Priority">Optional forced tile priority; null uses each map entry's priority bit.</param>
public sealed record Bg4BppRenderLayer(ushort TilemapWord, ushort CharacterWord,
    ushort HorizontalScroll, ushort VerticalScroll, int MapWidthTiles,
    int MapHeightTiles, bool? Priority) : RenderLayer;

/// <summary>An unscrolled 32-column 2-bpp plane, including the retained pause HUD.</summary>
/// <param name="TilemapWord">VRAM word address of the source map.</param>
/// <param name="CharacterWord">VRAM word address of the 2-bpp character data.</param>
/// <param name="RowCount">Number of tile rows in the map.</param>
/// <param name="Priority">Priority value assigned to this whole plane.</param>
public sealed record Bg2BppRenderLayer(ushort TilemapWord, ushort CharacterWord,
    int RowCount, bool Priority) : RenderLayer;
