using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Samus.Palettes;

/// <summary>The first and second of one verify normal suit catalog boundary case.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
internal readonly record struct VerifyNormalSuitCatalogBoundaryCase(Dictionary<ColourCell, RgbColor> First, int Second);
