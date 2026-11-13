namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Initial actor placement, animation selection, the processing it adds and collision radii.</summary>
internal readonly record struct TorizoInitializationDefinition(
    Position16<RoomPixels> Position, TorizoBodySegment Animation, SuperMetroid.Core.Enemies.EnemyProcessing Processing, Extent16<RoomPixels> Radius);
