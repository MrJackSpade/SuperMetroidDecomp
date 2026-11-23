namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>What each case of a <see cref="WorkRobotCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the cue is made into.</typeparam>
/// <param name="Step">Makes a <see cref="WorkRobotCue.Step"/> into the result.</param>
/// <param name="TryLaser">Makes a <see cref="WorkRobotCue.TryLaser"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="WorkRobotCue.Signal"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record WorkRobotCueCases<TContext, TResult>(Func<WorkRobotCue.Step, TContext, TResult> Step, Func<WorkRobotCue.TryLaser, TContext, TResult> TryLaser, Func<WorkRobotCue.Signal, TContext, TResult> Signal);
