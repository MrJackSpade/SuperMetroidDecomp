namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One cue of the Ceres Baby's animations: what it does, with the values it acts on.</summary>
internal abstract record CeresBabyCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private CeresBabyCue()
    {
    }

    /// <summary>What <paramref name="cases"/> makes of the cue, through the case it is.</summary>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="cases">What each case of the cue is made into.</param>
    internal abstract TResult Match<TResult>(CeresBabyCueCases<TResult> cases);

    /// <summary>
    /// The baby reacts to its motion (#2167): while its vertical velocity is nonzero its owner starts its expressive loop.
    /// </summary>
    internal sealed record ReactsToMotion : CeresBabyCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(CeresBabyCueCases<TResult> cases) => Ensure.NotNull(cases).ReactsToMotion(this);
    }

    /// <summary>Shows one of the Baby's shades.</summary>
    /// <param name="Shade">The shade an <see cref="UpdateColors"/> cue shows.</param>
    internal sealed record UpdateColors(CeresBabyShade Shade) : CeresBabyCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(CeresBabyCueCases<TResult> cases) => Ensure.NotNull(cases).UpdateColors(this);
    }

    /// <summary>
    /// The baby cries (#2167); while it is still, one time in two by the game's current random number, it instead rests
    /// silently, its owner starting its resting loop.
    /// </summary>
    internal sealed record Cries : CeresBabyCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(CeresBabyCueCases<TResult> cases) => Ensure.NotNull(cases).Cries(this);
    }
}

/// <summary>What each case of a <see cref="CeresBabyCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the cue is made into.</typeparam>
/// <param name="ReactsToMotion">Makes a <see cref="CeresBabyCue.ReactsToMotion"/> into the result.</param>
/// <param name="UpdateColors">Makes a <see cref="CeresBabyCue.UpdateColors"/> into the result.</param>
/// <param name="Cries">Makes a <see cref="CeresBabyCue.Cries"/> into the result.</param>
internal sealed record CeresBabyCueCases<TResult>(Func<CeresBabyCue.ReactsToMotion, TResult> ReactsToMotion, Func<CeresBabyCue.UpdateColors, TResult> UpdateColors, Func<CeresBabyCue.Cries, TResult> Cries);
