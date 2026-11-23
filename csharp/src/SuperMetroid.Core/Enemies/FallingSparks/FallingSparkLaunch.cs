namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>
/// What a falling spark's random launch choice chooses (#2510): one of the authored drifts, or the roll past them, whose
/// drift <see cref="FallingSparkUnauthoredLaunch"/> supplies (<see cref="FallingSparkLaunchDefinitions.Launch"/>).
/// </summary>
internal abstract partial record FallingSparkLaunch
{
    /// <summary>Only the launches below exist.</summary>
    private FallingSparkLaunch()
    {
    }

    /// <summary>
    /// The sideways drift per update the spark launches with, consulting <paramref name="unauthoredLaunch"/> only for the
    /// roll past the authored drifts.
    /// </summary>
    /// <param name="unauthoredLaunch">Supplies the drift of the roll past the authored launches.</param>
    internal abstract SignedSixteenSixteen DriftWith(Func<FallingSparkUnauthoredLaunch> unauthoredLaunch);

    /// <summary>An authored launch, drifting by <paramref name="Drift"/>.</summary>
    /// <param name="Drift">The signed sideways drift per update.</param>
    internal sealed partial record Authored(SignedSixteenSixteen Drift) : FallingSparkLaunch
    {
        /// <inheritdoc/>
        internal override SignedSixteenSixteen DriftWith(Func<FallingSparkUnauthoredLaunch> unauthoredLaunch) => Drift;
    }

    /// <summary>The roll past the seven authored launches, drifting fast leftward by <see cref="FallingSparkUnauthoredLaunch"/>'s drift.</summary>
    internal sealed partial record PastAuthored : FallingSparkLaunch
    {
        /// <summary>The one roll past the authored launches.</summary>
        internal static PastAuthored Instance { get; } = new();

        /// <summary>Only <see cref="Instance"/> exists.</summary>
        private PastAuthored()
        {
        }

        /// <inheritdoc/>
        internal override SignedSixteenSixteen DriftWith(Func<FallingSparkUnauthoredLaunch> unauthoredLaunch) =>
            Ensure.NotNull(unauthoredLaunch)().Drift;
    }
}
