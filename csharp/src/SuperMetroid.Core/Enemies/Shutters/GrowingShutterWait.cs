namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>What a growing shutter waits for before it grows (#2560): a timer with its updates, or Samus coming near.</summary>
public abstract record GrowingShutterWait
{
    /// <summary>What <paramref name="cases"/> makes of this wait, through the case it is (#2553).</summary>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <typeparam name="TResult">What the wait is made into.</typeparam>
    /// <param name="cases">What each case is made into.</param>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract TResult Match<TContext, TResult>(GrowingShutterWaitCases<TContext, TResult> cases, TContext context);

    /// <summary>The shutter grows once a timer runs out.</summary>
    /// <param name="Updates">The updates it waits.</param>
    public sealed record Timer(int Updates) : GrowingShutterWait
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(GrowingShutterWaitCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Timer(this, context);
    }

    /// <summary>The shutter grows once Samus comes near.</summary>
    /// <param name="Distance">The horizontal distance in pixels Samus must come within.</param>
    public sealed record SamusNear(int Distance) : GrowingShutterWait
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(GrowingShutterWaitCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).SamusNear(this, context);
    }
}
