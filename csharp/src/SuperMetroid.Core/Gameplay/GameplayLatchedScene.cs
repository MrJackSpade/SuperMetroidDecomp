using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The room's part of the scene an accepted frame shows: the room's blending, and the scrolling sky's vertical
/// scroll. A change an update makes shows one frame later.
/// </summary>
/// <param name="Blend">How the room effect's overlay blends with the room (#1545), or null when it describes none.</param>
/// <param name="ScrollingSky">The scrolling sky's vertical scroll, when the room has a sky.</param>
/// <param name="Room">The loaded room whose X-ray blending the frame shows.</param>
/// <param name="Boss">The loaded boss, which the X-ray blending reads, or null.</param>
public readonly partial record struct GameplayLatchedScene(
    RoomEffectBlend? Blend,
    LatchedScrollingSky? ScrollingSky,
    RoomDefinition Room,
    AreaBoss? Boss);
