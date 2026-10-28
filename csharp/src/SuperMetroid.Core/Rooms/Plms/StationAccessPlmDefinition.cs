namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One station side's access arm: the draws of its retracted and extended poses.</summary>
/// <param name="Retracted">The retracted arm.</param>
/// <param name="Extended">The extended arm.</param>
internal readonly record struct StationAccessPlmDefinition(
    StationDraw Retracted,
    StationDraw Extended);
