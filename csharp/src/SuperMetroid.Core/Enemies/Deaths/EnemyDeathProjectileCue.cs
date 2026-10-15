using SuperMetroid.Core.Rooms.SpriteObjects;

namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>One cue of an enemy's death explosion and drop programs, with the operands its native command carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="SpriteObject">The sprite object the cue spawns.</param>
internal readonly record struct EnemyDeathProjectileCue(EnemyDeathProjectileCueKind Kind, RoomSpriteObjectKind SpriteObject = default);
