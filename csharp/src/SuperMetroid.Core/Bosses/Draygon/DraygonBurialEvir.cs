using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>One of the Evirs Draygon's death spawns to bury him, with the definition that places and moves it.</summary>
/// <param name="Sprite">The effect actor the Evir was spawned into.</param>
/// <param name="Definition">Where it spawned, how it faces and how it moves.</param>
internal sealed record DraygonBurialEvir(RoomEffectActor Sprite, DraygonBurialEvirDefinition Definition);
