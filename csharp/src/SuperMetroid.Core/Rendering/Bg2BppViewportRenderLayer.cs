namespace SuperMetroid.Core.Rendering;

/// <summary>A 256-pixel-wide window into a wrapping 32-row 2-bpp tilemap.</summary>
/// <param name="TilemapWord">Tilemap base VRAM word address.</param>
/// <param name="CharacterWord">2-bpp character base VRAM word address.</param>
/// <param name="VerticalScroll">Vertical scroll register value in pixels, before tilemap wrapping.</param>
/// <param name="TransparentColorZero">Whether pen zero is transparent.</param>
/// <param name="Priority">Selects only tiles of one priority, or null for both.</param>
[IndependentFlag("the layer's colour-zero transparency, an independent PPU setting")]
public sealed record Bg2BppViewportRenderLayer(int TilemapWord, int CharacterWord,
    int VerticalScroll, bool TransparentColorZero, bool? Priority) : RenderLayer
{
    /// <summary>The largest scroll register value.</summary>
    public const int MaximumScroll = ushort.MaxValue;

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

    /// <summary>Vertical scroll in pixels, 0 through <see cref="MaximumScroll"/>; the sampler wraps it at the tilemap height.</summary>
    public int VerticalScroll
    {
        get;
        init => field = Ensure.BetweenInclusive(0, MaximumScroll, value);
    } = Ensure.BetweenInclusive(0, MaximumScroll, VerticalScroll);
}
