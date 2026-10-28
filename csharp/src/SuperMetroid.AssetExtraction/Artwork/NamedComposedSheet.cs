namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>A composed sprite sheet and the file name it is stored under.</summary>
/// <param name="Name">The sheet's file name, without extension.</param>
/// <param name="Sheet">The composed sheet.</param>
internal readonly record struct NamedComposedSheet(string Name, ComposedSpriteSheet Sheet);
