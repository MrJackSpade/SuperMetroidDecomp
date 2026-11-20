using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A block reaction whose setup saved the terrain at its block (#2484, #2554), which its images restore where they ask for
/// it as the block grows back. Grapple setup saves the earlier block; collision-bomb setup saves the bomb block's parent
/// instead.
/// </summary>
/// <param name="change">The reaction's authored timing.</param>
/// <param name="savedTerrain">The terrain the reaction's setup saved at its block.</param>
internal class RestoringBlockReactionObject(BlockReactionChange change, RoomTerrainBlock savedTerrain) : BlockReactionObject(change)
{
    /// <summary>The terrain the reaction's setup saved at its block, which it restores as the block grows back.</summary>
    internal RoomTerrainBlock SavedTerrain { get; } = savedTerrain;

    /// <inheritdoc/>
    private protected override void RestoreBlock(RoomObjectPlace place, RoomObjectStepContext context) =>
        Ensure.NotNull(context).DrawSavedTerrain(place, SavedTerrain);
}
