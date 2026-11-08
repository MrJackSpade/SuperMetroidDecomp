namespace SuperMetroid.Core.Assets;

/// <summary>
/// A fade toward one colour (#1512, #1544): at step <see cref="Step"/> of <see cref="Steps"/>, the colour shown is blended
/// that fraction of the way toward <see cref="Toward"/> (<see cref="RgbColor.Blend"/>).
/// </summary>
public sealed record ColourFade : ColourEffect
{

    /// <summary>The colour the fade ends in.</summary>
    public RgbColor Toward { get; }

    /// <summary>How far the fade has gone.</summary>
    public int Step { get; }

    /// <summary>The fade's steps.</summary>
    public int Steps { get; }

    /// <inheritdoc />
    public override RgbColor Apply(RgbColor shown) =>
        shown.Blend(Toward, Step, Steps);
}
