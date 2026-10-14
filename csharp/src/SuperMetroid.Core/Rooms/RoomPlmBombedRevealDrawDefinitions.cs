namespace SuperMetroid.Core.Rooms;

/// <summary>Non-breaking reveals of gated shootable parents in bank $84.</summary>
internal static class RoomPlmBombedRevealDrawDefinitions
{
    /// <summary>
    /// Describes a draw pointer handed over by the shared PLM draw interpreter; pointers
    /// outside the three reveals belong to other families.
    /// </summary>
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        if (!Enum.IsDefined((RoomPlmBombedRevealDraw)pointer))
        {
            draw = default;
            return false;
        }
        ushort word = (RoomPlmBombedRevealDraw)pointer switch
        {
            RoomPlmBombedRevealDraw.PowerBomb => 0xc057,
            RoomPlmBombedRevealDraw.SuperMissile => 0xc09f,
            RoomPlmBombedRevealDraw.CrumbleSingle => 0xb0bc,
            _ => throw new InvalidOperationException($"Undefined bombed-reveal draw ${pointer:X4}."),
        };
        draw = new(pointer, new RoomPlmShotBlockDrawDefinitions.Run[] { new(1, new ushort[] { word }, new(0, 0)) });
        return true;
    }
}
