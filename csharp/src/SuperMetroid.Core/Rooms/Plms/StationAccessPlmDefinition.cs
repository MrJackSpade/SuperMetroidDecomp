namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One station-access BTS paired with its PLM header and initial instruction list.</summary>
internal readonly record struct StationAccessPlmDefinition(
    ushort RetractedDrawPointer,
    ushort ExtendedDrawPointer)
{
    /// <summary>Selects the retracted or extended bank-$84 draw operand for this station side.</summary>
    internal ushort DrawPointer(bool extended) =>
        extended ? ExtendedDrawPointer : RetractedDrawPointer;
}
