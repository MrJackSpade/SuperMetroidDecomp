namespace SuperMetroid.Core.Game;

/// <summary>Native $88:DF94 selection of Draygon's BG2 main-screen HDMA bands.</summary>
public static class DraygonMainScreenWindow
{
    /// <summary>Returns physical visible scanlines [first,end); HUD occupies the first 32 lines.</summary>
    public static (int First, int End) Select(ushort bodyX, ushort bodyY, ushort cameraX, ushort cameraY, bool deleted)
    {
        int x = unchecked((short)(bodyX - cameraX));
        int y = unchecked((short)(bodyY - cameraY));
        if (deleted || x < DraygonBackgroundData.MinimumScreenX || x >= DraygonBackgroundData.MaximumScreenX ||
            y < DraygonBackgroundData.MinimumScreenY || y >= DraygonBackgroundData.MaximumScreenY)
            return (32, 32);
        if (y < DraygonBackgroundData.MiddleScreenY) return (32, DraygonBackgroundData.TopBandEnd);
        if (y < DraygonBackgroundData.BottomScreenY) return (32, 224);
        return (DraygonBackgroundData.BottomBandStart, 224);
    }
}
