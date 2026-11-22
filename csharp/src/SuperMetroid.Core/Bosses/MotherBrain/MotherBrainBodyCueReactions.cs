namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What the owner of a <see cref="MotherBrainBodyCue"/> does on each of its cases (#2553).</summary>
/// <param name="SetPose">Reacts to a <see cref="MotherBrainBodyCue.SetPose"/>.</param>
/// <param name="Move">Reacts to a <see cref="MotherBrainBodyCue.Move"/>.</param>
/// <param name="StepAndMove">Reacts to a <see cref="MotherBrainBodyCue.StepAndMove"/>.</param>
/// <param name="SpawnHandDust">Reacts to a <see cref="MotherBrainBodyCue.SpawnHandDust"/>.</param>
/// <param name="Signal">Reacts to a <see cref="MotherBrainBodyCue.Signal"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record MotherBrainBodyCueReactions<TContext>(Action<MotherBrainBodyCue.SetPose, TContext> SetPose, Action<MotherBrainBodyCue.Move, TContext> Move, Action<MotherBrainBodyCue.StepAndMove, TContext> StepAndMove, Action<MotherBrainBodyCue.SpawnHandDust, TContext> SpawnHandDust, Action<MotherBrainBodyCue.Signal, TContext> Signal);
