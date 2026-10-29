using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>One Samus spritemap composition: its editable parts, frontmost first, which the body sheets are composed from.</summary>
public sealed record SamusSpritemapDefinition(SpritePart[] Parts);
