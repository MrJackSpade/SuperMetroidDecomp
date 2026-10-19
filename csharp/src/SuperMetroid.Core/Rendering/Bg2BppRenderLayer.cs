namespace SuperMetroid.Core.Rendering;

/// <summary>An unscrolled 32-column 2-bpp plane, including the retained pause HUD.</summary>
/// <param name="TilemapWord">Tilemap base VRAM word address.</param>
/// <param name="CharacterWord">2-bpp character base VRAM word address.</param>
/// <param name="RowCount">Visible tile rows.</param>
/// <param name="Priority">Selects only tiles of one priority.</param>
public sealed record Bg2BppRenderLayer(int TilemapWord, int CharacterWord,
    int RowCount, bool Priority) : RenderLayer
{
    /// <summary>Tilemap base VRAM word address, 0 through <c>VramWordRange.MaximumWord</c>.</summary>
    public int TilemapWord
    {
        get;
        init => field = VramWordRange.Require(value);
    } = VramWordRange.Require(TilemapWord);

    /// <summary>2-bpp character base VRAM word address, 0 through <c>VramWordRange.MaximumWord</c>.</summary>
    public int CharacterWord
    {
        get;
        init => field = VramWordRange.Require(value);
    } = VramWordRange.Require(CharacterWord);
}
