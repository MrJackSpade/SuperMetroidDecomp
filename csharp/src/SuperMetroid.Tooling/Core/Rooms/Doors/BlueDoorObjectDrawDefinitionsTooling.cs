using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Development-tool members of <see cref="BlueDoorObjectDrawDefinitions"/>; never linked by player hosts.</summary>
internal static class BlueDoorObjectDrawDefinitionsTooling
{
    /// <summary>Enumerates editable door frames, then each orientation's alias frame.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (var frame in RoomObjectBlueDoorVisualFiles.Editable) yield return frame;
            for (int orientation = 0; orientation < 4; orientation++)
            {
                RoomObjectBlueDoorVisualFiles.TryGet(RoomObjectDraw.Of(new BlueDoorCaps.Draw(
                    DoorCapOrientations.FromTableIndex(orientation), BlueDoorObjectDrawDefinitions.AliasFrame)), out var draw);
                yield return draw;
            }
        }
    }
}
