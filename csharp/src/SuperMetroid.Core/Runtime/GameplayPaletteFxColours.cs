using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Runtime;

/// <summary>The gameplay colours palette-FX objects write (#1405): the background and sprite sets, and Samus's suit and beams.</summary>
/// <param name="colours">Gameplay's colour sets.</param>
/// <param name="samus">Samus's colours.</param>
internal sealed class GameplayPaletteFxColours(GameplayColours colours, SamusColours samus) : IPaletteFxColourOwners
{
    /// <inheritdoc />
    public ColourRun BackgroundSet(BackgroundColourSet set) => colours.BackgroundSets.Set(set);

    /// <inheritdoc />
    public ColourRun ScreenBackgroundSet(int set) =>
        throw new InvalidOperationException($"Gameplay names each background set; it has no frontend screen's set {set}.");

    /// <inheritdoc />
    public ColourRun SpriteSets => colours.SpriteSets.All;

    /// <inheritdoc />
    public ColourRun SamusSuit => samus.Suit;

    /// <inheritdoc />
    public ColourRun SamusBeams => samus.Beam;
}
