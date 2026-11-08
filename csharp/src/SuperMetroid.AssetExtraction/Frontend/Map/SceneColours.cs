using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The installed scene colours as the game reads them (#1544): colours the game shows that no picture holds. The game
/// loads no palette document for them; every picture it draws is pre-coloured by the import.
/// </summary>
/// <param name="TitleBackdrop">The title's backdrop.</param>
/// <param name="Menus">Each menu screen's backdrop.</param>
/// <param name="CeresHaze">The Ceres haze's two tints.</param>
public sealed record SceneColours(RgbColor TitleBackdrop, MenuBackdrops Menus, CeresHazeTints CeresHaze);
