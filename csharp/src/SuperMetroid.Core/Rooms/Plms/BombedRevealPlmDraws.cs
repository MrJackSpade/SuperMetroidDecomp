using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.Plms.RoomPlmBombedRevealDrawDefinitions;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The single-block draw of each bombed reveal.</summary>
internal static class BombedRevealPlmDraws
{
    /// <summary>
    /// Describes a draw pointer handed over by the shared PLM draw interpreter; pointers
    /// outside the three reveals belong to other families.
    /// </summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        if (request is not PlmFrameDraw<RoomPlmBombedRevealDraw>)
        {
            draw = default;
            return false;
        }
        RoomPlmBombedRevealDraw frame = PlmDraw.FrameOf<RoomPlmBombedRevealDraw>(request, "RoomPlmBombedRevealDraw");
        int word = Words.TryGetValue(frame, out ushort found)
            ? found
            : throw new InvalidOperationException($"Undefined bombed-reveal draw {request}.");
        draw = new(request, new RoomPlmShotBlockDrawDefinitions.Run[] { new(PlmRunDirection.Horizontal, new ushort[] { RoomLevelWord.FromRaw(word).Raw }, new(0, 0)) });
        return true;
    }
}
