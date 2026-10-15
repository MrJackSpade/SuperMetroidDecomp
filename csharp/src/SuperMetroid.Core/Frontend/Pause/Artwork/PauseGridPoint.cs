namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Zero-based destination tile-cell coordinates on a 32-by-32 pause equipment page, distinct from character-atlas coordinates.</summary>
public sealed record PauseGridPoint
{
    /// <summary>Destination column in tile cells, zero through 31.</summary>
    public required int Column { get; init; }
    /// <summary>Destination row in tile cells, zero through 31; the native word offset is <c>Row * 32 + Column</c>.</summary>
    public required int Row { get; init; }
}
