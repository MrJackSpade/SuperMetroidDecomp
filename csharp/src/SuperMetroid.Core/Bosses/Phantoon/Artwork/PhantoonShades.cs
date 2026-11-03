using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>
/// The shades the import pre-colours Phantoon's body sheet and his room's pictures in (#1512): each settled colouring of
/// his body, each step of a fade from one to another over each length his fades use, and each step of the Wrecked Ship's
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
    /// His body after the step of value <paramref name="numerator"/> of a fade from <paramref name="from"/> toward
    /// <paramref name="to"/> over <paramref name="denominator"/> steps.
    /// </summary>
    public static PictureShade Fading(PhantoonBodyLook from, PhantoonBodyLook to, int denominator, int numerator) =>
        new($"phantoon-body-{from.Key}-to-{to.Key}-over-{denominator}", numerator);

    /// <summary>The room after the step of value <paramref name="numerator"/> of the Wrecked Ship's power coming on.</summary>
    public static PictureShade PowerOn(int numerator) => new("phantoon-power-on", numerator);
}
