namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One cue of Fake Kraid's animations: what it does, with the values it acts on.</summary>
internal abstract record FakeKraidCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private FakeKraidCue()
    {
    }

    /// <summary>What <paramref name="cases"/> makes of the cue, through the case it is.</summary>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="cases">What each case of the cue is made into.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract TResult Match<TContext, TResult>(FakeKraidCueCases<TContext, TResult> cases, TContext context);

    /// <summary>Spits a pair of projectiles the cue's way (#2163).</summary>
    /// <param name="Direction">The way the spit flies.</param>
    internal sealed record Spit(HorizontalDirection Direction) : FakeKraidCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(FakeKraidCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Spit(this, context);
    }

    /// <summary>Continues at the step or spit animation his wait between spits and his facing select.</summary>
    internal sealed record ChooseAction : FakeKraidCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(FakeKraidCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).ChooseAction(this, context);
    }

    /// <summary>Walks one step (<see cref="FakeKraidProjectileDefinitions.StepPixels"/>) unless the walk's last step turns him round, then faces Samus.</summary>
    internal sealed record Move : FakeKraidCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(FakeKraidCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Move(this, context);
    }

    /// <summary>Queues Fake Kraid's cry while he is on screen.</summary>
    internal sealed record PlayCrySound : FakeKraidCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(FakeKraidCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).PlayCrySound(this, context);
    }
}
