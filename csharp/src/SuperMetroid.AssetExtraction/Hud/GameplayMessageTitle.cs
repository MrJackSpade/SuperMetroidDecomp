namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>One selected title string and BG palette; its placement is centered by the builder rather than supplied as gameplay state.</summary>
public sealed record GameplayMessageTitle
{
    /// <summary>Nonempty text of at most nineteen characters, using uppercase A..Z, space, hyphen, period or question mark; no arbitrary Unicode glyph substitution is performed.</summary>
    public required string Text { get; init; }
    /// <summary>BG palette selector 0..7 applied to title glyphs and the space-filled interior, with BG priority set and no reflection flags.</summary>
    public required int Palette { get; init; }
}
