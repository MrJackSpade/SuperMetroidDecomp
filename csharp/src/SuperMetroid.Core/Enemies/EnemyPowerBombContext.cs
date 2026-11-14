using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// One enemy the Power Bomb's blast reached in the power-bomb pass, with the update's room as the pass sees it: a
/// species whose reaction runs a step of its main behaviour reads the room's terrain and camera from here (#1807).
/// </summary>
/// <param name="Enemy">The enemy whose reaction runs.</param>
/// <param name="Samus">Samus, when the pass has one.</param>
/// <param name="Level">The room's terrain this update.</param>
/// <param name="Camera">The camera's position this update, as the power-bomb pass runs.</param>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal readonly record struct EnemyPowerBombContext(
    RoomEnemySlot Enemy,
    SamusState? Samus,
    RoomLevelData Level,
    Position16<RoomPixels> Camera);
