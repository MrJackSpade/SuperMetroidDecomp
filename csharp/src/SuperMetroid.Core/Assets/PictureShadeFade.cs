namespace SuperMetroid.Core.Assets;

/// <summary>
/// A fade between two shadings of a picture (#2215), a colour effect a draw publishes as a shader parameter: each of the
/// picture's colours shows partway from what <paramref name="From"/> shows over the colours shaded so far to what
/// <paramref name="To"/> shows over them, <paramref name="Progress"/> of the way, as the backend blends a fade (#2294, #2563).
/// A fade started while another runs starts from that fade: its <paramref name="From"/> holds it.
/// </summary>
/// <param name="From">The shading the fade starts from, laid over the colours shaded so far.</param>
/// <param name="To">The shading the fade ends on, laid over the colours shaded so far.</param>
/// <param name="Progress">How far the fade has come, from showing <paramref name="From"/> to showing <paramref name="To"/>.</param>
public sealed partial record PictureShadeFade(PictureShading From, PictureShading To, FadeLevel Progress)
{
    /// <summary>The shading the fade starts from.</summary>
    public PictureShading From { get; } = Ensure.NotNull(From);

    /// <summary>The shading the fade ends on.</summary>
    public PictureShading To { get; } = Ensure.NotNull(To);

    /// <summary>
    /// Shows the fade over <paramref name="shown"/>, the colour each of <paramref name="picture"/>'s colour keys shows so
    /// far, by key: each colour shows partway from its colour under <see cref="From"/> toward its colour under
    /// <see cref="To"/>, as <paramref name="blend"/> blends a colour's fade (<see cref="ColourFade"/>).
    /// </summary>
    /// <exception cref="InvalidDataException">The installation did not pre-colour the picture in a shade either shading names.</exception>
    internal void ApplyOver(SpriteImage picture, Dictionary<ColourKey, RgbColor> shown, IColourBlend blend)
    {
        Ensure.NotNull(blend);
        var from = new Dictionary<ColourKey, RgbColor>(shown);
        var to = new Dictionary<ColourKey, RgbColor>(shown);
        From.ApplyOver(picture, from, blend);
        To.ApplyOver(picture, to, blend);
        foreach (ColourKey key in picture.ColourKeys)
            shown[key] = blend.Partway(from[key], to[key], Progress);
    }
}
