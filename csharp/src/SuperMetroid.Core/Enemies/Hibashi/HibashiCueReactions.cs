namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>What the owner of a <see cref="HibashiCue"/> does on each of its cases (#2553).</summary>
/// <param name="EruptionStep">Reacts to a <see cref="HibashiCue.EruptionStep"/>.</param>
/// <param name="Signal">Reacts to a <see cref="HibashiCue.Signal"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record HibashiCueReactions<TContext>(Action<HibashiCue.EruptionStep, TContext> EruptionStep, Action<HibashiCue.Signal, TContext> Signal);
