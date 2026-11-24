using System.Text.Json.Nodes;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// What each explicit scroll layout leaves in the scroll cells after a room's own screens: the cartridge loads fifty
/// cells for such a layout whatever the room's size, and the camera reads the ones past the room at its edges.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>Writes, for each explicit scroll layout by name, the states of the cells after the room's own screens.</summary>
    private void WriteScrollLayouts()
    {
        ScrollCellsPastRoomScreens<RoomScrollState> past = _installation.LoadScrollCellsPastRoomScreens();
        var layouts = new JsonObject();
        foreach (RoomScrollDefinition.ExplicitLayout layout in RoomScrollDefinitions.Explicit)
        {
            var cells = new JsonArray();
            foreach (RoomScrollState state in past.After(layout.Name))
                cells.Add(ScrollStateName(state));
            layouts[layout.Name] = cells;
        }

        _tree.WriteData("rooms/scroll_layouts.json", layouts);
    }

    /// <summary>A scroll state as the Godot game names it.</summary>
    private static string ScrollStateName(RoomScrollState state) => state switch
    {
        RoomScrollState.Closed => "closed",
        RoomScrollState.Open => "open",
        RoomScrollState.OpenAlignedToTop => "open_aligned_to_top",
        _ => throw new InvalidDataException($"Undefined scroll state {state}."),
    };
}
