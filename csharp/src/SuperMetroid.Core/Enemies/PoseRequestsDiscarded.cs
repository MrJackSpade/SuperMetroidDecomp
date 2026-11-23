namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy grabbed or released Samus in the pass (#2571): the transitions the input phase chose for her old pose are discarded.</summary>
public sealed partial record PoseRequestsDiscarded : EnemyPassEvent;
