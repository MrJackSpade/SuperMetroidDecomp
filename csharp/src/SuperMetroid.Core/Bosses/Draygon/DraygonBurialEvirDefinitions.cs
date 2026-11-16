using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The six Evirs that close in on Draygon as he is buried: each starts on an arc above him and drifts back toward its
/// centre, under one pixel per update on each axis, every one of them downward.
/// </summary>
internal static class DraygonBurialEvirDefinitions
{
    /// <summary>
    /// The burial Evirs in the order Draygon's death spawns them: three on the right of the arc, drawn as the intro Evir
    /// facing left and drifting left, then three on its left facing right and drifting right, a sixteenth of a turn apart
    /// from the lowest on the right to the lowest on the left. The two leftmost start off the room's left edge.
    /// </summary>
    internal static IReadOnlyList<DraygonBurialEvirDefinition> InSpawnOrder { get; } =
    [
        new(new(678, 229), new(24), RoomEffectActorKind.DraygonIntroEvir),
        new(new(538, 89), new(40), RoomEffectActorKind.DraygonIntroEvir),
        new(new(355, 13), new(56), RoomEffectActorKind.DraygonIntroEvir),
        new(new(156, 13), new(72), RoomEffectActorKind.DraygonDeathEvirFacingRight),
        new(new(-27, 89), new(88), RoomEffectActorKind.DraygonDeathEvirFacingRight),
        new(new(-167, 229), new(104), RoomEffectActorKind.DraygonDeathEvirFacingRight),
    ];
}
