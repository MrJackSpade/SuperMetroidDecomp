using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>A composed sheet as the import installs it (#1544): its installed manifest and each pixel's ink.</summary>
/// <param name="Manifest">The manifest naming the image's own colours.</param>
/// <param name="Inks">Each pixel's ink, row by row from the top, naming its colour by its place among the image's colours.</param>
internal sealed record InstalledSheetLayers(InstalledSheetDocument Manifest, SpriteImageInk[] Inks);
