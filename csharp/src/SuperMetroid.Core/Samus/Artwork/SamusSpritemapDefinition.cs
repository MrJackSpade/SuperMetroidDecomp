using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>One Samus spritemap composition: its editable parts, frontmost first.</summary>
public sealed record SamusSpritemapDefinition(SpritePart[] Parts);
