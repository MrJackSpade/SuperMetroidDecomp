using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>
/// Gameplay's colours as a check reads them back by position, with Samus's held by her own colours (#1405): her
/// effects write <see cref="Samus"/>, and the check reads every colour, hers among them, through the router.
/// </summary>
internal sealed class SamusColourRig : IPaletteColors, IPaletteFxColourOwners
{
    /// <summary>Creates fresh colours, Samus's black like the rest.</summary>
    public SamusColourRig()
    {
        var spriteSets = new SpriteColourSets();
        Samus = new SamusColours(spriteSets);
        Gameplay = new GameplayColours(spriteSets, Samus);
    }

    /// <summary>Samus's colours, which her effects write.</summary>
    public SamusColours Samus { get; }

    /// <inheritdoc />
    public ColourRun Colours(PaletteFxColourOwner owner) => new GameplayPaletteFxColours(Gameplay, Samus).Colours(owner);

    /// <summary>The router, for what takes gameplay's colours: it routes Samus's positions to <see cref="Samus"/>.</summary>
    public GameplayColours Gameplay { get; }

    /// <inheritdoc />
    public ReadOnlySpan<RgbColor> Colors => Gameplay.Colors;

    /// <inheritdoc />
    public void SetColor(int index, RgbColor color) => Gameplay.AsPaletteColors().SetColor(index, color);

    /// <summary>The colour at <paramref name="index"/> expanded to 8-bit RGBA.</summary>
    public RgbaPixel GetRgba(int index) => Gameplay.AsPaletteColors().GetRgba(index);

    /// <inheritdoc />
    public void LoadBytes(ReadOnlySpan<byte> bytes, int destinationIndex = 0) => Gameplay.AsPaletteColors().LoadBytes(bytes, destinationIndex);
}
