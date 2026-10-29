using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The room's part of the scene an accepted frame shows: the room's blending, and the scrolling sky's layer-2 map and
/// vertical scroll. A change the main loop makes shows one frame later.
/// </summary>
/// <param name="FxType">The room effect whose blending the frame shows.</param>
/// <param name="LayerBlendConfiguration">The room's layer-blending configuration.</param>
/// <param name="ScrollingSky">The scrolling sky's layer-2 map and vertical scroll, when the room has a sky.</param>
/// <param name="Room">The loaded room whose X-ray blending the frame shows, or null before a room exists.</param>
/// <param name="BossId">Area boss identifier read by the X-ray blending branch, 0 through 65535; only compared for equality.</param>
public readonly record struct GameplayLatchedScene(
    RoomFxType FxType,
    LayerBlendingConfiguration LayerBlendConfiguration,
    LatchedScrollingSky? ScrollingSky,
    RoomHeaderDefinition? Room,
    int BossId);
