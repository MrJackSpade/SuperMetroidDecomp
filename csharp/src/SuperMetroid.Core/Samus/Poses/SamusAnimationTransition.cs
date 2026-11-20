namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Publishes <paramref name="Pose"/> as the transitional pose Samus takes once the animation ends.</summary>
/// <param name="Pose">The pose the animation ends in.</param>
internal sealed record SamusAnimationTransition(SamusPoseId Pose) : SamusAnimationCue
{
    /// <inheritdoc/>
    internal override TResult Match<TContext, TResult>(SamusAnimationCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Transition(this, context);

    /// <inheritdoc/>
    internal override IReadOnlyList<SamusPoseId> PublishedPoses => [Pose];
}
