using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>
/// Gameplay's colours as a check reads them back by position, with Samus's held by her own colours (#1405): her
/// effects write <see cref="Samus"/>, and the check reads every colour, hers among them, through the router.
/// </summary>
internal sealed class SamusColourRig : IPaletteColors, IPaletteFxColourOwners
{
    /// <summary>Routes Samus's positions to <see cref="Samus"/>.</summary>
    private readonly GameplayColours colours;

    /// <summary>Creates fresh colours, Samus's black like the rest.</summary>
    public SamusColourRig()
    {
        var spriteSets = new SpriteColourSets();
        Samus = new SamusColours(spriteSets.Set(SamusColourDefinitions.SuitlessSpriteSet));
        colours = new GameplayColours(spriteSets, Samus);
    }

    /// <summary>Samus's colours, which her effects write.</summary>
    public SamusColours Samus { get; }

    /// <inheritdoc />
    public ColourRun Colours(PaletteFxColourOwner owner) => new GameplayPaletteFxColours(colours, Samus).Colours(owner);

    /// <summary>The router, for what takes gameplay's colours.</summary>
    public GameplayColours Gameplay => colours;

    /// <inheritdoc />
    public ReadOnlySpan<Bgr555> Colors => colours.Colors;

    /// <inheritdoc />
    public void SetColor(int index, Bgr555 color) => colours.SetColor(index, color);

    /// <inheritdoc />
    public Rgba32 GetRgba(int index) => colours.GetRgba(index);

    /// <inheritdoc />
    public void LoadBytes(ReadOnlySpan<byte> bytes, int destinationIndex = 0) => colours.LoadBytes(bytes, destinationIndex);
}
