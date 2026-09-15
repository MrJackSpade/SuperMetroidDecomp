using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

/// <summary>Solid obstacle controls after controller-earned temporary boost, without synthetic collision outcomes.</summary>
internal static class TemporaryBlueObstacleProbe
{
    public static void Install(RoomLevelData level, SamusState samus, bool left, int mode)
    {
        int column = (samus.XPosition + (left ? -40 : 40)) >> 4;
        int row = (samus.YPosition - 64) >> 4;
        if ((mode & 1) != 0)
            for (int y = 0; y < level.HeightInBlocks; y++)
                level.SetForegroundEntry(y * level.WidthInBlocks + column, 0x8000);
        if ((mode & 2) != 0)
            for (int x = 0; x < level.WidthInBlocks; x++)
                level.SetForegroundEntry(row * level.WidthInBlocks + x, 0x8000);
    }

    public static void Verify(SamusState samus, int frame, int mode)
    {
        if (frame == 420 && samus.HorizontalSpeed.SpeedBoostCounter != 0x401)
            throw new InvalidDataException("Ceiling contact incorrectly cancelled temporary boost before landing.");
        if (frame == 440 && samus.HorizontalSpeed.SpeedBoostCounter != (mode == 0 ? 0x401 : 0))
            throw new InvalidDataException("Wall contact / ordinary landing did not cancel temporary boost.");
        if (frame == 499 && samus.HorizontalSpeed.SpeedBoostCounter != 0)
            throw new InvalidDataException("Ordinary landing retained temporary boost.");
    }
}
