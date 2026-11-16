using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The actors and input a room's enemy initialization reads: room terrain, Samus, the held controller mask and the
/// camera.
/// </summary>
public interface IRoomEnemyActorContext
{
    /// <summary>Room terrain and collision data.</summary>
    RoomLevelData Level { get; }

    /// <summary>Samus.</summary>
    SamusState Samus { get; }

    /// <summary>The gameplay actions the player holds this update.</summary>
    GameplayAction ControllerInput { get; }

    /// <summary>The camera's room position.</summary>
    Position16<RoomPixels> Camera { get; }
}
