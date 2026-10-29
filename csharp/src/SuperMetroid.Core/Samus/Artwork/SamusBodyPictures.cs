namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The half pictures Samus's body shows: each half's, or null for a half no frame has selected yet.</summary>
/// <param name="Top">The top half's picture.</param>
/// <param name="Bottom">The bottom half's picture.</param>
public readonly record struct SamusBodyPictures(SamusBodyPicture? Top, SamusBodyPicture? Bottom);
