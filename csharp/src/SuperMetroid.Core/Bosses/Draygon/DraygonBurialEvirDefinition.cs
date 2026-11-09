namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>One compiled Draygon death/burial Evir position and movement record.</summary>
/// <param name="SubspeedMagnitude">The per-axis subpixel speeds: unsigned magnitudes with zero whole words.</param>
/// <param name="Initial">The spawn position.</param>
/// <param name="Angle">The angle that selects each axis's movement sign.</param>
internal readonly record struct DraygonBurialEvirDefinition(
    FixedVector<RoomPixels> SubspeedMagnitude,
    Position16<RoomPixels> Initial,
    TrigonometryAngle Angle);
