namespace SuperMetroid.Core.Rendering;

/// <summary>A scrolled 4-bpp plane using tilemap and character base VRAM word addresses.</summary>
/// <param name="TilemapWord">Tilemap base VRAM word address.</param>
/// <param name="CharacterWord">4-bpp character base VRAM word address.</param>
/// <param name="Scroll">The BGnHOFS/BGnVOFS register pair, before tilemap wrapping.</param>
/// <param name="MapSize">Tilemap size in eight-pixel cells, 32 or 64 on each axis.</param>
/// <param name="Priority">Selects only tiles of one priority, or null for both.</param>
public sealed record Bg4BppRenderLayer(int TilemapWord, int CharacterWord,
    Position16<BgPlanePixels> Scroll, Extent16<TilemapCells> MapSize, bool? Priority) : RenderLayer
{
    /// <summary>Tilemap base VRAM word address, 0 through <see cref="VramWordRange.MaximumWord"/>.</summary>
    public int TilemapWord
    {
        get => tilemapWord;
        init => tilemapWord = VramWordRange.Require(value);
    }

    /// <summary>Validated store of <see cref="TilemapWord"/>.</summary>
    private readonly int tilemapWord = VramWordRange.Require(TilemapWord);

    /// <summary>4-bpp character base VRAM word address, 0 through <see cref="VramWordRange.MaximumWord"/>.</summary>
    public int CharacterWord
    {
        get => characterWord;
        init => characterWord = VramWordRange.Require(value);
    }

    /// <summary>Validated store of <see cref="CharacterWord"/>.</summary>
    private readonly int characterWord = VramWordRange.Require(CharacterWord);
}
