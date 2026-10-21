using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Development-tool members of <see cref="BlueDoorPlmDrawDefinitions"/>; never linked by player hosts.</summary>
internal static class BlueDoorPlmDrawDefinitionsTooling
{
    /// <summary>Enumerates editable door frames, then each orientation's alias frame.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (var frame in BlueDoorPlmDrawDefinitions.Editable) yield return frame;
            for (int orientation = 0; orientation < 4; orientation++)
            {
                BlueDoorPlmDrawDefinitions.TryGet(PlmDraw.Of(new BlueDoorPlmDrawDefinitions.Draw(
                    DoorCapOrientations.FromTableIndex(orientation), BlueDoorPlmDrawDefinitions.AliasFrame)), out var draw);
                yield return draw;
            }
        }
    }
}
