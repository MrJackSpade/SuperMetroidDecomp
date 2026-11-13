using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The installed documents an installed map presentation was read with that the import composes pictures from (#1466,
/// #1467): the game reads none of them. The documents whose entries are the original's packed placements are import data
/// (#1638, <see cref="ImportPresentationDocuments"/>). The colours only the import pre-colours sheets
/// from are not among them: the game's read leaves them unread (#1544, <see cref="ImportPresentationColours"/>), and the
/// characters are import data the installation never holds (#1636, <see cref="ImportPresentationCharacters"/>).
/// </summary>
/// <param name="StaffCredits">The editable staff credits, pictured into the ending's credits rows.</param>
/// <param name="HighlightColours">
/// The map highlight cycle with its colours, which the map sprite sheet's highlight shades are pre-coloured from (#1512).
/// The game reads the same document for each frame's hold: a frame's hold and its look are one record of the effect.
/// </param>
[ArchitectureReview("context-bag", "The installed documents an installed map presentation was read with; every member is immutable artwork data the import composes pictures from, not a service.")]
internal sealed record InstalledPresentationSources(
    CreditsPresentation StaffCredits,
    MapPaletteCycle HighlightColours);
