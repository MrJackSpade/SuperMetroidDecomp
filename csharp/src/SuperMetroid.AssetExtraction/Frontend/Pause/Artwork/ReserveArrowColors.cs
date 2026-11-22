using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>The reserve arrow's two animated colours.</summary>
/// <param name="Shade">The colour of the arrow bevel's shade.</param>
/// <param name="Light">The colour of the arrow bevel's light.</param>
internal readonly partial record struct ReserveArrowColors(RgbColor Shade, RgbColor Light);
