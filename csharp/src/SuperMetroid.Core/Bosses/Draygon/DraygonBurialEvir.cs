using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>One of the Evirs Draygon's death spawns to bury him, with the definition-table entry that moves it.</summary>
/// <param name="Sprite">The sprite object the Evir was spawned into.</param>
/// <param name="Entry">Its burial definition-table entry, from zero.</param>
internal sealed record DraygonBurialEvir(RoomSpriteObject Sprite, int Entry);
