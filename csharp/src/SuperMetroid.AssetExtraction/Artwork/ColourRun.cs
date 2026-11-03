using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// A run of colours a load writes over consecutive inks (#1512): the first ink it writes, and the colour of each ink from
/// there on. An ink is its colour group's place times sixteen plus its colour.
/// </summary>
/// <param name="FirstInk">The first ink the run writes.</param>
/// <param name="Colours">The colour of each ink of the run, in order.</param>
internal readonly record struct ColourRun(int FirstInk, RgbColor[] Colours);
