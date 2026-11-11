using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>What the installation holds of one sheet (#1633): its manifest and its decoded pixels and maps.</summary>
/// <param name="Manifest">The installed manifest: the frames, the shades by colour key, the backdrop's key and which maps the sheet has.</param>
/// <param name="Pixels">The installed pixels, with the recolour-region map and the blend map where the sheet has them.</param>
internal sealed record InstalledSheet(InstalledSheetDocument Manifest, SpriteImagePixels Pixels);
