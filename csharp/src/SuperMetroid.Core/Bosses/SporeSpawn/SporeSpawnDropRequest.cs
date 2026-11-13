namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Frame-local marker for one item-drop attempt from a destroyed spore or the boss's death cue.</summary>
/// <remarks>Contains no position or table selector: the publisher also invokes drop spawning immediately, using the stalk's drop table for a spore or the body's for one of the sixteen death drops.</remarks>
public readonly record struct SporeSpawnDropRequest();
