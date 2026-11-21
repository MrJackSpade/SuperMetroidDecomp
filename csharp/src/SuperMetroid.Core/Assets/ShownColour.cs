namespace SuperMetroid.Core.Assets;

/// <summary>
/// A colour a frame shows that the backend works out (#2563): a plain colour; a picture's backdrop colour under the shading
/// its pictures show, as a room's backdrop shows under the room's fades; or a colour at a level of a fade between black and
/// itself, as the opening's backdrop shows under its crossfade and a white wash as it fades. The simulation names it; the
/// backend shows it, blending any fade (<see cref="ShownWith"/>).
/// </summary>
public abstract partial record ShownColour
{
    /// <summary>Only the cases below name a shown colour.</summary>
    private ShownColour()
    {
    }

    /// <summary>The colour shown, with its fades blended by <paramref name="blend"/>.</summary>
    /// <param name="blend">The backend's blending of a fade's colours.</param>
    /// <exception cref="InvalidDataException">The picture's installation did not pre-colour it in a shade its shading names.</exception>
    public abstract RgbColor ShownWith(IColourBlend blend);

    /// <summary>A shown colour of one plain colour.</summary>
    public static implicit operator ShownColour(RgbColor colour) => FromRgbColor(colour);

    /// <summary>A shown colour of <paramref name="colour"/> alone.</summary>
    /// <param name="colour">The colour shown.</param>
    public static ShownColour FromRgbColor(RgbColor colour) => new Plain(colour);

    /// <summary>One plain colour.</summary>
    /// <param name="Colour">The colour shown.</param>
    public sealed partial record Plain(RgbColor Colour) : ShownColour
    {
        /// <inheritdoc />
        public override RgbColor ShownWith(IColourBlend blend) => Colour;
    }

    /// <summary>A picture's colour of one key under a shading, as a room's backdrop shows under the room's shading and fades.</summary>
    /// <param name="Picture">The picture naming the colour.</param>
    /// <param name="Key">The picture's colour key shown.</param>
    /// <param name="Shading">The shading the picture shows in.</param>
    public sealed partial record OfPicture(SpriteImage Picture, ColourKey Key, PictureShading Shading) : ShownColour
    {
        /// <inheritdoc />
        public override RgbColor ShownWith(IColourBlend blend) => Shading.Shown(Picture, Key, blend);
    }

    /// <summary>A colour shown at a level of a fade between black and itself.</summary>
    /// <param name="Colour">The colour at its whole level.</param>
    /// <param name="Level">How far up from black it shows.</param>
    public sealed partial record Faded(RgbColor Colour, FadeLevel Level) : ShownColour
    {
        /// <inheritdoc />
        public override RgbColor ShownWith(IColourBlend blend) => Ensure.NotNull(blend).AtLevel(Colour, Level);
    }
}
