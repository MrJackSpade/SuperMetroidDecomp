namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// One of Samus's body half pictures: the characters one half of the body shows. The import numbers the pictures once,
/// in the order it stores them; the number is an identity, which gameplay only compares and the body sheets name frames by.
/// </summary>
/// <param name="Ordinal">The picture's place in the import's stored order.</param>
public readonly record struct SamusBodyPicture(int Ordinal);
