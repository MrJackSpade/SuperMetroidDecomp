using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>
/// Gameplay's owner-keyed colours with Samus's own, as a check reads and writes them by native position (#1405, #1464):
/// her effects write <see cref="Samus"/>, and the check addresses every colour, hers among them, at the native boundary.
/// A read or write of one position touches only the owner colour it decodes to; <see cref="Colors"/> builds the whole
/// native image from the owners.
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

    /// <summary>Fresh colours holding <paramref name="image"/>, a native 256-colour image, as the cartridge's initial load leaves them.</summary>
    [WholeImageFixture("gameplay's colours from a 256-colour native colour image")]
    public static SamusColourRig Starting(ReadOnlySpan<RgbColor> image)
    {
        var rig = new SamusColourRig();
        rig.Gameplay.LoadStartingImage(image);
        return rig;
    }

    /// <summary>Samus's colours, which her effects write.</summary>
    public SamusColours Samus { get; }

    /// <inheritdoc />
    public ColourRun Colours(PaletteFxColourOwner owner) => new GameplayPaletteFxColours(Gameplay, Samus).Colours(owner);

    /// <summary>Gameplay's colours, which hold Samus's as one of their owners.</summary>
    public GameplayColours Gameplay { get; }

    /// <inheritdoc />
    public ReadOnlySpan<RgbColor> Colors => Gameplay.Colors;

    /// <inheritdoc />
    public void SetColor(int index, RgbColor color) => Gameplay.AtPosition(index).Write(0, color);

    /// <summary>The colour at <paramref name="index"/> expanded to 8-bit RGBA.</summary>
    public RgbaPixel GetRgba(int index) => Gameplay.AtPosition(index)[0].ToRgbaPixel();

    /// <inheritdoc />
    public void LoadBytes(ReadOnlySpan<byte> bytes, int destinationIndex = 0) => Gameplay.AsPaletteColors().LoadBytes(bytes, destinationIndex);
}
