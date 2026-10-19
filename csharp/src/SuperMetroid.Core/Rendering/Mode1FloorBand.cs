using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>Mode-1 BG2 and OBJ register values after a bottom-of-screen video-mode switch.</summary>
/// <param name="FirstScanline">Inclusive physical screen Y where the lower band begins; sampling does not restart its scroll origin here.</param>
/// <param name="TilemapWord">BG2 tilemap base VRAM word address.</param>
/// <param name="CharacterWord">BG2 4-bpp character base VRAM word address.</param>
/// <param name="Scroll">Native BG2 scroll in pixels, retained as its unsigned register words.</param>
/// <param name="MapSize">BG2 size in eight-pixel cells, 32 or 64 on each axis when used by a gameplay layer.</param>
/// <param name="Room">The room's layer-2 plane the band shows (#1346), or null when it shows the BG2 tilemap.</param>
public readonly record struct Mode1FloorBand(int FirstScanline, int TilemapWord, int CharacterWord,
    Position16<Bg2Pixels> Scroll, Extent16<TilemapCells> MapSize, RoomBackgroundLayer? Room = null)
{
    /// <summary>BG2 tilemap base VRAM word address, 0 through <c>VramWordRange.MaximumWord</c>.</summary>
    public int TilemapWord
    {
        get;
        init => field = VramWordRange.Require(value);
    } = VramWordRange.Require(TilemapWord);

    /// <summary>BG2 4-bpp character base VRAM word address, 0 through <c>VramWordRange.MaximumWord</c>.</summary>
    public int CharacterWord
    {
        get;
        init => field = VramWordRange.Require(value);
    } = VramWordRange.Require(CharacterWord);
}
