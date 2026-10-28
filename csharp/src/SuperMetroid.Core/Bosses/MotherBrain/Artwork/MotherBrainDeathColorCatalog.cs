using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Editable Mother Brain death-fade and exploded-door RGB5 images.</summary>
public sealed class MotherBrainDeathColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => MotherBrainDeathColors.ContentIdentity(this);

    /// <summary>The selected body, leg and corpse fades and exploded door.</summary>
    internal readonly MotherBrainDeathColorSelection selection;

    /// <summary>Compresses stock fade images into calculated stages while retaining independently edited frames.</summary>
    internal MotherBrainDeathColorCatalog(RgbColor[][] bodyFade, RgbColor[][] legFade,
        RgbColor[][] corpseFade, RgbColor[] explodedDoor) =>
        selection = new(new MotherBrainDeathFade(bodyFade, MotherBrainFadeTarget.Black),
            new MotherBrainDeathFade(legFade, MotherBrainFadeTarget.Black, MotherBrainPalettePart.BackLeg),
            new MotherBrainDeathFade(corpseFade, MotherBrainFadeTarget.DrainedBody),
            MotherBrainDeathColors.EditedDoor(explodedDoor));

    /// <summary>Returns one body-fade color from the first segment of the native $AD:EA0A images, applied to both CGRAM 65..78 and 145..158 during the death fade.</summary>
    /// <param name="frame">Zero-based authored palette stage 0..15, not an elapsed gameplay-update count; the stock sequence ends at black.</param>
    /// <param name="color">Zero-based color within the fourteen-color body segment, 0..13.</param>
    /// <returns>Packed SNES BGR555 color, preserving any independently edited stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The stage or color index is outside the authored images.</exception>
    public RgbColor BodyColor(int frame, int color) =>
        selection.BodyFade.Checked(frame, color, nameof(BodyColor));

    /// <summary>Returns one back-leg fade color from the second fourteen-color segment of each native $AD:EA0A image, applied to CGRAM 177..190.</summary>
    /// <param name="frame">Zero-based authored palette stage 0..15; the stock back-leg sequence fades to black alongside the body.</param>
    /// <param name="color">Zero-based color within the back-leg segment, 0..13.</param>
    /// <returns>Packed SNES BGR555 color from the selected back-leg image.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The stage or color index is outside the authored images.</exception>
    public RgbColor LegColor(int frame, int color) =>
        selection.LegFade.Checked(frame, color, nameof(LegColor));

    /// <summary>Returns one detached-head corpse color from the native $AD:F119 fade-to-gray images, applied to CGRAM 241..255 before corpse rotting.</summary>
    /// <param name="frame">Zero-based authored palette stage 0..7, progressing from the death-start colors to the drained corpse appearance.</param>
    /// <param name="color">Zero-based opaque color in sprite palette seven, 0..14; transparent color zero is omitted.</param>
    /// <returns>Packed SNES BGR555 color from the selected corpse image.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The stage or color index is outside the authored images.</exception>
    public RgbColor CorpseColor(int frame, int color) =>
        selection.CorpseFade.Checked(frame, color, nameof(CorpseColor));

    /// <summary>Returns a color from the $A9:9534 exploded escape-door palette, installed at CGRAM 145..158 when the escape sequence opens the door.</summary>
    /// <param name="color">Zero-based palette color 0..13, corresponding to native sprite-palette-one colors 1..14.</param>
    /// <returns>Packed SNES BGR555 color from the selected fourteen-color image.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The color index is outside 0..13.</exception>
    public RgbColor ExplodedDoorColor(int color) => MotherBrainDeathColors.ExplodedDoorColor(selection.ExplodedDoor, color);
}
