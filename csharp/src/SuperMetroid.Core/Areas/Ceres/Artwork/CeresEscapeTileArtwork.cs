using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>
/// Editable Ceres escape warning and door characters. The native transfer order,
/// sizes and destinations remain compiled gameplay/presentation metadata.
/// </summary>
public sealed class CeresEscapeTileArtwork
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("CeresEscapeTileArtwork-v1", content =>
        {
            content.Append("pages", pages.Length);
            foreach (RoomCharacterAtlas page in pages)
                content.Append("tiles", page.Transfer.Span);
        });

    /// <summary>Validated, owned warning-text and door atlases in native transfer order.</summary>
    private readonly RoomCharacterAtlas[] pages;

    /// <summary>Requires both tile pages at their compiled transfer sizes and copies the supplied atlas references.</summary>
    internal CeresEscapeTileArtwork(RoomCharacterAtlas[] pages)
    {
        Ensure.NotNull(pages);
        if (pages.Length != CeresEscapeTileArtworkDefinitions.All.Length)
            throw new InvalidDataException("Ceres escape artwork requires both tile pages.");
        for (int index = 0; index < pages.Length; index++)
        {
            if (pages[index] is null || pages[index].Transfer.Length !=
                CeresEscapeTileArtworkDefinitions.All[index].ByteCount)
                throw new InvalidDataException(
                    $"Ceres escape tile page {index} has the wrong size.");
        }
        this.pages = [.. pages];
    }

    /// <summary>Returns a slice from the selected installed page only when the requested transfer fits wholly inside it.</summary>
    internal bool TryResolve(int sourceAddress, int byteCount,
        out ReadOnlyMemory<byte> characters)
    {
        for (int index = 0; index < pages.Length; index++)
        {
            CeresEscapeTileSheetDefinition page =
                CeresEscapeTileArtworkDefinitions.All[index];
            int offset = sourceAddress - page.SourceAddress;
            if (offset < 0 || byteCount <= 0 ||
                offset > page.ByteCount - byteCount)
                continue;
            characters = pages[index].Transfer.Slice(offset, byteCount);
            return true;
        }
        characters = default;
        return false;
    }
}
