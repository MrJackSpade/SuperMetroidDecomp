using static SuperMetroid.Core.Rooms.Plms.CrateriaMainstreetEscapePassagePlmDefinitions;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The open-passage draw of Crateria's main street escape.</summary>
internal static class CrateriaMainstreetEscapePassageDraws
{
    /// <summary>Recognizes the escape passage draw pointer and supplies its two adjacent air blocks.</summary>
    internal static bool TryGetDraw(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        if (request != ClearPairDraw)
        {
            draw = default;
            return false;
        }
        draw = new(request, new RoomPlmShotBlockDrawDefinitions.Run[]
        {
            new(PlmRunDirection.Horizontal, new ushort[] { PlmLevelWordDefinitions.BlankAir, PlmLevelWordDefinitions.BlankAir }, new(0, 0)),
        });
        return true;
    }
}
