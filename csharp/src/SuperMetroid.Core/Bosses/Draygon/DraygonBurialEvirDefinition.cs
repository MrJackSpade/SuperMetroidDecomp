using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>One of Draygon's burial Evirs: where it spawns, which way it faces and how it moves toward him.</summary>
/// <param name="SubspeedMagnitude">The per-axis subpixel speeds: unsigned magnitudes of less than one pixel per update.</param>
/// <param name="Initial">The spawn position.</param>
/// <param name="Angle">The angle that selects each axis's movement sign.</param>
/// <param name="Kind">The sprite it is drawn as: facing right on the left of the circle, facing left on the right.</param>
internal readonly record struct DraygonBurialEvirDefinition(
    FixedVector<RoomPixels> SubspeedMagnitude,
    Position16<RoomPixels> Initial,
    TrigonometryAngle Angle,
    RoomSpriteObjectKind Kind);
