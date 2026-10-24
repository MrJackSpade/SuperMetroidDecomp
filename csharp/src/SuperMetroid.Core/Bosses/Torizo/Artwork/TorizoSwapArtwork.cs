using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo.Artwork;

/// <summary>
/// Editable indexed characters for the native Torizo instruction uploads.
/// Address and destination ownership remain in compiled control data.
/// </summary>
public sealed class TorizoSwapArtwork
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("TorizoInstructionVramArtwork-v1", content =>
        {
            content.Append("pages", pages.Length);
            foreach (RoomCharacterAtlas page in pages)
                content.Append("tiles", page.Characters.Span);
        });

    /// <summary>Validated selected character transfers in tile-sheet definition order.</summary>
    internal readonly RoomCharacterAtlas[] pages;

    /// <summary>Copies and validates one installed character atlas for every required Torizo upload page.</summary>
    internal TorizoSwapArtwork(RoomCharacterAtlas[] pages)
    {
        Ensure.NotNull(pages);
        if (pages.Length != TorizoSwapArtworkDefinitions.All.Length)
            throw new InvalidDataException("Torizo instruction artwork requires every tile page.");
        for (int index = 0; index < pages.Length; index++)
        {
            if (pages[index] is null || pages[index].Characters.Length !=
                TorizoSwapArtworkDefinitions.All[index].ByteCount)
                throw new InvalidDataException(
                    $"Torizo instruction tile page {index} has the wrong size.");
        }
        this.pages = [.. pages];
    }

    /// <summary>The <paramref name="byteCount"/> installed characters of <paramref name="page"/> from byte <paramref name="pageByte"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The range leaves the page.</exception>
    internal ReadOnlyMemory<byte> Characters(TorizoTilePage page, int pageByte, int byteCount) =>
        pages[(int)page].Characters.Slice(pageByte, byteCount);
}
