using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>One of Draygon's burial Evirs: where it spawns, the heading it drifts back along and which way it faces.</summary>
/// <param name="Start">
/// Where it spawns, measured from the room's top-left corner; an Evir left of that corner starts off the room's left edge.
/// </param>
/// <param name="Heading">
/// Its heading from the arc's centre, counted up from the right: it drifts back along it toward the centre.
/// </param>
/// <param name="Kind">The sprite it is drawn as: facing right on the left of the arc, facing left on the right.</param>
internal readonly record struct DraygonBurialEvirDefinition(
    Offset16<RoomPixels> Start,
    TrigonometryAngle Heading,
    RoomEffectActorKind Kind);
