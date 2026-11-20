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
    internal abstract TResult Match<TResult>(FakeKraidCueCases<TResult> cases);

    /// <summary>Spits a pair of projectiles the cue's way (#2163).</summary>
    /// <param name="Direction">The way the spit flies.</param>
    internal sealed record Spit(HorizontalDirection Direction) : FakeKraidCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(FakeKraidCueCases<TResult> cases) => Ensure.NotNull(cases).Spit(this);
    }

    /// <summary>Continues at the step or spit animation his wait between spits and his facing select.</summary>
    internal sealed record ChooseAction : FakeKraidCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(FakeKraidCueCases<TResult> cases) => Ensure.NotNull(cases).ChooseAction(this);
    }

    /// <summary>Walks one step (<see cref="FakeKraidProjectileDefinitions.StepPixels"/>) unless the walk's last step turns him round, then faces Samus.</summary>
    internal sealed record Move : FakeKraidCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(FakeKraidCueCases<TResult> cases) => Ensure.NotNull(cases).Move(this);
    }

    /// <summary>Queues Fake Kraid's cry while he is on screen.</summary>
    internal sealed record PlayCrySound : FakeKraidCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(FakeKraidCueCases<TResult> cases) => Ensure.NotNull(cases).PlayCrySound(this);
    }
}

/// <summary>What each case of a <see cref="FakeKraidCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the cue is made into.</typeparam>
/// <param name="Spit">Makes a <see cref="FakeKraidCue.Spit"/> into the result.</param>
/// <param name="ChooseAction">Makes a <see cref="FakeKraidCue.ChooseAction"/> into the result.</param>
/// <param name="Move">Makes a <see cref="FakeKraidCue.Move"/> into the result.</param>
/// <param name="PlayCrySound">Makes a <see cref="FakeKraidCue.PlayCrySound"/> into the result.</param>
internal sealed record FakeKraidCueCases<TResult>(Func<FakeKraidCue.Spit, TResult> Spit, Func<FakeKraidCue.ChooseAction, TResult> ChooseAction, Func<FakeKraidCue.Move, TResult> Move, Func<FakeKraidCue.PlayCrySound, TResult> PlayCrySound);
