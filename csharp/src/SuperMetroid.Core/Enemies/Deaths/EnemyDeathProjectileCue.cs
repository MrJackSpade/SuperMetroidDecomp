using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>One cue of an enemy's death explosion and drop animations: what it does, with the values it acts on.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="SpriteObject">The sprite object the cue spawns.</param>
internal readonly record struct EnemyDeathProjectileCue(EnemyDeathProjectileCueKind Kind, RoomSpriteObjectKind SpriteObject = default);
