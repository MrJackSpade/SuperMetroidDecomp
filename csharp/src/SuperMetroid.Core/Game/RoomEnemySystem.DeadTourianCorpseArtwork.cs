using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// All four corpse families copy from the same native bank-$B7 tile source.
    /// An installed catalog must use its editable ED7F PNG. Constructed cartridge
    /// fixtures with no catalog retain their native read path.
    /// </summary>
    private ReadOnlySpan<byte> InstalledDeadTourianCorpseTiles()
    {
        if (TileArtwork is null)
            return [];
        if (!TileArtwork.TryResolve(DeadTourianCorpseArtworkDefinitions.SourceAddress,
                DeadTourianCorpseArtworkDefinitions.ByteCount, out ReadOnlyMemory<byte> tiles))
            throw new InvalidDataException(
                "Installed Tourian corpse sheet enemy-ed7f-tiles.png is missing or has the wrong size.");
        return tiles.Span;
    }
}
