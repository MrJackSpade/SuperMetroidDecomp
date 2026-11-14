using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Bosses.Torizo.Artwork;

/// <summary>
/// The shades the import pre-colours the Torizos' sheets in (#1512): the orb, flame and statue-shading colours both load,
/// each settled colouring of a Torizo's body and belly, and each run of counter values a fade takes from one toward another.
/// </summary>
public static class TorizoShades
{
    /// <summary>The orb, flame and statue-shading colours both Torizos load.</summary>
    public static PictureShade Shared { get; } = new("torizo-shared", 0);

    /// <summary>A Torizo's body and belly showing <paramref name="look"/>.</summary>
    public static PictureShade Body(TorizoLook look) => new($"torizo-{look.Key}", 0);

    /// <summary>
    /// A Torizo's body and belly after the steps of counter values <paramref name="first"/> through
    /// <paramref name="last"/> of a fade toward <paramref name="to"/>, taken from <paramref name="from"/> as shown.
    /// </summary>
    /// <param name="from">The colouring shown when the fade took its first step.</param>
    /// <param name="to">The colouring the fade runs toward.</param>
    /// <param name="first">The counter value of the fade's first moving step, one or more.</param>
    /// <param name="last">The counter value of its last step.</param>
    public static PictureShade Fading(TorizoLook from, TorizoLook to, int first, int last) =>
        new($"torizo-{from.Key}-to-{to.Key}-from-{Ensure.BetweenInclusive(1, last, first)}", last);

    /// <summary>
    /// A Torizo's glowing belly colours after the steps of counter values <paramref name="first"/> through
    /// <paramref name="last"/> of a fade toward <paramref name="to"/>, taken from the belly glow's
    /// <paramref name="glow"/> as shown.
    /// </summary>
    /// <param name="glow">The glow's frame shown when the fade took its first step over it.</param>
    /// <param name="to">The colouring the fade runs toward.</param>
    /// <param name="first">The counter value of the fade's first moving step over the glow, one or more.</param>
    /// <param name="last">The counter value of the fade's last step.</param>
    public static PictureShade BellyFading(ColourEffectFrame glow, TorizoLook to, int first, int last) =>
        new($"torizo-belly-{glow.Animation}-{glow.Frame}-to-{to.Key}-from-{Ensure.BetweenInclusive(1, last, first)}", last);
}
