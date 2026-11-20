using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Development-tool members of <see cref="BlueDoorObjectDrawDefinitions"/>; never linked by player hosts.</summary>
internal static class BlueDoorObjectDrawDefinitionsTooling
{
    /// <summary>Enumerates editable door frames, then each orientation's closed cap drawn solid.</summary>
    internal static IEnumerable<RoomObjectDrawLists.DrawList> All
    {
        get
        {
            foreach (var frame in RoomObjectBlueDoorVisualFiles.Editable) yield return frame;
            for (int orientation = 0; orientation < 4; orientation++)
            {
                RoomObjectBlueDoorVisualFiles.TryGet(BlueDoorCaps.ClosedSolid(
                    DoorCapOrientations.InTableOrder[orientation]), out var draw);
                yield return draw;
            }
        }
    }
}
