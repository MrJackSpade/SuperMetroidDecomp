using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.Core;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The blocks each tileset's rooms can show (#1636): every block some room state of the tileset places in its level data
/// where the room shows it, and every block a writer (<see cref="RoomBlockWriters"/>) writes in a state whose installed data
/// holds what starts that writer. A tileset's block sheet pictures these and no other, so tile graphics a room loads among
/// its blocks only for another actor (Ridley's in the Ceres Ridley arena, whose blocks show what the arena loads for his fight) are not pictured
/// as blocks. Ceres's rooms load no common room elements and place no room objects, so nothing writes blocks there. The
/// inventory is the import's private knowledge, read once from the installed stock the sheets compose from: nothing installs
/// it and nothing at run time recomputes it; a room that shows a block its sheet does not picture fails there, naming it.
/// </summary>
internal sealed class RoomBlockInventory
{
    /// <summary>The blocks each inventoried tileset's rooms can show, ascending.</summary>
    private readonly Dictionary<TilesetDefinition, SortedSet<int>> shown;

    /// <summary>Keeps the blocks each tileset's rooms can show.</summary>
    private RoomBlockInventory(Dictionary<TilesetDefinition, SortedSet<int>> shown) => this.shown = shown;

    /// <summary>The blocks the rooms of <paramref name="tileset"/> can show, ascending.</summary>
    /// <exception cref="InvalidDataException">No inventoried room state loads the tileset.</exception>
    internal IReadOnlySet<int> Shown(TilesetDefinition tileset) =>
        shown.TryGetValue(tileset, out SortedSet<int>? blocks) ? blocks
            : throw new InvalidDataException($"No inventoried room state loads tileset {RoomBlockSheet.NameOf(tileset)}.");

    /// <summary>
    /// The inventory of every retail room state, from the levels' installed layouts and terrain and the installed room-object
    /// catalogs and X-ray reveal under the content directory <paramref name="content"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">An installed layout, terrain, catalog or reveal is invalid, or a room state's level is not installed.</exception>
    internal static RoomBlockInventory Load(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        RoomLevelArtworks levels = RoomVisualLayoutFiles.Load(Path.Combine(content, GameInstallationLayout.RoomVisualLayoutDirectoryName));
        InstalledRoomLevelStreams terrain = RoomLevelStreamFiles.Load(Path.Combine(content, GameInstallationLayout.RoomLevelStreamDirectoryName));
        InstalledXrayReveals xray = XrayRevealVisualFiles.Load(Path.Combine(content, GameInstallationLayout.XrayRevealVisualDirectoryName));
        // The import's own decoding of the straddled terrain the game installs, which is not presentation the player edits.
        StraddledTerrain<RoomTerrainBlock> straddled = StraddledTerrainFiles.Read(StraddledTerrainFiles.Decode());
        return Of(RetailStates(levels, terrain, straddled), RoomBlockWriters.Of(RoomObjectVisualCatalogSet.Load(content), xray));
    }

    /// <summary>
    /// The inventory of <paramref name="states"/>, each showing what its level places and what <paramref name="writers"/> its
    /// installed data starts write; a Ceres state shows only what its level places.
    /// </summary>
    internal static RoomBlockInventory Of(IEnumerable<InventoriedRoomState> states, IReadOnlyList<RoomBlockWriter> writers)
    {
        Ensure.NotNull(states);
        Ensure.NotNull(writers);
        var shown = new Dictionary<TilesetDefinition, SortedSet<int>>();
        foreach (InventoriedRoomState state in states)
        {
            Ensure.NotNull(state);
            if (!shown.TryGetValue(state.Tileset, out SortedSet<int>? blocks))
                shown.Add(state.Tileset, blocks = []);
            // A state whose screen is the Mode 7 floor shows none of its level's blocks (#1647).
            if (!state.DrawsLevel)
                continue;
            blocks.UnionWith(state.Placed);
            if (state.InCeres)
                continue;
            foreach (RoomBlockWriter writer in writers)
            {
                if (writer.RunsIn(state))
                    blocks.UnionWith(writer.Blocks);
            }
        }
        return new RoomBlockInventory(shown);
    }

    /// <summary>
    /// Every retail room state, with the level layout it shows, its installed terrain, the shootable reactions a Wave
    /// shot's straddled read sets off there through <paramref name="straddled"/> (read once for each level the states
    /// share), and the doors into its room.
    /// </summary>
    private static IEnumerable<InventoriedRoomState> RetailStates(RoomLevelArtworks levels, InstalledRoomLevelStreams terrain,
        StraddledTerrain<RoomTerrainBlock> straddled)
    {
        ILookup<RoomDefinition, DoorDefinition> entrances = RoomDoorListDefinitions.ByRoom.Values
            .SelectMany(list => list)
            .Select(entry => entry.Door)
            .OfType<DoorDefinition>()
            .ToLookup(door => door.DestinationRoom);
        var straddledShots = new Dictionary<InstalledRoomLevel, IReadOnlySet<RoomTerrainReaction>>();
        foreach (RoomDefinition room in RetailRooms.All)
        {
            foreach (RoomStateSettings state in RoomStateLists.GetStates(room))
            {
                InstalledRoomLevel installed = terrain.Get(state.Level);
                if (!straddledShots.TryGetValue(installed, out IReadOnlySet<RoomTerrainReaction>? shots))
                    straddledShots.Add(installed, shots = StraddledShotReactions.Of(installed, straddled));
                yield return InventoriedRoomState.Of(room, state, levels.Get(state.Level), installed, shots, entrances[room]);
            }
        }
    }
}
