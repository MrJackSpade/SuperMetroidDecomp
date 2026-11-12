using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Xray;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// What of one room state decides the blocks it can show (#1636): the tileset whose block sheet pictures them, the blocks
/// its level data places where the room shows them, and what decides which writers of blocks run in it (the area its room
/// lies in, the room objects and enemies its populations place, its main routine and its X-ray overlay).
/// </summary>
/// <param name="Tileset">The tileset the state loads, whose block sheet pictures the blocks it shows.</param>
/// <param name="Area">The area the state's room lies in.</param>
/// <param name="Placed">
/// Every block the state's level data places where the room shows it: in its foreground, and in its background where the
/// state's background shows its level data.
/// </param>
/// <param name="Headers">The room objects the state's population places.</param>
/// <param name="Enemies">The enemies the state's population places.</param>
/// <param name="MainCallback">The state's main routine.</param>
/// <param name="XrayOverlay">The X-ray reveal's overlay of the state, or null where it has none.</param>
internal sealed record InventoriedRoomState(TilesetDefinition Tileset, AreaId Area, IReadOnlySet<int> Placed,
    IReadOnlySet<RoomObjectHeaderId> Headers, IReadOnlySet<EnemyDefinitionId> Enemies, RoomMainCallback MainCallback,
    XrayRoomOverlayDefinition? XrayOverlay)
{
    /// <summary>Whether the state's room lies in Ceres, whose rooms load no common room elements and place no room objects.</summary>
    internal bool InCeres => Area == AreaId.Ceres;

    /// <summary>
    /// What of <paramref name="state"/> of <paramref name="room"/>, whose level data shows <paramref name="level"/>, decides
    /// the blocks it can show. Its background's blocks are shown only where its background scrolls over its level data,
    /// as the room streams it; otherwise the background shows a library background or nothing of the level.
    /// </summary>
    internal static InventoriedRoomState Of(RoomHeaderDefinition room, RoomStateSettings state, RoomLevelArtwork level)
    {
        Ensure.NotNull(room);
        Ensure.NotNull(state);
        Ensure.NotNull(level);
        var placed = new HashSet<int>();
        foreach (RoomBlockAppearance block in level.Foreground.Span)
            placed.Add(block.Block);
        if (state.BackgroundScrollModes.Horizontal.Streams || state.BackgroundScrollModes.Vertical.Streams)
        {
            foreach (RoomBlockAppearance block in level.Background.Span)
                placed.Add(block.Block);
        }
        var headers = new HashSet<RoomObjectHeaderId>();
        foreach (RoomObjectPlacement placement in state.ObjectPopulation.Placements.Span)
            headers.Add(placement.Header.Header);
        var enemies = new HashSet<EnemyDefinitionId>();
        foreach (RoomEnemyPopulationRecord record in state.EnemyPopulation.Records.Span)
            enemies.Add(record.DefinitionId);
        return new InventoriedRoomState(RoomTilesets.Get(state.GraphicsSet), room.AreaIndex, placed, headers, enemies, state.MainCallback,
            state.XrayOverlay);
    }
}
