namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Named station/side dispatch for BTS $47..$4C, matching $94:91C7..91D2.
/// Each case selects its bank-$84 header/list and retracted/extended draw operands.
/// Energy and missile stations share access artwork on each side. Other BTS
/// values, including the save-floor trigger, are outside this extending domain.
/// </summary>
internal static class StationAccessPlmDefinitions
{
    /// <summary><c>$84:B6D7/$AD86</c>, map-station access from its right side.</summary>
    public static StationAccessPlmDefinition MapRight =>
        new(0x9f49, 0x9f55);

    /// <summary><c>$84:B6DB/$ADA4</c>, map-station access from its left side.</summary>
    public static StationAccessPlmDefinition MapLeft =>
        new(0x9f5b, 0x9f67);

    /// <summary><c>$84:B6E3/$ADF1</c>, energy-station access from its right side.</summary>
    public static StationAccessPlmDefinition EnergyRight =>
        new(0x9fb5, 0x9fbb);

    /// <summary><c>$84:B6E7/$AE13</c>, energy-station access from its left side.</summary>
    public static StationAccessPlmDefinition EnergyLeft =>
        new(0x9fc1, 0x9fc7);

    /// <summary><c>$84:B6EF/$AE7B</c>, missile-station access from its right side.</summary>
    public static StationAccessPlmDefinition MissileRight =>
        new(0x9fb5, 0x9fbb);

    /// <summary><c>$84:B6F3/$AE9D</c>, missile-station access from its left side.</summary>
    public static StationAccessPlmDefinition MissileLeft =>
        new(0x9fc1, 0x9fc7);

    /// <summary>Resolves the access actor selected by one station-special-block BTS.</summary>
    public static StationAccessPlmDefinition Resolve(StationAccessBehavior behavior) => behavior switch
    {
        StationAccessBehavior.MapRight => MapRight,
        StationAccessBehavior.MapLeft => MapLeft,
        StationAccessBehavior.EnergyRight => EnergyRight,
        StationAccessBehavior.EnergyLeft => EnergyLeft,
        StationAccessBehavior.MissileRight => MissileRight,
        StationAccessBehavior.MissileLeft => MissileLeft,
        _ => throw new InvalidDataException($"Station access has invalid BTS ${(byte)behavior:X2}.")
    };
}

/// <summary>One station-access BTS paired with its PLM header and initial instruction list.</summary>
/// <param name="RetractedDrawPointer">Native draw-list operand used while the access block is retracted.</param>
/// <param name="ExtendedDrawPointer">Native draw-list operand used after the access block extends.</param>
internal readonly record struct StationAccessPlmDefinition(
    ushort RetractedDrawPointer,
    ushort ExtendedDrawPointer)
{
    /// <summary>Selects the draw-list operand for the access block's current animation phase.</summary>
    /// <param name="extended"><see langword="true"/> selects the extended phase; otherwise selects the retracted phase.</param>
    internal ushort DrawPointer(bool extended) =>
        extended ? ExtendedDrawPointer : RetractedDrawPointer;
}
