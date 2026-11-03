using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// A shade and the colours it shows over a run of inks (#1512), before the inks it lays them over are placed: a step of an
/// effect a sheet takes wherever its row lies.
/// </summary>
/// <param name="Shade">The shade's name, which the game's draws give.</param>
/// <param name="Colours">The colours it shows, one for each ink of its run.</param>
internal sealed record ShadeColours(PictureShade Shade, RgbColor[] Colours);
