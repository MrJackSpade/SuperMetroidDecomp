using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The terrain change of each of Crocomire's arena mutations.</summary>
internal static class CrocomireArenaMutationDefinitions
{
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
