namespace SuperMetroid.Core.Rendering;

/// <summary>An unscrolled 32-column 2-bpp plane, including the retained pause HUD.</summary>
public sealed record Bg2BppRenderLayer(ushort TilemapWord, ushort CharacterWord,
    int RowCount, bool Priority) : RenderLayer;
