namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The body halves a Samus composition's picture shows (#1463): a picture showing a half is drawn with the definition
/// record selected for that half, and a picture showing neither shows no record.
/// </summary>
/// <param name="Top">Whether the picture shows the top half's record.</param>
/// <param name="Bottom">Whether the picture shows the bottom half's record.</param>
public readonly record struct SamusBodyHalvesShown(bool Top, bool Bottom);
