namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// One Samus spritemap composition: its place in the installed composition list, an identity the import chooses once
/// and composes whole body pictures from (#1585).
/// </summary>
/// <param name="Ordinal">The composition's place in the installed list.</param>
public readonly record struct SamusSpritemapId(int Ordinal);
