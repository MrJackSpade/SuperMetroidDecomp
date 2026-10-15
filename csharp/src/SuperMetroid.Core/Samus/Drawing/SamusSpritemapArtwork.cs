using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Samus spritemaps with the characters their parts name.</summary>
/// <param name="Spritemaps">The installed bank-$92 spritemaps.</param>
/// <param name="Characters">Characters the installed spritemaps' tile numbers index.</param>
/// <param name="PageCharacters">
/// The standard OBJ page with <paramref name="Characters"/> over it, for spritemaps that name characters beyond them
/// (the zero-pointer WRAM spritemap among them); built only when such a spritemap is drawn.
/// </param>
public readonly record struct SamusSpritemapArtwork(
    SamusSpritemapArtworkCatalog Spritemaps,
    SpriteArtwork Characters,
    Func<SpriteArtwork> PageCharacters);
