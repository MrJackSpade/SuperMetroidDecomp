namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Facing-specific animation lists installed while a Yard is airborne.</summary>
internal readonly record struct YardAirborneInstructionDefinition(
    YardProgram VisibleProgram,
    YardProgram HidingProgram);
