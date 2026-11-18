using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The downward-gate trigger a shootable block's reaction selects (<see cref="BlockReactionDefinitions.DownwardGateTriggers"/>,
/// #2484): the trigger's colour, and the side of its gate it sits on. What each trigger draws and which shots set it off
/// are its definition's (<see cref="DownwardGateTriggerDefinitions"/>).
/// </summary>
/// <param name="Color">The trigger's colour.</param>
/// <param name="Side">The side of its gate the trigger sits on.</param>
public readonly record struct DownwardGateTrigger(DownwardGateTriggerColor Color, HorizontalDirection Side);
