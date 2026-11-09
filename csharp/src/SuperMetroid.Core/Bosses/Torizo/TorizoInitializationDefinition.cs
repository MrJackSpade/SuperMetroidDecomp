namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Initial actor placement, program selection, the processing it adds and collision radii.</summary>
internal readonly record struct TorizoInitializationDefinition(
    Position16<RoomPixels> Position, TorizoBodyProgram Program, SuperMetroid.Core.Enemies.EnemyProcessing Processing, Extent16<RoomPixels> Radius);
