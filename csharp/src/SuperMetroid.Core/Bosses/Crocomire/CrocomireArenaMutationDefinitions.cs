using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The room PLM and program of each of Crocomire's arena mutations.</summary>
internal static class CrocomireArenaMutationDefinitions
{
    /// <summary>The room PLM each mutation occupies its slot as.</summary>
    internal static readonly IReadOnlyDictionary<CrocomireArenaMutation, RoomObjectHeaderId> RoomObjects =
        new Dictionary<CrocomireArenaMutation, RoomObjectHeaderId>
        {
            [CrocomireArenaMutation.ClearBridge] = RoomObjectHeaderId.ClearCrocomireBridge,
            [CrocomireArenaMutation.CrumbleBridgeBlock] = RoomObjectHeaderId.CrumbleCrocomireBridgeBlock,
            [CrocomireArenaMutation.ClearBridgeBlock] = RoomObjectHeaderId.ClearCrocomireBridgeBlock,
            [CrocomireArenaMutation.ClearInvisibleWall] = RoomObjectHeaderId.ClearCrocomireInvisibleWall,
            [CrocomireArenaMutation.CreateInvisibleWall] = RoomObjectHeaderId.CreateCrocomireInvisibleWall,
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
