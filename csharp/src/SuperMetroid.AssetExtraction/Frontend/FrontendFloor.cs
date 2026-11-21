using System.Collections.Frozen;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.AssetExtraction.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>A frontend floor sheet and how it composes from the stock artwork (#2563).</summary>
/// <param name="Sheet">The installed sheet.</param>
/// <param name="Compose">How the sheet composes from the stock artwork.</param>
internal sealed record FrontendFloor(SpriteSheetId Sheet, Func<ScreenTileSources, ComposedSpriteSheet> Compose);
