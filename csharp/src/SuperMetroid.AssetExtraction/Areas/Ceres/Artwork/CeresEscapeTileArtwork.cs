using SuperMetroid.AssetExtraction.Assets;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;

/// <summary>
/// Editable Ceres escape warning and door characters. The transfer order,
/// sizes and destinations remain compiled gameplay/presentation metadata.
/// </summary>
public sealed class CeresEscapeTileArtwork
{

    /// <summary>Validated, owned warning-text and door atlases in transfer order.</summary>
    private readonly RoomCharacterAtlas[] pages;

    /// <summary>Requires both tile pages at their compiled transfer sizes and copies the supplied atlas references.</summary>
    internal CeresEscapeTileArtwork(RoomCharacterAtlas[] pages)
    {
        Ensure.NotNull(pages);
        if (pages.Length != CeresEscapeTileArtworkDefinitions.All.Length)
            throw new InvalidDataException("Ceres escape artwork requires both tile pages.");
        for (int index = 0; index < pages.Length; index++)
        {
            if (pages[index] is null || pages[index].Characters.Length !=
                CeresEscapeTileArtworkDefinitions.All[index].ByteCount)
                throw new InvalidDataException(
                    $"Ceres escape tile page {index} has the wrong size.");
        }
        this.pages = [.. pages];
    }

    /// <summary>Returns a slice of installed page <paramref name="page"/> only when the requested range fits wholly inside it.</summary>
    /// <param name="page">The page: <see cref="CeresEscapeTileArtworkDefinitions.WarningText"/> or <see cref="CeresEscapeTileArtworkDefinitions.Doors"/>.</param>
    /// <param name="offset">The first byte of the page requested.</param>
    /// <param name="byteCount">The bytes requested.</param>
    /// <param name="characters">The requested characters, or empty when the range is not inside a page.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="page"/> is not one of the Ceres escape pages.</exception>
    internal bool TryResolve(TileSheetDefinition page, int offset, int byteCount,
        out ReadOnlyMemory<byte> characters)
    {
        // The pages are held in their definitions' transfer order.
        int held = CeresEscapeTileArtworkDefinitions.All.IndexOf(page);
        if (held < 0)
            throw new ArgumentOutOfRangeException(nameof(page), page, "Not a Ceres escape tile page.");
        if (offset < 0 || byteCount <= 0 || offset > page.ByteCount - byteCount)
        {
            characters = default;
            return false;
        }
        characters = pages[held].Characters.Slice(offset, byteCount);
        return true;
    }
}
