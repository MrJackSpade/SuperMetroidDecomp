using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The room backgrounds one gameplay frame shows (#1346), resolved from level data with no stored picture grid: foreground from the room's
/// level plane, the one thing the background shows (#2544), the source room's screen during a door's opening scroll, and the text an
/// escape shows over either layer (#1555). The frame boundary resolves every picture as it latches the frame.
/// </summary>
/// <param name="Foreground">The room's level plane on the foreground.</param>
/// <param name="Background">
/// What the background shows: the room's background plane, the sky's rows, a boss's body, the room's library background image, the
/// X-ray's reveal, or nothing.
/// </param>
/// <param name="DoorSource">The source room's foreground screen during a door's opening scroll, or null.</param>
/// <param name="ForegroundText">The text an escape shows over the foreground (Ceres's warning), or null (#1555).</param>
/// <param name="BackgroundText">The text an escape shows over the background (Zebes's warning), or null (#1348, #1555).</param>
/// <param name="Floor">The turning floor the room shows, or null when it shows none (#1349, #1467, #1649).</param>
public sealed record RoomBackgrounds(RoomBackgroundLayer Foreground, ShownRoomBackground Background,
    RoomDoorBackground? DoorSource = null, RoomOverlayPictures? ForegroundText = null,
    RoomOverlayPictures? BackgroundText = null, AffineFloor? Floor = null);
