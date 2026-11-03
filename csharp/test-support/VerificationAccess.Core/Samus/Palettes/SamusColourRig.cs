using SuperMetroid.Core.Assets;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>
/// Gameplay's owner-keyed colours beside Samus's colour state, as a check reads and writes them by native position (#1405,
/// #1464, #1512): her effects name shades in <see cref="Samus"/>, which a check resolves through the import's shades, and
/// the check addresses every gameplay colour at the native boundary. Samus's colours are no longer among gameplay's, so
/// the native positions of her suit and beams address gameplay's sprite sets there, which only other sprites show.
/// </summary>
internal sealed class SamusColourRig : IPaletteColors
{
    /// <summary>Creates fresh colours, gameplay's black and Samus's in their starting shades.</summary>
    public SamusColourRig()
    {
        Samus = new SamusColours();
        Gameplay = new GameplayColours(new SpriteColourSets());
    }

    /// <summary>Fresh colours holding <paramref name="image"/>, a native 256-colour image, as the cartridge's initial load leaves them.</summary>
    [WholeImageFixture("gameplay's colours from a 256-colour native colour image")]
    public static SamusColourRig Starting(ReadOnlySpan<RgbColor> image)
    {
        var rig = new SamusColourRig();
        rig.Gameplay.LoadStartingImage(image);
        return rig;
    }

    /// <summary>Samus's colour state, which her effects set.</summary>
    public SamusColours Samus { get; }

    /// <inheritdoc />
    public ColourRun BackgroundSet(BackgroundColourSet set) => Gameplay.BackgroundSets.Set(set);

    /// <summary>Every sprite set's colours, set after set.</summary>
    public ColourRun SpriteSets => Gameplay.SpriteSets.All;

    /// <summary>Gameplay's colours, which no longer hold Samus's.</summary>
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
