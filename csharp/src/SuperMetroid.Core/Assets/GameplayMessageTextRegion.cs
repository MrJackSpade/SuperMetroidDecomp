namespace SuperMetroid.Core.Assets;

/// <summary>Defines one bounded text field in a gameplay-message notice.</summary>
public sealed record GameplayMessageTextRegion
{
    /// <summary>Gets the zero-based content-row index.</summary>
    public required int Row { get; init; }
    /// <summary>Gets the zero-based starting column.</summary>
    public required int Column { get; init; }
    /// <summary>Gets the field width in tilemap cells.</summary>
    public required int Width { get; init; }
    /// <summary>Gets the supported left or center alignment name.</summary>
    public required string Alignment { get; init; }
    /// <summary>Gets the text rendered in this field.</summary>
    public required string Text { get; init; }
    /// <summary>Gets the three-bit tilemap palette index.</summary>
    public required int Palette { get; init; }
}
