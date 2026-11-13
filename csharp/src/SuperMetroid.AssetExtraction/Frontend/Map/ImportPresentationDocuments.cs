using SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The map presentation's documents only the import reads, from the component's import data (#1638): their entries are
/// the original's packed tile placements, which the import composes pictures from. The installation never holds them; it
/// holds the pictures, and the game reads none of them.
/// </summary>
/// <param name="FileSelect">
/// The file-select screen's pages, fields and compositions, with the installed layout they are placed by, which the
/// file-select and menu sheets are composed from.
/// </param>
/// <param name="Pause">The pause screen's backdrops, wireframes and equipment-page artwork, pictured into the pause sheet.</param>
internal sealed record ImportPresentationDocuments(FileSelectSourcePresentation FileSelect, PauseScreenArtwork Pause);
