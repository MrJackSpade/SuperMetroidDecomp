using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The room object and terrain change of each of Crocomire's arena mutations.</summary>
internal static class CrocomireArenaMutationDefinitions
{
    /// <summary>The room object each mutation is.</summary>
    internal static readonly IReadOnlyDictionary<CrocomireArenaMutation, RoomObjectHeaderId> RoomObjects =
        new Dictionary<CrocomireArenaMutation, RoomObjectHeaderId>
        {
            [CrocomireArenaMutation.ClearBridge] = RoomObjectHeaderId.ClearCrocomireBridge,
            [CrocomireArenaMutation.CrumbleBridgeBlock] = RoomObjectHeaderId.CrumbleCrocomireBridgeBlock,
            [CrocomireArenaMutation.ClearBridgeBlock] = RoomObjectHeaderId.ClearCrocomireBridgeBlock,
            [CrocomireArenaMutation.ClearInvisibleWall] = RoomObjectHeaderId.ClearCrocomireInvisibleWall,
            [CrocomireArenaMutation.CreateInvisibleWall] = RoomObjectHeaderId.CreateCrocomireInvisibleWall,
        };

    /// <summary>The change each mutation makes after its empty setup: its layout, drawn on its first update; it is gone on the next.</summary>
    internal static readonly IReadOnlyDictionary<CrocomireArenaMutation, TerrainChange> Changes =
        new Dictionary<CrocomireArenaMutation, TerrainChange>
        {
            [CrocomireArenaMutation.ClearBridge] = TerrainChange.DrawnOnce(RoomObjectDraw.Of(CrocomireArenaDraw.ClearBridge)),
            [CrocomireArenaMutation.CrumbleBridgeBlock] = TerrainChange.DrawnOnce(RoomObjectDraw.Of(CrocomireArenaDraw.CrumbleBridgeBlock)),
            [CrocomireArenaMutation.ClearBridgeBlock] = TerrainChange.DrawnOnce(RoomObjectDraw.Of(CrocomireArenaDraw.ClearBridgeBlock)),
            [CrocomireArenaMutation.ClearInvisibleWall] = TerrainChange.DrawnOnce(RoomObjectDraw.Of(CrocomireArenaDraw.ClearInvisibleWall)),
            [CrocomireArenaMutation.CreateInvisibleWall] = TerrainChange.DrawnOnce(RoomObjectDraw.Of(CrocomireArenaDraw.CreateInvisibleWall)),
        };
}
