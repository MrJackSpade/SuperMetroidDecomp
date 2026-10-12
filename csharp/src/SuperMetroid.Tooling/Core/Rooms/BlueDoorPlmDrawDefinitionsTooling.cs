namespace SuperMetroid.Core.Rooms;

/// <summary>Development-tool members of <see cref="BlueDoorPlmDrawDefinitions"/>; never linked by player hosts.</summary>
internal static class BlueDoorPlmDrawDefinitionsTooling
{
    /// <summary>Enumerates editable door frames, then the draw list immediately before each orientation's first frame.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (var frame in BlueDoorPlmDrawDefinitions.Editable) yield return frame;
            for (int orientation = 0; orientation < 4; orientation++)
            {
                BlueDoorPlmDrawDefinitions.TryGet((ushort)(BlueDoorPlmDrawDefinitions.LeftFrame0 + orientation * 60 - BlueDoorPlmDrawDefinitions.DrawListBytes), out var draw);
                yield return draw;
            }
        }
    }
}
