namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Selects the cartridge path that owns initial BG1 tilemap publication for a room load.
/// </summary>
internal enum RoomViewportLoadMode
{
    /// <summary>
    /// Load stations, startup, and explicit debug loads call <c>$80:A176</c> to draw all
    /// seventeen visible block columns immediately.
    /// </summary>
    DisplayInitialViewport,

    /// <summary>
    /// State-$0B door loads retain the source-room VRAM ring and replace it one boundary
    /// row or column at a time through <c>$80:AD30-$80:AFC0</c>.
    /// </summary>
    StreamThroughDoor,
}

/// <summary>
/// Source-room PPU scroll words retained until directional door setup derives its offsets.
/// </summary>
/// <param name="Bg1Horizontal">Displayed BG1 horizontal register value before door setup.</param>
/// <param name="Bg1Vertical">Displayed BG1 vertical register value before door setup.</param>
internal readonly record struct DoorOpeningPpuScroll(
    ushort Bg1Horizontal,
    ushort Bg1Vertical);
