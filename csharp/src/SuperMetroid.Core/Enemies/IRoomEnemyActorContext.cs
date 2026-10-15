using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The actors and input enemy initialization and AI read: room terrain, Samus, the held controller
/// mask and the camera. Terrain and Samus are absent only in fixtures without them; an AI routine that
/// needs one fails where it reads it.
/// </summary>
public interface IRoomEnemyActorContext
{
    /// <summary>Room terrain and collision data, if the room has been built.</summary>
    RoomLevelData? Level { get; }

    /// <summary>Samus, if present.</summary>
    SamusState? Samus { get; }

    /// <summary>The raw held SNES controller mask.</summary>
    SnesButton ControllerInput { get; }

    /// <summary>The camera's room position.</summary>
    Position16<RoomPixels> Camera { get; }
}
