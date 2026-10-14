namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Initial actor placement, program selection, property mask and collision radii.</summary>
internal readonly record struct TorizoInitializationDefinition(
    Position16<RoomPixels> Position, TorizoBodyProgram Program, ushort PropertyMask, Extent16<RoomPixels> Radius);
