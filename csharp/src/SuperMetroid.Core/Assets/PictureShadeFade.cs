namespace SuperMetroid.Core.Assets;

/// <summary>
/// A fade between two shadings of a picture (#2215), a colour effect a draw publishes as a shader parameter: each of the
/// picture's colours shows partway from what <paramref name="From"/> shows over the colours shaded so far to what
/// <paramref name="To"/> shows over them, <paramref name="Progress"/> of <paramref name="Length"/> of the way. A fade
/// started while another runs starts from that fade: its <paramref name="From"/> holds it.
/// </summary>
/// <param name="From">The shading the fade starts from, laid over the colours shaded so far.</param>
/// <param name="To">The shading the fade ends on, laid over the colours shaded so far.</param>
/// <param name="Progress">How far the fade has come, zero (showing <paramref name="From"/>) through <paramref name="Length"/> (showing <paramref name="To"/>).</param>
/// <param name="Length">The fade's length, one or more.</param>
public sealed record PictureShadeFade(PictureShading From, PictureShading To, int Progress, int Length)
{
    /// <summary>The fade's length, one or more.</summary>
    public int Length { get; } = Ensure.BetweenInclusive(1, int.MaxValue, Length);

    /// <summary>How far the fade has come, zero through <see cref="Length"/>.</summary>
    public int Progress { get; } = Ensure.BetweenInclusive(0, Length, Progress);

    /// <summary>The shading the fade starts from.</summary>
    public PictureShading From { get; } = Ensure.NotNull(From);

    /// <summary>The shading the fade ends on.</summary>
    public PictureShading To { get; } = Ensure.NotNull(To);

    /// <summary>
    /// Shows the fade over <paramref name="shown"/>, the colour each of <paramref name="picture"/>'s colour keys shows so
    /// far: each channel moves from its colour under <see cref="From"/> toward its colour under <see cref="To"/> by the
    /// fade's progress, truncated to a whole level.
    /// </summary>
    /// <exception cref="InvalidDataException">The installation did not pre-colour the picture in a shade either shading names.</exception>
    internal void ApplyOver(SpriteImage picture, Span<RgbColor> shown)
    {
        var from = new RgbColor[shown.Length];
        var to = new RgbColor[shown.Length];
        shown.CopyTo(from);
        shown.CopyTo(to);
        From.ApplyOver(picture, from);
        To.ApplyOver(picture, to);
        for (int key = 0; key < shown.Length; key++)
        {
            shown[key] = new RgbColor(
                Channel(from[key].Red, to[key].Red), Channel(from[key].Green, to[key].Green), Channel(from[key].Blue, to[key].Blue));
        }
    }

    /// <summary>One channel partway from <paramref name="from"/> to <paramref name="to"/>.</summary>
    private int Channel(int from, int to) => from + ((to - from) * Progress / Length);
}
