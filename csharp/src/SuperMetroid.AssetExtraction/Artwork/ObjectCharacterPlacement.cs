namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Where the object characters lie in video memory, as the object-selection byte places them (#2563).</summary>
/// <param name="NameBaseWord">The first name table's base, in video words.</param>
/// <param name="SecondTableDisplacementWords">How far past the first name table the second lies, in video words.</param>
internal readonly record struct ObjectCharacterPlacement(int NameBaseWord, int SecondTableDisplacementWords);
