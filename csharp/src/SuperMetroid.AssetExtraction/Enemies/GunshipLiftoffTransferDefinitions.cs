using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The five gunship takeoff pages, in takeoff order: the steps of its artwork sequence (#1513).</summary>
internal static class GunshipLiftoffTransferDefinitions
{
    /// <summary>The takeoff's five pages, five consecutive installed pages.</summary>
    internal static readonly GunshipLiftoffTransferDefinition[] Frames =
    [
        new(InstalledArtworkId.GunshipLiftoffFirstTiles),
        new(InstalledArtworkId.GunshipLiftoffSecondTiles),
        new(InstalledArtworkId.GunshipLiftoffThirdTiles),
        new(InstalledArtworkId.GunshipLiftoffFourthTiles),
        new(InstalledArtworkId.GunshipLiftoffFifthTiles),
    ];
}
