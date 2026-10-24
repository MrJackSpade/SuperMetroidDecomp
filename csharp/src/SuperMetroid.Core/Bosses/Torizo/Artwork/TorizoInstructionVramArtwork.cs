using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo.Artwork;

/// <summary>
/// Editable indexed characters for the native Torizo instruction uploads.
/// Address and destination ownership remain in compiled control data.
/// </summary>
public sealed class TorizoInstructionVramArtwork
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("TorizoInstructionVramArtwork-v1", content =>
        {
            content.Append("pages", pages.Length);
            foreach (RoomCharacterAtlas page in pages)
                content.Append("tiles", page.Characters.Span);
        });

    /// <summary>Validated selected character transfers in tile-sheet definition order.</summary>
    private readonly RoomCharacterAtlas[] pages;

    /// <summary>Copies and validates one installed character atlas for every required Torizo upload page.</summary>
    internal TorizoInstructionVramArtwork(RoomCharacterAtlas[] pages)
    {
        Ensure.NotNull(pages);
        if (pages.Length != TorizoInstructionVramArtworkDefinitions.All.Length)
            throw new InvalidDataException("Torizo instruction artwork requires every tile page.");
        for (int index = 0; index < pages.Length; index++)
        {
            if (pages[index] is null || pages[index].Characters.Length !=
                TorizoInstructionVramArtworkDefinitions.All[index].ByteCount)
                throw new InvalidDataException(
                    $"Torizo instruction tile page {index} has the wrong size.");
        }
        this.pages = [.. pages];
    }

    /// <summary>Finds the selected bytes for a native upload range wholly contained in one tile page.</summary>
    internal bool TryResolve(int sourceAddress, int byteCount,
        out ReadOnlyMemory<byte> characters)
    {
        for (int index = 0; index < pages.Length; index++)
        {
            TileSheetDefinition page =
                TorizoInstructionVramArtworkDefinitions.All[index];
            int offset = sourceAddress - page.SourceAddress;
            if (offset < 0 || byteCount <= 0 ||
                offset > page.ByteCount - byteCount)
                continue;
            characters = pages[index].Characters.Slice(offset, byteCount);
            return true;
        }
        characters = default;
        return false;
    }
}
