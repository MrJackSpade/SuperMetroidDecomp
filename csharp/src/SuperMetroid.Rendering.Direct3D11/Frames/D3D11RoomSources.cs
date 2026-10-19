using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>Selects the room source a BG2 pass reads.</summary>
internal static class D3D11RoomSources
{
    /// <summary>The room's layer-2 plane, else the sky's rows, else the tilemap BG2's owner loaded (#1346, #1347).</summary>
    internal static D3D11RoomSource Bg2Of(RoomBackgrounds rooms) =>
        rooms.Bg2 is not null ? D3D11RoomSource.Bg2
            : rooms.Sky is not null ? D3D11RoomSource.Sky
            : D3D11RoomSource.VramTilemap;
}
