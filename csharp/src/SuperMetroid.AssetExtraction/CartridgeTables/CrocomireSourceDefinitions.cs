using SuperMetroid.AssetExtraction.Bosses.Crocomire;
using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the Crocomire colour and skeleton tables extraction reads (#1413): the import pipeline's
/// private knowledge of the cartridge, decoded once into the installed Crocomire documents and sheets.
/// </summary>
internal static class CrocomireSourceDefinitions
{
    /// <summary>$A4:B89D, the body and head colours restored after hurt flashes.</summary>
    internal const int FightBodyColors = 0xa4b89d;

    /// <summary>$A4:B8BD, the initial wall transfer.</summary>
    internal const int InitialWallColors = 0xa4b8bd;

    /// <summary>$A4:B8DD, the initial projectile transfer.</summary>
    internal const int InitialProjectileColors = 0xa4b8dd;

    /// <summary>$A4:B8FD, the skeleton arm's sprite palette.</summary>
    internal const int SkeletonArmColors = 0xa4b8fd;

    /// <summary>$A4:B91D, the wall spikes' sprite palette.</summary>
    internal const int WallSpikesColors = 0xa4b91d;

    /// <summary>$AD:A600, the first of the skeleton's contiguous character chunks the tables at $A4:99CB/$99D9 select.</summary>
    internal const int SkeletonFirstChunk = 0xada600;

    /// <summary>The cartridge source of skeleton chunk <paramref name="index"/>; the end-of-sequence entry has none.</summary>
    internal static int SkeletonChunk(int index) =>
        SkeletonFirstChunk + Ensure.BetweenInclusive(0, CrocomireSkeletonTransferDefinitions.FrameCount - 1, index) *
            CrocomireSkeletonCharacterDefinitions.ChunkByteCount;
}
