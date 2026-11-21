namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Which way Draygon faces and the straight direction his eye looks toward (#2557), which choose the eye's look.</summary>
/// <param name="Facing">The way he faces.</param>
/// <param name="Toward">The straight direction Samus lies nearest from the eye's mouth.</param>
internal readonly record struct DraygonEyeLook(HorizontalDirection Facing, CompassDirection Toward);
