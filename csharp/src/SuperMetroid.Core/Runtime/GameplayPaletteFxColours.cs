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
    public ColourRun Colours(PaletteFxColourOwner owner) => owner switch
    {
        PaletteFxColourOwner.Background => colours.BackgroundSets.All,
        PaletteFxColourOwner.Sprite => colours.SpriteSets.All,
        PaletteFxColourOwner.SamusSuit => samus.Suit,
        PaletteFxColourOwner.SamusBeams => samus.Beam,
        _ => throw new ArgumentOutOfRangeException(nameof(owner), owner, "Unknown palette-FX colour owner."),
    };
}
