namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Where one of the Evirs of Draygon's death and burial starts and how it moves.</summary>
/// <param name="SubspeedMagnitude">The per-axis subpixel speeds: unsigned magnitudes of less than one pixel per update.</param>
/// <param name="Initial">The spawn position.</param>
/// <param name="Angle">The angle that selects each axis's movement sign.</param>
internal readonly record struct DraygonBurialEvirDefinition(
    FixedVector<RoomPixels> SubspeedMagnitude,
    Position16<RoomPixels> Initial,
    TrigonometryAngle Angle);
