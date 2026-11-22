namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>What the owner of a <see cref="SporeSpawnCue"/> does on each of its cases (#2553).</summary>
/// <param name="ShowDeathShade">Reacts to a <see cref="SporeSpawnCue.ShowDeathShade"/>.</param>
/// <param name="QueueSound">Reacts to a <see cref="SporeSpawnCue.QueueSound"/>.</param>
/// <param name="SetMovement">Reacts to a <see cref="SporeSpawnCue.SetMovement"/>.</param>
/// <param name="SetSway">Reacts to a <see cref="SporeSpawnCue.SetSway"/>.</param>
/// <param name="SetSporeGeneration">Reacts to a <see cref="SporeSpawnCue.SetSporeGeneration"/>.</param>
/// <param name="Signal">Reacts to a <see cref="SporeSpawnCue.Signal"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record SporeSpawnCueReactions<TContext>(Action<SporeSpawnCue.ShowDeathShade, TContext> ShowDeathShade, Action<SporeSpawnCue.QueueSound, TContext> QueueSound, Action<SporeSpawnCue.SetMovement, TContext> SetMovement, Action<SporeSpawnCue.SetSway, TContext> SetSway, Action<SporeSpawnCue.SetSporeGeneration, TContext> SetSporeGeneration, Action<SporeSpawnCue.Signal, TContext> Signal);
