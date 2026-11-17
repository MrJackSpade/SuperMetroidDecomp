using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>The room's background shown in a band below the turning Ceres floor, at the bottom of the screen.</summary>
/// <param name="FirstLine">The first screen line of the band; the background's scroll is measured from the top of the screen, not from here.</param>
/// <param name="Scroll">
/// The background pixel the screen's top-left corner shows, as a signed position (#2308), as the ordinary view's background
/// scroll is (<see cref="GameplaySceneView.BackgroundScroll"/>): a scroll left of or above the background's origin is
/// negative, and the room's background plane, which does not repeat, shows nothing there.
/// </param>
/// <param name="Room">The room's background plane the band shows (#1346, #1403), drawn from its block pictures.</param>
public readonly record struct TurningRoomLowerBand(int FirstLine, Offset16<BackgroundPixels> Scroll, RoomBackgroundLayer Room);
