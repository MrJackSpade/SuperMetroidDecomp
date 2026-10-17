namespace SuperMetroid.Core.Rendering;

/// <summary>An unscrolled 32-column 2-bpp plane, including the retained pause HUD.</summary>
/// <param name="TilemapWord">Tilemap base VRAM word address.</param>
/// <param name="CharacterWord">2-bpp character base VRAM word address.</param>
/// <param name="RowCount">Visible tile rows.</param>
/// <param name="Priority">Selects only tiles of one priority.</param>
public sealed record Bg2BppRenderLayer(int TilemapWord, int CharacterWord,
    int RowCount, bool Priority) : RenderLayer
{
    /// <summary>Tilemap base VRAM word address, 0 through <see cref="VramWordRange.MaximumWord"/>.</summary>
    public int TilemapWord
    {
        get => tilemapWord;
        init => tilemapWord = VramWordRange.Require(value);
    }

    /// <summary>Validated store of <see cref="TilemapWord"/>.</summary>
    private readonly int tilemapWord = VramWordRange.Require(TilemapWord);

    /// <summary>2-bpp character base VRAM word address, 0 through <see cref="VramWordRange.MaximumWord"/>.</summary>
    public int CharacterWord
    {
        get => characterWord;
        init => characterWord = VramWordRange.Require(value);
    }

    /// <summary>Validated store of <see cref="CharacterWord"/>.</summary>
    private readonly int characterWord = VramWordRange.Require(CharacterWord);
}
