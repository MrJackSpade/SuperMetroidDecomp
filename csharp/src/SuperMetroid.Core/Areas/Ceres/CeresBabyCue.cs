namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One cue of the Ceres Baby's animations: what it does, with the values it acts on.</summary>
internal abstract record CeresBabyCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private CeresBabyCue()
    {
    }

    /// <summary>What the function for this case makes of the cue, whichever case it is.</summary>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="reactsToMotion">Makes a <see cref="ReactsToMotion"/> into the result.</param>
    /// <param name="updateColors">Makes a <see cref="UpdateColors"/> into the result.</param>
    /// <param name="cries">Makes a <see cref="Cries"/> into the result.</param>
    internal abstract TResult Match<TResult>(Func<ReactsToMotion, TResult> reactsToMotion, Func<UpdateColors, TResult> updateColors, Func<Cries, TResult> cries);

    /// <summary>
    /// The baby reacts to its motion (#2167): while its vertical velocity is nonzero its owner starts its expressive loop.
    /// </summary>
    internal sealed record ReactsToMotion : CeresBabyCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<ReactsToMotion, TResult> reactsToMotion, Func<UpdateColors, TResult> updateColors, Func<Cries, TResult> cries) => Ensure.NotNull(reactsToMotion)(this);
    }

    /// <summary>Shows one of the Baby's shades.</summary>
    /// <param name="Shade">The shade an <see cref="UpdateColors"/> cue shows.</param>
    internal sealed record UpdateColors(CeresBabyShade Shade) : CeresBabyCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<ReactsToMotion, TResult> reactsToMotion, Func<UpdateColors, TResult> updateColors, Func<Cries, TResult> cries) => Ensure.NotNull(updateColors)(this);
    }

    /// <summary>
    /// The baby cries (#2167); while it is still, one time in two by the game's current random number, it instead rests
    /// silently, its owner starting its resting loop.
    /// </summary>
    internal sealed record Cries : CeresBabyCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<ReactsToMotion, TResult> reactsToMotion, Func<UpdateColors, TResult> updateColors, Func<Cries, TResult> cries) => Ensure.NotNull(cries)(this);
    }
}
