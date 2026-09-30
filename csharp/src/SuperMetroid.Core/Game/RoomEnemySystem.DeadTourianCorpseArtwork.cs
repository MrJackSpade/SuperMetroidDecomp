using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// Zoomer, Ripper and Skree copy from the shared installed bank-$B7 source sheet.
    /// Resolving their editable ED7F PNG is mandatory before publishing an owner or
    /// initializing its mutable rot table. There is no cartridge fallback.
    /// </summary>
    private ReadOnlySpan<byte> InstalledDeadTourianCorpseTiles()
    {
        if (TileArtwork is null)
            throw new InvalidDataException("Dead Tourian corpses require installed artwork.");
        if (!TileArtwork.TryResolve(DeadTourianCorpseArtworkDefinitions.SourceAddress,
                DeadTourianCorpseArtworkDefinitions.ByteCount, out ReadOnlyMemory<byte> tiles))
            throw new InvalidDataException(
                "Installed Tourian corpse sheet enemy-ed7f-tiles.png is missing or has the wrong size.");
        return tiles.Span;
    }
}
