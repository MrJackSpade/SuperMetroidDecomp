using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>A frontend floor sheet and how it composes from the stock artwork (#2563).</summary>
/// <param name="Sheet">The installed sheet.</param>
/// <param name="Compose">How the sheet composes from the stock artwork.</param>
internal sealed record FrontendFloor(SpriteSheetId Sheet, Func<ScreenTileSources, ComposedSpriteSheet> Compose);
