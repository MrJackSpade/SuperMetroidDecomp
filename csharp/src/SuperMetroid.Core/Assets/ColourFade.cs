namespace SuperMetroid.Core.Assets;

/// <summary>
/// A fade toward one colour (#1512): at step <see cref="Step"/> of <see cref="Steps"/>, each channel has moved that
/// fraction of the way, rounding toward the colour shown.
/// </summary>
public sealed record ColourFade : ColourEffect
{
    /// <summary>Keeps a fade's colour and progress.</summary>
    /// <param name="toward">The colour the fade ends in.</param>
    /// <param name="step">How far the fade has gone, from zero (no change) to <paramref name="steps"/> (all <paramref name="toward"/>).</param>
    /// <param name="steps">The fade's steps, at least one.</param>
    public ColourFade(RgbColor toward, int step, int steps)
    {
        Steps = Ensure.BetweenInclusive(1, int.MaxValue, steps);
        Step = Ensure.BetweenInclusive(0, steps, step);
        Toward = toward;
    }

    /// <summary>The colour the fade ends in.</summary>
    public RgbColor Toward { get; }

    /// <summary>How far the fade has gone.</summary>
    public int Step { get; }

    /// <summary>The fade's steps.</summary>
    public int Steps { get; }

    /// <inheritdoc />
    public override RgbColor Apply(RgbColor shown) =>
        shown.Zip(Toward, (_, from, to) => from + (to - from) * Step / Steps);
}
