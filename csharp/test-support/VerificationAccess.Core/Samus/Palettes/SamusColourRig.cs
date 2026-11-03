using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>
/// Gameplay's owner-keyed colours beside Samus's colour state, as a check reads and writes them by native position (#1405,
/// #1464, #1512): her effects name shades in <see cref="Samus"/>, which a check resolves through the import's shades, and
/// the check addresses every gameplay colour at the native boundary. Samus's colours are no longer among gameplay's, so
/// the native positions of her suit and beams address gameplay's sprite sets there, which only other sprites show.
/// </summary>
internal sealed class SamusColourRig : IPaletteColors, IPaletteFxColourOwners
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

    /// <inheritdoc />
    /// <remarks>
    /// The rig stands for colour memory at the native boundary, where a screen's set and gameplay's background set of
    /// the same native position are the same colours, so a screen effect stepped on the rig writes the gameplay set the
    /// native position of the screen set holding its colours decodes to.
    /// </remarks>
    public ColourRun ScreenColours(PaletteFxColours colours)
    {
        ScreenColourPlacement screen = PaletteFxColoursPositions.ScreenPlacement(colours);
        // A screen's sprite set is the sprite set of the same native position, written from its first colour.
        return screen is ScreenSpriteSetPlacement
            ? Gameplay.SpriteSets.All.FromSet(screen.Set, screen.Colour)
            : Gameplay.BackgroundSets.Set(NativeBackgroundSets.AtNativeSet(screen.Set)).Slice(screen.Colour, screen.ColourCount);
    }

    /// <inheritdoc />
    public ColourRun SpriteSets => Gameplay.SpriteSets.All;

    /// <inheritdoc />
    /// <remarks>At the native boundary the suit's colours are gameplay's sprite set of their native position.</remarks>
    public ColourRun SamusSuit => Gameplay.SpriteSets.All.FromSet(SamusSuitNativeSet, 0).Slice(0, ColourSets.SetColours);

    /// <inheritdoc />
    /// <remarks>At the native boundary the beams' colours are gameplay's sprite set of their native position.</remarks>
    public ColourRun SamusBeams => Gameplay.SpriteSets.All.FromSet(SamusBeamsNativeSet, 0).Slice(0, ColourSets.SetColours);

    /// <summary>The native sprite set Samus's suit colours lie in.</summary>
    private const int SamusSuitNativeSet = 4;

    /// <summary>The native sprite set Samus's beam colours lie in.</summary>
    private const int SamusBeamsNativeSet = 6;

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
