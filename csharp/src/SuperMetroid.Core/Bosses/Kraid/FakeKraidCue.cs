namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One cue of Fake Kraid's animations: what it does, with the values it acts on.</summary>
internal abstract partial record FakeKraidCue
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
    internal sealed partial record Spit(HorizontalDirection Direction) : FakeKraidCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(FakeKraidCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Spit(this, context);
    }

    /// <summary>Continues at the step or spit animation his wait between spits and his facing select.</summary>
    internal sealed partial record ChooseAction : FakeKraidCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(FakeKraidCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).ChooseAction(this, context);
    }

    /// <summary>Walks one step (<see cref="FakeKraidProjectileDefinitions.StepPixels"/>) unless the walk's last step turns him round, then faces Samus.</summary>
    internal sealed partial record Move : FakeKraidCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(FakeKraidCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Move(this, context);
    }

    /// <summary>Queues Fake Kraid's cry while he is on screen.</summary>
    internal sealed partial record PlayCrySound : FakeKraidCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(FakeKraidCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).PlayCrySound(this, context);
    }
}

/// <summary>What each case of a <see cref="FakeKraidCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the cue is made into.</typeparam>
/// <param name="Spit">Makes a <see cref="FakeKraidCue.Spit"/> into the result.</param>
/// <param name="ChooseAction">Makes a <see cref="FakeKraidCue.ChooseAction"/> into the result.</param>
/// <param name="Move">Makes a <see cref="FakeKraidCue.Move"/> into the result.</param>
/// <param name="PlayCrySound">Makes a <see cref="FakeKraidCue.PlayCrySound"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record FakeKraidCueCases<TContext, TResult>(Func<FakeKraidCue.Spit, TContext, TResult> Spit, Func<FakeKraidCue.ChooseAction, TContext, TResult> ChooseAction, Func<FakeKraidCue.Move, TContext, TResult> Move, Func<FakeKraidCue.PlayCrySound, TContext, TResult> PlayCrySound);
