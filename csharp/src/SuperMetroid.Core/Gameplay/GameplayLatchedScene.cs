using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The room's part of the scene an accepted frame shows: the room's blending, and the scrolling sky's background map and
/// vertical scroll. A change the main loop makes shows one frame later.
/// </summary>
/// <param name="FxType">The room effect whose blending the frame shows.</param>
/// <param name="Blend">How the room effect's overlay blends with the room (#1545), or null when it describes none.</param>
/// <param name="ScrollingSky">The scrolling sky's background map and vertical scroll, when the room has a sky.</param>
/// <param name="Room">The loaded room whose X-ray blending the frame shows, or null before a room exists.</param>
/// <param name="Boss">The loaded boss, which the X-ray blending reads, or null.</param>
public readonly record struct GameplayLatchedScene(
    RoomFxType FxType,
    RoomEffectBlend? Blend,
    LatchedScrollingSky? ScrollingSky,
    RoomDefinition? Room,
    AreaBoss? Boss);
