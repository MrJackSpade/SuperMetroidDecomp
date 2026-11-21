namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// What starts a moving shutter (#2560): a closed choice of a timer with its wait, Samus coming near with the distance,
/// starting at once, and a shot's reaction once or every time. Each carries only what it reads.
/// </summary>
/// <param name="Mode">The kind of trigger.</param>
public abstract record ShutterTrigger(ShutterTriggerMode Mode)
{
    /// <summary>The shutter starts at once.</summary>
    public static ShutterTrigger Immediate { get; } = new Plain(ShutterTriggerMode.Immediate);

    /// <summary>A shot starts the shutter once.</summary>
    public static ShutterTrigger OneShotReaction { get; } = new Plain(ShutterTriggerMode.OneShotReaction);

    /// <summary>A shot starts the shutter every time.</summary>
    public static ShutterTrigger RepeatableReaction { get; } = new Plain(ShutterTriggerMode.RepeatableReaction);

    /// <summary>What <paramref name="cases"/> makes of this trigger, through the case it is (#2553).</summary>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <typeparam name="TResult">What the trigger is made into.</typeparam>
    /// <param name="cases">What each case is made into.</param>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract TResult Match<TContext, TResult>(ShutterTriggerCases<TContext, TResult> cases, TContext context);

    /// <summary>A timer starts the shutter.</summary>
    /// <param name="Wait">The updates it waits.</param>
    public sealed record Timer(int Wait) : ShutterTrigger(ShutterTriggerMode.Timer)
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(ShutterTriggerCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Timer(this, context);
    }

    /// <summary>Samus coming near starts the shutter.</summary>
    /// <param name="Distance">The horizontal distance in pixels Samus must come within.</param>
    public sealed record HorizontalProximity(int Distance) : ShutterTrigger(ShutterTriggerMode.HorizontalProximity)
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(ShutterTriggerCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).HorizontalProximity(this, context);
    }

    /// <summary>A trigger that reads no wait or distance.</summary>
    /// <param name="Mode">The kind of trigger.</param>
    private sealed record Plain(ShutterTriggerMode Mode) : ShutterTrigger(Mode)
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(ShutterTriggerCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Plain(this, context);
    }
}

/// <summary>What each case of a <see cref="ShutterTrigger"/> is made into (#2553).</summary>
/// <param name="Timer">Makes a <see cref="ShutterTrigger.Timer"/> into the result.</param>
/// <param name="HorizontalProximity">Makes a <see cref="ShutterTrigger.HorizontalProximity"/> into the result.</param>
/// <param name="Plain">Makes a trigger that reads no wait or distance (starting at once, or a shot's reaction) into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
/// <typeparam name="TResult">What the trigger is made into.</typeparam>
internal sealed record ShutterTriggerCases<TContext, TResult>(
    Func<ShutterTrigger.Timer, TContext, TResult> Timer,
    Func<ShutterTrigger.HorizontalProximity, TContext, TResult> HorizontalProximity,
    Func<ShutterTrigger, TContext, TResult> Plain);
