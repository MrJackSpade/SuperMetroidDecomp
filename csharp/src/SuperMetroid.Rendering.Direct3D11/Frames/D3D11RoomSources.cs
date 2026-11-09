using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>Selects what a pass drawing the room's background draws.</summary>
internal static class D3D11RoomSources
{
    /// <summary>Added to a source when pictures written over the background cover it (Tiles.hlsl SourceWrittenOverBackground).</summary>
    internal const uint WrittenOverBackgroundFlag = 256;

    /// <summary>
    /// A boss body the background shows (#1352), else the room's background plane, else the X-ray reveal (#1392), else the
    /// sky's rows, else the room's library background image (#1346, #1347, #1403), with <see cref="WrittenOverBackgroundFlag"/>
    /// when pictures are written over the background (#1348).
    /// </summary>
    /// <exception cref="InvalidOperationException">The frame publishes nothing the background shows.</exception>
    internal static uint BackgroundSourceOf(RoomBackgrounds rooms) =>
        (uint)(rooms.BossBody is not null ? D3D11RoomSource.BossBody
            : rooms.Background is not null ? D3D11RoomSource.Background
            : rooms.XrayReveal is not null ? D3D11RoomSource.XrayReveal
            : rooms.Sky is not null ? D3D11RoomSource.Sky
            : rooms.BackgroundImage is not null ? D3D11RoomSource.LibraryBackground
            : throw new InvalidOperationException("The frame publishes no background plane, sky, boss body, reveal or background image for the background."))
            | (rooms.BackgroundText is null ? 0 : WrittenOverBackgroundFlag);
}
