namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>What each case of a <see cref="ShutterTrigger"/> is made into (#2553).</summary>
/// <param name="Timer">Makes a <see cref="ShutterTrigger.Timer"/> into the result.</param>
/// <param name="HorizontalProximity">Makes a <see cref="ShutterTrigger.HorizontalProximity"/> into the result.</param>
/// <param name="Plain">Makes a trigger that reads no wait or distance (starting at once, or a shot's reaction) into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
/// <typeparam name="TResult">What the trigger is made into.</typeparam>
internal sealed partial record ShutterTriggerCases<TContext, TResult>(
    Func<ShutterTrigger.Timer, TContext, TResult> Timer,
    Func<ShutterTrigger.HorizontalProximity, TContext, TResult> HorizontalProximity,
    Func<ShutterTrigger, TContext, TResult> Plain);
