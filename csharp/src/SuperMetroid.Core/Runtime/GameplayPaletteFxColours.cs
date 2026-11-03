using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Runtime;

/// <summary>The gameplay colours palette-FX objects write (#1405, #1512): the sprite sets, and Samus's suit and beams.</summary>
/// <param name="colours">Gameplay's colour sets.</param>
internal sealed class GameplayPaletteFxColours(GameplayColours colours) : IPaletteFxColourOwners
{
    /// <inheritdoc />
    public ColourRun ScreenColours(PaletteFxColours colours) =>
        throw new InvalidOperationException($"Gameplay holds no frontend screen's colours for {colours}.");

    /// <inheritdoc />
    public ColourRun SpriteSets => colours.SpriteSets.All;

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    /// Always: Samus's suit is a shade her colour state names (#1512); a palette effect on it lays a shade over it
    /// through <see cref="SamusColours.LayOverSuit"/>, which the palette-FX conversion supplies.
    /// </exception>
    public ColourRun SamusSuit => throw new InvalidOperationException(
        $"Samus's suit is shaded (#1512); palette FX lay a shade over it with {nameof(SamusColours)}.{nameof(SamusColours.LayOverSuit)}.");

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    /// Always: Samus's beams are a shade her colour state names (#1512); a palette effect on them lays a shade over them
    /// through <see cref="SamusColours.LayOverBeams"/>, which the palette-FX conversion supplies.
    /// </exception>
    public ColourRun SamusBeams => throw new InvalidOperationException(
        $"Samus's beams are shaded (#1512); palette FX lay a shade over them with {nameof(SamusColours)}.{nameof(SamusColours.LayOverBeams)}.");
}
