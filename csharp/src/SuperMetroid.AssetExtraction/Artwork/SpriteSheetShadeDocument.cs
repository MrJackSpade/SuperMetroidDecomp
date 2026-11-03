namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>One shade's manifest entry (#1512): the inks it recolours and each one's colour as a packed colour word.</summary>
public sealed record SpriteSheetShadeDocument
{
    /// <summary>The inks recoloured, in rising order.</summary>
    public required int[] Inks { get; init; }

    /// <summary>Each ink's colour, as a packed colour word.</summary>
    public required ushort[] Colours { get; init; }
}
