using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>
/// The shades the import pre-colours Phantoon's body sheet and his room's pictures in (#1512): each settled colouring of
/// his body, each run of steps of a fade from one toward another over each length his fades use, and each step of the Wrecked Ship's
/// power coming on over the room once he dies.
/// </summary>
public static class PhantoonShades
{
    /// <summary>The lengths his body's fades run over.</summary>
    public static IReadOnlyList<int> FadeDenominators { get; } = [1, 12];

    /// <summary>The length of the power coming on.</summary>
    public const int PowerOnDenominator = 12;

    /// <summary>His body showing <paramref name="look"/>.</summary>
    public static PictureShade Body(PhantoonBodyLook look) => new($"phantoon-body-{look.Key}", 0);

    /// <summary>
    /// His body after the steps of values <paramref name="first"/> through <paramref name="last"/> of a fade toward
    /// <paramref name="to"/> over <paramref name="denominator"/> steps, taken from <paramref name="from"/> as shown.
    /// </summary>
    /// <param name="from">The colouring shown when the fade took its first step.</param>
    /// <param name="to">The colouring the fade runs toward.</param>
    /// <param name="denominator">The fade's length.</param>
    /// <param name="first">The value of the fade's first moving step, one or more.</param>
    /// <param name="last">The value of its last step.</param>
    public static PictureShade Fading(PhantoonBodyLook from, PhantoonBodyLook to, int denominator, int first, int last) =>
        new($"phantoon-body-{from.Key}-to-{to.Key}-over-{denominator}-from-{Ensure.BetweenInclusive(1, last, first)}", last);

    /// <summary>The room after the step of value <paramref name="numerator"/> of the Wrecked Ship's power coming on.</summary>
    public static PictureShade PowerOn(int numerator) => new("phantoon-power-on", numerator);
}
