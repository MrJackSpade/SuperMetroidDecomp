namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>What the owner of a <see cref="YappingMawCue"/> does on each of its cases (#2553).</summary>
/// <param name="HoldSamus">Reacts to a <see cref="YappingMawCue.HoldSamus"/>.</param>
/// <param name="Signal">Reacts to a <see cref="YappingMawCue.Signal"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record YappingMawCueReactions<TContext>(Action<YappingMawCue.HoldSamus, TContext> HoldSamus, Action<YappingMawCue.Signal, TContext> Signal);
