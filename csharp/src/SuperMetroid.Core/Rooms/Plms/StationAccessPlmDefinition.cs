namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One station side's access arm: the draws of its retracted and extended poses.</summary>
/// <param name="Retracted">The retracted arm.</param>
/// <param name="Extended">The extended arm.</param>
internal readonly record struct StationAccessPlmDefinition(
    StationDraw Retracted,
    StationDraw Extended)
{
    /// <summary>Selects the retracted or extended arm draw for this station side.</summary>
    internal PlmDraw Draw(bool extended) => PlmDraw.Of(extended ? Extended : Retracted);
}
