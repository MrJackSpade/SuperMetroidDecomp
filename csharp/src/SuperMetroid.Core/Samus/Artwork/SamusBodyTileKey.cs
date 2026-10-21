namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>A tile of a Samus body transfer.</summary>
/// <param name="Upper">The upper.</param>
/// <param name="Set">The set.</param>
/// <param name="Position">The position.</param>
/// <param name="Tile">The tile.</param>
internal readonly record struct SamusBodyTileKey(bool Upper, int Set, int Position, int Tile);
