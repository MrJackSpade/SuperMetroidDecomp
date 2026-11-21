using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Torizo;

/// <summary>
/// Editable indexed characters for the native Torizo instruction uploads.
/// Address and destination ownership remain in compiled control data.
/// </summary>
public sealed class TorizoSwapArtwork
{

    /// <summary>Validated selected character transfers, by the tile page each holds.</summary>
    private readonly Dictionary<TorizoTilePage, RoomCharacterAtlas> pages;

    /// <summary>Copies and validates one installed character atlas for every required Torizo upload page.</summary>
    /// <exception cref="InvalidDataException">A page is missing, extra or of the wrong size.</exception>
    internal TorizoSwapArtwork(IReadOnlyDictionary<TorizoTilePage, RoomCharacterAtlas> pages)
    {
        Ensure.NotNull(pages);
        if (pages.Count != TorizoSwapArtworkDefinitions.All.Length)
            throw new InvalidDataException("Torizo instruction artwork requires every tile page.");
        this.pages = [];
        foreach (TorizoTileSheet sheet in TorizoSwapArtworkDefinitions.All)
        {
            if (!pages.TryGetValue(sheet.Page, out RoomCharacterAtlas? atlas) || atlas.Characters.Length != sheet.ByteCount)
                throw new InvalidDataException($"Torizo instruction tile page {sheet.Page} is missing or has the wrong size.");
            this.pages.Add(sheet.Page, atlas);
        }
    }

    /// <summary>The <paramref name="byteCount"/> installed characters of <paramref name="page"/> from byte <paramref name="pageByte"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The range leaves the page.</exception>
    internal ReadOnlyMemory<byte> Characters(TorizoTilePage page, int pageByte, int byteCount) =>
        Page(page).Characters.Slice(pageByte, byteCount);

    /// <summary>The installed atlas of <paramref name="page"/>, found by its tile sheet's identity.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No such page is installed.</exception>
    private RoomCharacterAtlas Page(TorizoTilePage page) =>
        pages.TryGetValue(page, out RoomCharacterAtlas? atlas) ? atlas : throw new ArgumentOutOfRangeException(nameof(page), page, "Undefined Torizo tile page.");
}
