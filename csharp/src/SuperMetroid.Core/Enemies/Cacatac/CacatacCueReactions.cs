namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>What the owner of a <see cref="CacatacCue"/> does on each of its cases (#2553).</summary>
/// <param name="SpawnSpike">Reacts to a <see cref="CacatacCue.SpawnSpike"/>.</param>
/// <param name="Signal">Reacts to a <see cref="CacatacCue.Signal"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record CacatacCueReactions<TContext>(Action<CacatacCue.SpawnSpike, TContext> SpawnSpike, Action<CacatacCue.Signal, TContext> Signal);
