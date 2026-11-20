using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>The reserve arrow's two animated colours.</summary>
/// <param name="Color6">The colour of the arrow bevel's shade.</param>
/// <param name="Color11">The colour of the arrow bevel's light.</param>
internal readonly record struct ReserveArrowColors(RgbColor Color6, RgbColor Color11);
