namespace SuperMetroid.Core.Game;

/// <summary>Facing-specific animation lists installed while a Yard is airborne.</summary>
internal readonly record struct YardAirborneInstructionDefinition(
    ushort VisibleInstructionList,
    ushort HidingInstructionList);
