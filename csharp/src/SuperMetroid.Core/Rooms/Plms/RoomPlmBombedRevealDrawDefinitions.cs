using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Non-breaking reveals of gated shootable parents in bank $84.</summary>
internal static class RoomPlmBombedRevealDrawDefinitions
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
        int word = PlmDraw.FrameOf<RoomPlmBombedRevealDraw>(request, "RoomPlmBombedRevealDraw") switch
        {
            RoomPlmBombedRevealDraw.PowerBomb => 0xc057,
            RoomPlmBombedRevealDraw.SuperMissile => 0xc09f,
            RoomPlmBombedRevealDraw.CrumbleSingle => 0xb0bc,
            _ => throw new InvalidOperationException($"Undefined bombed-reveal draw {request}."),
        };
        draw = new(request, new RoomPlmShotBlockDrawDefinitions.Run[] { new(PlmRunDirection.Horizontal, new ushort[] { RoomLevelWord.FromRaw(word).Raw }, new(0, 0)) });
        return true;
    }
}
