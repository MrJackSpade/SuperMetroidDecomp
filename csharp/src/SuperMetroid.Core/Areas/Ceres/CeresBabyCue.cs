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
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract TResult Match<TContext, TResult>(CeresBabyCueCases<TContext, TResult> cases, TContext context);

    /// <summary>
    /// The baby reacts to its motion (#2167): while its vertical velocity is nonzero its owner starts its expressive loop.
    /// </summary>
    internal sealed record ReactsToMotion : CeresBabyCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(CeresBabyCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).ReactsToMotion(this, context);
    }

    /// <summary>Shows one of the Baby's shades.</summary>
    /// <param name="Shade">The shade an <see cref="UpdateColors"/> cue shows.</param>
    internal sealed record UpdateColors(CeresBabyShade Shade) : CeresBabyCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(CeresBabyCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).UpdateColors(this, context);
    }

    /// <summary>
    /// The baby cries (#2167); while it is still, one time in two by the game's current random number, it instead rests
    /// silently, its owner starting its resting loop.
    /// </summary>
    internal sealed record Cries : CeresBabyCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(CeresBabyCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Cries(this, context);
    }
}

/// <summary>What each case of a <see cref="CeresBabyCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the cue is made into.</typeparam>
/// <param name="ReactsToMotion">Makes a <see cref="CeresBabyCue.ReactsToMotion"/> into the result.</param>
/// <param name="UpdateColors">Makes a <see cref="CeresBabyCue.UpdateColors"/> into the result.</param>
/// <param name="Cries">Makes a <see cref="CeresBabyCue.Cries"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record CeresBabyCueCases<TContext, TResult>(Func<CeresBabyCue.ReactsToMotion, TContext, TResult> ReactsToMotion, Func<CeresBabyCue.UpdateColors, TContext, TResult> UpdateColors, Func<CeresBabyCue.Cries, TContext, TResult> Cries);
