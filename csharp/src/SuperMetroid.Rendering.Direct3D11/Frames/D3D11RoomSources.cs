using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>Selects the room source a BG2 pass reads.</summary>
internal static class D3D11RoomSources
{
    /// <summary>Added to a room source when words written over BG2 cover it (Tiles.hlsl RoomSourceBg2Overlay).</summary>
    internal const uint Bg2OverlayFlag = 256;

    /// <summary>
    /// A boss body that owns BG2 (#1352), else the room's layer-2 plane, else the sky's rows, else the tilemap BG2's
    /// owner loaded (#1346, #1347), with <see cref="Bg2OverlayFlag"/> when words are written over BG2 (#1348).
    /// </summary>
    internal static uint Bg2Of(RoomBackgrounds rooms) =>
        (uint)(rooms.BossBg2 is not null ? D3D11RoomSource.BossBg2
            : rooms.Bg2 is not null ? D3D11RoomSource.Bg2
            : rooms.Sky is not null ? D3D11RoomSource.Sky
            : D3D11RoomSource.VramTilemap) | (rooms.Bg2Overlay is null ? 0 : Bg2OverlayFlag);
}
