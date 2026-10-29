namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// One Samus spritemap composition: its place in the installed composition list, an identity the import chooses once,
/// which gameplay only compares and the body sheets name frames by.
/// </summary>
/// <param name="Ordinal">The composition's place in the installed list.</param>
public readonly record struct SamusSpritemapId(int Ordinal);
