namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Frame-local marker for one item-drop attempt from a destroyed spore or the boss's death instruction.</summary>
/// <remarks>Contains no position or table selector: the publisher also invokes drop spawning immediately, using stalk header $DF7F for a spore or body header $DF3F for one of the sixteen death drops.</remarks>
public readonly record struct SporeSpawnDropRequest();
