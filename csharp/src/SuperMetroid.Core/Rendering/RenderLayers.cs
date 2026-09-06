namespace SuperMetroid.Core.Rendering;

/// <summary>One immutable insertion into an explicitly ordered PPU priority ladder.</summary>
public abstract record RenderLayer;

/// <summary>Inserts only pixels whose winning OAM record has this priority.</summary>
public sealed record ObjPriorityRenderLayer(byte Priority) : RenderLayer;

/// <summary>A scrolled 4-bpp plane using native tilemap and character word addresses.</summary>
public sealed record Bg4BppRenderLayer(ushort TilemapWord, ushort CharacterWord,
    ushort HorizontalScroll, ushort VerticalScroll, int MapWidthTiles,
    int MapHeightTiles, bool Priority) : RenderLayer;

/// <summary>An unscrolled 32-column 2-bpp plane, including the retained pause HUD.</summary>
public sealed record Bg2BppRenderLayer(ushort TilemapWord, ushort CharacterWord,
    int RowCount, bool Priority) : RenderLayer;
