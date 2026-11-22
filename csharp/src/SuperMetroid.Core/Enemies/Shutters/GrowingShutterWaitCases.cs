namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>What each case of a <see cref="GrowingShutterWait"/> is made into (#2553).</summary>
/// <param name="Timer">Makes a <see cref="GrowingShutterWait.Timer"/> into the result.</param>
/// <param name="SamusNear">Makes a <see cref="GrowingShutterWait.SamusNear"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
/// <typeparam name="TResult">What the wait is made into.</typeparam>
internal sealed record GrowingShutterWaitCases<TContext, TResult>(
    Func<GrowingShutterWait.Timer, TContext, TResult> Timer,
    Func<GrowingShutterWait.SamusNear, TContext, TResult> SamusNear);
