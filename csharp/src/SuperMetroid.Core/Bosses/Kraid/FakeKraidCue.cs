namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One cue of Fake Kraid's animations: what it does, with the values it acts on.</summary>
internal abstract record FakeKraidCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private FakeKraidCue()
    {
    }

    /// <summary>What the function for this case makes of the cue, whichever case it is.</summary>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="spit">Makes a <see cref="Spit"/> into the result.</param>
    /// <param name="chooseAction">Makes a <see cref="ChooseAction"/> into the result.</param>
    /// <param name="move">Makes a <see cref="Move"/> into the result.</param>
    /// <param name="playCrySound">Makes a <see cref="PlayCrySound"/> into the result.</param>
    internal abstract TResult Match<TResult>(Func<Spit, TResult> spit, Func<ChooseAction, TResult> chooseAction, Func<Move, TResult> move, Func<PlayCrySound, TResult> playCrySound);

    /// <summary>Spits a pair of projectiles the cue's way (#2163).</summary>
    /// <param name="Direction">The way the spit flies.</param>
    internal sealed record Spit(HorizontalDirection Direction) : FakeKraidCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<Spit, TResult> spit, Func<ChooseAction, TResult> chooseAction, Func<Move, TResult> move, Func<PlayCrySound, TResult> playCrySound) => Ensure.NotNull(spit)(this);
    }

    /// <summary>Continues at the step or spit animation his wait between spits and his facing select.</summary>
    internal sealed record ChooseAction : FakeKraidCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<Spit, TResult> spit, Func<ChooseAction, TResult> chooseAction, Func<Move, TResult> move, Func<PlayCrySound, TResult> playCrySound) => Ensure.NotNull(chooseAction)(this);
    }

    /// <summary>Walks one step (<see cref="FakeKraidProjectileDefinitions.StepPixels"/>) unless the walk's last step turns him round, then faces Samus.</summary>
    internal sealed record Move : FakeKraidCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<Spit, TResult> spit, Func<ChooseAction, TResult> chooseAction, Func<Move, TResult> move, Func<PlayCrySound, TResult> playCrySound) => Ensure.NotNull(move)(this);
    }

    /// <summary>Records the cry sound while Fake Kraid is on screen.</summary>
    internal sealed record PlayCrySound : FakeKraidCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<Spit, TResult> spit, Func<ChooseAction, TResult> chooseAction, Func<Move, TResult> move, Func<PlayCrySound, TResult> playCrySound) => Ensure.NotNull(playCrySound)(this);
    }
}
