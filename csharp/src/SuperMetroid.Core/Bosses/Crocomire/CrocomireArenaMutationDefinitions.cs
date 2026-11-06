using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The room PLM and program of each of Crocomire's arena mutations.</summary>
internal static class CrocomireArenaMutationDefinitions
{
    /// <summary>The room PLM each mutation occupies its slot as.</summary>
    internal static readonly IReadOnlyDictionary<CrocomireArenaMutation, PlmHeaderId> Plms =
        new Dictionary<CrocomireArenaMutation, PlmHeaderId>
        {
            [CrocomireArenaMutation.ClearBridge] = PlmHeaderId.ClearCrocomireBridge,
            [CrocomireArenaMutation.CrumbleBridgeBlock] = PlmHeaderId.CrumbleCrocomireBridgeBlock,
            [CrocomireArenaMutation.ClearBridgeBlock] = PlmHeaderId.ClearCrocomireBridgeBlock,
            [CrocomireArenaMutation.ClearInvisibleWall] = PlmHeaderId.ClearCrocomireInvisibleWall,
            [CrocomireArenaMutation.CreateInvisibleWall] = PlmHeaderId.CreateCrocomireInvisibleWall,
        };

    /// <summary>The arena program each mutation runs after its empty setup.</summary>
    internal static readonly IReadOnlyDictionary<CrocomireArenaMutation, CrocomireArenaProgram> Programs =
        new Dictionary<CrocomireArenaMutation, CrocomireArenaProgram>
        {
            [CrocomireArenaMutation.ClearBridge] = CrocomireArenaProgram.ClearBridge,
            [CrocomireArenaMutation.CrumbleBridgeBlock] = CrocomireArenaProgram.CrumbleBridgeBlock,
            [CrocomireArenaMutation.ClearBridgeBlock] = CrocomireArenaProgram.ClearBridgeBlock,
            [CrocomireArenaMutation.ClearInvisibleWall] = CrocomireArenaProgram.ClearInvisibleWall,
            [CrocomireArenaMutation.CreateInvisibleWall] = CrocomireArenaProgram.CreateInvisibleWall,
        };
}
