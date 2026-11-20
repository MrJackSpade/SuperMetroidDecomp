using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The terrain change of each of Crocomire's arena mutations.</summary>
internal static class CrocomireArenaMutationDefinitions
{
    /// <summary>The change each mutation makes after its empty setup: its layout, drawn on its first update; it is gone on the next.</summary>
    internal static readonly IdentityTable<CrocomireArenaMutation, TerrainChange> Changes = new(new Dictionary<CrocomireArenaMutation, TerrainChange>
        {
            [CrocomireArenaMutation.ClearBridge] = TerrainChange.DrawnOnce(CrocomireArenaDraws.Family.Of(CrocomireArenaDraw.ClearBridge)),
            [CrocomireArenaMutation.CrumbleBridgeBlock] = TerrainChange.DrawnOnce(CrocomireArenaDraws.Family.Of(CrocomireArenaDraw.CrumbleBridgeBlock)),
            [CrocomireArenaMutation.ClearBridgeBlock] = TerrainChange.DrawnOnce(CrocomireArenaDraws.Family.Of(CrocomireArenaDraw.ClearBridgeBlock)),
            [CrocomireArenaMutation.ClearInvisibleWall] = TerrainChange.DrawnOnce(CrocomireArenaDraws.Family.Of(CrocomireArenaDraw.ClearInvisibleWall)),
            [CrocomireArenaMutation.CreateInvisibleWall] = TerrainChange.DrawnOnce(CrocomireArenaDraws.Family.Of(CrocomireArenaDraw.CreateInvisibleWall)),
        });
}
