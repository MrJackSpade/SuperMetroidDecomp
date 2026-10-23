using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>Selects the room source a BG2 pass reads.</summary>
internal static class D3D11RoomSources
{
    /// <summary>Added to a room source when words written over BG2 cover it (Tiles.hlsl RoomSourceBg2Overlay).</summary>
    internal const uint Bg2OverlayFlag = 256;

    /// <summary>
    /// A boss body that owns BG2 (#1352), else the room's layer-2 plane, else the X-ray reveal (#1392), else the sky's
    /// rows, else the room's library background image (#1346, #1347, #1403), with <see cref="Bg2OverlayFlag"/> when words
    /// are written over BG2 (#1348).
    /// </summary>
    /// <exception cref="InvalidOperationException">The frame publishes nothing BG2 shows.</exception>
    internal static uint Bg2Of(RoomBackgrounds rooms) =>
        (uint)(rooms.BossBg2 is not null ? D3D11RoomSource.BossBg2
            : rooms.Bg2 is not null ? D3D11RoomSource.Bg2
            : rooms.XrayReveal is not null ? D3D11RoomSource.XrayReveal
            : rooms.Sky is not null ? D3D11RoomSource.Sky
            : rooms.Bg2Image is not null ? D3D11RoomSource.Bg2Image
            : throw new InvalidOperationException("The frame publishes no layer-2 plane, sky, boss body, reveal or background image for BG2."))
            | (rooms.Bg2Overlay is null ? 0 : Bg2OverlayFlag);
}
