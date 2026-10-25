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
                content.Append("tiles", page.Characters.Span);
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
            if (pages[index] is null || pages[index].Characters.Length !=
                CeresEscapeTileArtworkDefinitions.All[index].ByteCount)
                throw new InvalidDataException(
                    $"Ceres escape tile page {index} has the wrong size.");
        }
        this.pages = [.. pages];
    }

    /// <summary>Returns a slice of installed page <paramref name="page"/> only when the requested range fits wholly inside it.</summary>
    /// <param name="page">The page: 0 for the warning text, 1 for the doors (<see cref="CeresEscapeTileArtworkDefinitions.All"/>).</param>
    /// <param name="offset">The first byte of the page requested.</param>
    /// <param name="byteCount">The bytes requested.</param>
    /// <param name="characters">The requested characters, or empty when the range is not inside a page.</param>
    internal bool TryResolve(int page, int offset, int byteCount,
        out ReadOnlyMemory<byte> characters)
    {
        if ((uint)page >= (uint)pages.Length || offset < 0 || byteCount <= 0 ||
            offset > CeresEscapeTileArtworkDefinitions.All[page].ByteCount - byteCount)
        {
            characters = default;
            return false;
        }
        characters = pages[page].Characters.Slice(offset, byteCount);
        return true;
    }
}
