using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>Selects what a pass drawing the room's layer 2 draws.</summary>
internal static class D3D11RoomSources
{
    /// <summary>Added to a source when words written over layer 2 cover it (Tiles.hlsl SourceLayer2Overlay).</summary>
    internal const uint Layer2OverlayFlag = 256;

    /// <summary>
    /// A boss body that owns layer 2 (#1352), else the room's layer-2 plane, else the X-ray reveal (#1392), else the sky's
    /// rows, else the room's library background image (#1346, #1347, #1403), with <see cref="Layer2OverlayFlag"/> when words
    /// are written over layer 2 (#1348).
    /// </summary>
    /// <exception cref="InvalidOperationException">The frame publishes nothing layer 2 shows.</exception>
    internal static uint Layer2Of(RoomBackgrounds rooms) =>
        (uint)(rooms.BossBg2 is not null ? D3D11RoomSource.BossBody
            : rooms.Bg2 is not null ? D3D11RoomSource.Layer2
            : rooms.XrayReveal is not null ? D3D11RoomSource.XrayReveal
            : rooms.Sky is not null ? D3D11RoomSource.Sky
            : rooms.Bg2Image is not null ? D3D11RoomSource.LibraryBackground
            : throw new InvalidOperationException("The frame publishes no layer-2 plane, sky, boss body, reveal or background image for layer 2."))
            | (rooms.Bg2Overlay is null ? 0 : Layer2OverlayFlag);
}
