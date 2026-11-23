using SuperMetroid.AssetExtraction.Assets;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;

/// <summary>
/// Editable Ceres escape warning and door characters. The transfer order,
/// sizes and destinations remain compiled gameplay/presentation metadata.
/// </summary>
public sealed partial class CeresEscapeTileArtwork
{

    /// <summary>Validated, owned warning-text and door atlases, by the page each holds.</summary>
    private readonly Dictionary<TileSheetDefinition, RoomCharacterAtlas> pages;

    /// <summary>Requires both tile pages at their compiled transfer sizes and copies the supplied atlas references.</summary>
    /// <exception cref="InvalidDataException">A page is missing, extra or of the wrong size.</exception>
    internal CeresEscapeTileArtwork(IReadOnlyDictionary<TileSheetDefinition, RoomCharacterAtlas> pages)
    {
        Ensure.NotNull(pages);
        if (pages.Count != CeresEscapeTileArtworkDefinitions.All.Length)
            throw new InvalidDataException("Ceres escape artwork requires both tile pages.");
        this.pages = [];
        foreach (TileSheetDefinition page in CeresEscapeTileArtworkDefinitions.All)
        {
            if (!pages.TryGetValue(page, out RoomCharacterAtlas? atlas) || atlas.Characters.Length != page.ByteCount)
                throw new InvalidDataException($"Ceres escape tile page {page.FileName} is missing or has the wrong size.");
            this.pages.Add(page, atlas);
        }
    }

    /// <summary>Returns a slice of installed page <paramref name="page"/> only when the requested range fits wholly inside it.</summary>
    /// <param name="page">The page: <see cref="CeresEscapeTileArtworkDefinitions.WarningText"/> or <c>Doors</c>.</param>
    /// <param name="offset">The first byte of the page requested.</param>
    /// <param name="byteCount">The bytes requested.</param>
    /// <param name="characters">The requested characters, or empty when the range is not inside a page.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="page"/> is not one of the Ceres escape pages.</exception>
    internal bool TryResolve(TileSheetDefinition page, int offset, int byteCount,
        out ReadOnlyMemory<byte> characters)
    {
        if (!pages.TryGetValue(page, out RoomCharacterAtlas? held))
            throw new ArgumentOutOfRangeException(nameof(page), page, "Not a Ceres escape tile page.");
        if (offset < 0 || byteCount <= 0 || offset > page.ByteCount - byteCount)
        {
            characters = default;
            return false;
        }
        characters = held.Characters.Slice(offset, byteCount);
        return true;
    }
}
