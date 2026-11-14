using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>One of the Evirs Draygon's death spawns to bury him, with the definition that places and moves it.</summary>
/// <param name="Sprite">The sprite object the Evir was spawned into.</param>
/// <param name="Definition">Where it spawned, how it faces and how it moves.</param>
internal sealed record DraygonBurialEvir(RoomSpriteObject Sprite, DraygonBurialEvirDefinition Definition);
