namespace SuperMetroid.Core.Animation;

/// <summary>
/// The frame an animation shows next (#1600), or that it has ended. Every outcome but a pause is an <see cref="Unpaused"/>
/// one, all a play of an animation that never pauses comes to (#2553).
/// </summary>
/// <typeparam name="TSegment">The segment identity.</typeparam>
/// <typeparam name="TFrame">The frame identity.</typeparam>
internal abstract partial record AnimationShown<TSegment, TFrame> : IHidingAnimationShown<TSegment, TFrame>
    where TSegment : struct
    where TFrame : struct
{
    /// <summary>Allows only the outcomes below.</summary>
    private AnimationShown()
    {
    }

    /// <inheritdoc/>
    void IHidingAnimationShown<TSegment, TFrame>.React<TContext>(HidingAnimationShownReactions<TSegment, TFrame, TContext> reactions, TContext context) =>
        Ensure.NotNull(reactions).Shown(this, context);

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this outcome (#2553).</summary>
    /// <param name="reactions">What its owner does on each outcome.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(AnimationShownReactions<TSegment, TFrame, TContext> reactions, TContext context);

    /// <summary>What <paramref name="cases"/> make of this outcome (#2553).</summary>
    /// <typeparam name="TContext">What the case's answer reads besides the case.</typeparam>
    /// <typeparam name="TResult">What an outcome comes to.</typeparam>
    /// <param name="cases">What its owner makes of each outcome.</param>
    /// <param name="context">What the case's answer reads besides the case, handed to it so the cases are built once.</param>
    internal abstract TResult Match<TContext, TResult>(AnimationShownCases<TSegment, TFrame, TContext, TResult> cases, TContext context);

    /// <summary>An outcome that is no pause: a frame, a held frame, a rest or the end (#2553).</summary>
    internal abstract record Unpaused : AnimationShown<TSegment, TFrame>, IUnpausedHidingAnimationShown<TSegment, TFrame>
    {
        /// <summary>Allows only the outcomes below that are no pause.</summary>
        private protected Unpaused()
        {
        }

        /// <inheritdoc/>
        TResult IUnpausedHidingAnimationShown<TSegment, TFrame>.Match<TContext, TResult>(UnpausedHidingAnimationShownCases<TSegment, TFrame, TContext, TResult> cases, TContext context) =>
            Ensure.NotNull(cases).Shown(this, context);

        /// <summary>Carries out the reaction <paramref name="reactions"/> has to this outcome (#2553).</summary>
        /// <param name="reactions">What its owner does on each outcome.</param>
        /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
        /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
        internal abstract void React<TContext>(UnpausedAnimationShownReactions<TSegment, TFrame, TContext> reactions, TContext context);

        /// <summary>What <paramref name="cases"/> make of this outcome (#2553).</summary>
        /// <typeparam name="TContext">What the case's answer reads besides the case.</typeparam>
        /// <typeparam name="TResult">What an outcome comes to.</typeparam>
        /// <param name="cases">What its owner makes of each outcome.</param>
        /// <param name="context">What the case's answer reads besides the case, handed to it so the cases are built once.</param>
        internal abstract TResult Match<TContext, TResult>(UnpausedAnimationShownCases<TSegment, TFrame, TContext, TResult> cases, TContext context);
    }

    /// <summary>The animation shows <paramref name="Shown"/>, a frame of <paramref name="Segment"/>, for <paramref name="Duration"/> updates.</summary>
    /// <param name="Segment">The segment the frame belongs to.</param>
    /// <param name="Shown">The frame.</param>
    /// <param name="Duration">The updates it shows for.</param>
    internal sealed partial record Frame(TSegment Segment, TFrame Shown, int Duration) : Unpaused
    {
        /// <inheritdoc/>
        internal override void React<TContext>(UnpausedAnimationShownReactions<TSegment, TFrame, TContext> reactions, TContext context) => Ensure.NotNull(reactions).Frame(this, context);

        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(UnpausedAnimationShownCases<TSegment, TFrame, TContext, TResult> cases, TContext context) =>
            Ensure.NotNull(cases).Frame(this, context);

        /// <inheritdoc/>
        internal override void React<TContext>(AnimationShownReactions<TSegment, TFrame, TContext> reactions, TContext context) => Ensure.NotNull(reactions).Frame(this, context);

        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(AnimationShownCases<TSegment, TFrame, TContext, TResult> cases, TContext context) =>
            Ensure.NotNull(cases).Frame(this, context);
    }

    /// <summary>
    /// The animation shows <paramref name="Shown"/> and rests on it for good (#1607), until its owner plays another.
    /// </summary>
    /// <param name="Shown">The frame.</param>
    internal sealed partial record HeldFrame(TFrame Shown) : Unpaused
    {
        /// <inheritdoc/>
        internal override void React<TContext>(UnpausedAnimationShownReactions<TSegment, TFrame, TContext> reactions, TContext context) => Ensure.NotNull(reactions).HeldFrame(this, context);

        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(UnpausedAnimationShownCases<TSegment, TFrame, TContext, TResult> cases, TContext context) =>
            Ensure.NotNull(cases).HeldFrame(this, context);

        /// <inheritdoc/>
        internal override void React<TContext>(AnimationShownReactions<TSegment, TFrame, TContext> reactions, TContext context) => Ensure.NotNull(reactions).HeldFrame(this, context);

        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(AnimationShownCases<TSegment, TFrame, TContext, TResult> cases, TContext context) =>
            Ensure.NotNull(cases).HeldFrame(this, context);
    }

    /// <summary>The animation pauses for <paramref name="Duration"/> updates, showing no new frame.</summary>
    /// <param name="Duration">The updates the pause lasts.</param>
    internal sealed partial record Paused(int Duration) : AnimationShown<TSegment, TFrame>
    {
        /// <inheritdoc/>
        internal override void React<TContext>(AnimationShownReactions<TSegment, TFrame, TContext> reactions, TContext context) => Ensure.NotNull(reactions).Paused(this, context);

        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(AnimationShownCases<TSegment, TFrame, TContext, TResult> cases, TContext context) =>
            Ensure.NotNull(cases).Paused(this, context);
    }

    /// <summary>The animation rests on its last frame until its owner plays another.</summary>
    internal sealed partial record Held : Unpaused
    {
        /// <inheritdoc/>
        internal override void React<TContext>(UnpausedAnimationShownReactions<TSegment, TFrame, TContext> reactions, TContext context) => Ensure.NotNull(reactions).Held(this, context);

        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(UnpausedAnimationShownCases<TSegment, TFrame, TContext, TResult> cases, TContext context) =>
            Ensure.NotNull(cases).Held(this, context);

        /// <inheritdoc/>
        internal override void React<TContext>(AnimationShownReactions<TSegment, TFrame, TContext> reactions, TContext context) => Ensure.NotNull(reactions).Held(this, context);

        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(AnimationShownCases<TSegment, TFrame, TContext, TResult> cases, TContext context) =>
            Ensure.NotNull(cases).Held(this, context);

        /// <summary>The one held outcome.</summary>
        internal static Held Instance { get; } = new();
    }

    /// <summary>The animation has ended.</summary>
    internal sealed partial record Ended : Unpaused
    {
        /// <inheritdoc/>
        internal override void React<TContext>(UnpausedAnimationShownReactions<TSegment, TFrame, TContext> reactions, TContext context) => Ensure.NotNull(reactions).Ended(this, context);

        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(UnpausedAnimationShownCases<TSegment, TFrame, TContext, TResult> cases, TContext context) =>
            Ensure.NotNull(cases).Ended(this, context);

        /// <inheritdoc/>
        internal override void React<TContext>(AnimationShownReactions<TSegment, TFrame, TContext> reactions, TContext context) => Ensure.NotNull(reactions).Ended(this, context);

        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(AnimationShownCases<TSegment, TFrame, TContext, TResult> cases, TContext context) =>
            Ensure.NotNull(cases).Ended(this, context);

        /// <summary>The one ended outcome.</summary>
        internal static Ended Instance { get; } = new();
    }
}
