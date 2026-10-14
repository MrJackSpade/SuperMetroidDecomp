namespace SuperMetroid.Core.Game;

/// <summary>Initial actor placement, program selection, property mask and collision radii.</summary>
public readonly record struct TorizoInitializationDefinition(
    Position16<RoomPixels> Position, ushort Instruction, ushort PropertyMask, Extent16<RoomPixels> Radius);
