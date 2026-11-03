using SuperMetroid.AssetExtraction.Samus.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Samus.Palettes;

/// <summary>The first and second of one verify normal suit catalog boundary case.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
internal readonly record struct VerifyNormalSuitCatalogBoundaryCase(Dictionary<TableColourPlace, RgbColor> First, int Second);
